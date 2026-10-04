using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class FrdMonsterAction : MonoBehaviour
{
    [SerializeField] private float walkRadius = 10f;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private LayerMask foodLayer; // LayerMask にするとインスペクターで層を選びやすくなります

    public Transform target;
    public NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // GameObject 破棄時に非同期処理を安全にキャンセルできるようにトークンを渡す
        ActionLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void Update()
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private async UniTaskVoid ActionLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 0 または 1 をランダムに選ぶ（Random.Range の整数オーバーロードは最大値が含まれません）
            await ActionPatterns(Random.Range(0, 2), token);
            await UniTask.WaitForSeconds(3f, cancellationToken: token);
        }
    }

    protected UniTask ActionPatterns(int index, CancellationToken token)
    {
        return index switch
        {
            0 => RandomWalk(token),
            1 => Eat(token),
            _ => throw new System.ArgumentOutOfRangeException(nameof(index))
        };
    }

    private async UniTask RandomWalk(CancellationToken token)
    {
        int maxTries = 10;

        for (int i = 0; i < maxTries; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * walkRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);

                    // 【CS1998解消】目的地に到着するまで await で待機する
                    await UniTask.WaitUntil(() =>
                    {
                        if (agent == null) return true;
                        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
                    }, cancellationToken: token);

                    break; // 移動が完了したらリトライループを抜ける
                }
            }
        }
    }

    private async UniTask Eat(CancellationToken token)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, foodLayer);

        if (hits.Length == 0)
        {
            Debug.Log("範囲内に食べ物が見つかりませんでした。");
            return;
        }

        int randomIndex = Random.Range(0, hits.Length);
        Transform targetFood = hits[randomIndex].transform;

        agent.SetDestination(targetFood.position);

        await UniTask.WaitUntil(() =>
        {
            if (targetFood == null || agent == null) return true;
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        }, cancellationToken: token);

        Debug.Log($"ランダムに選んだ食べ物 ({targetFood.name}) に到着しました！");

        // ※ agent = null; を削除（null にすると次回から参照エラーで動かなくなります）
        await UniTask.Delay(10000, cancellationToken: token);
    }
}