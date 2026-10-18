using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(Collider))]
public class Trap : MonoBehaviour
{
    [Header("検出")]
    public float detectionRadius = 1.0f;
    public LayerMask monsterLayer;
    public string monsterTag = "Monster";

    [Header("捕獲ムービー")]
    public Animator trapAnimator;
    public string captureTriggerName = "Capture";

    [Header("動作")]
    public float postCaptureDelay = 1.0f;
    public bool destroyOnCapture = true;

    [Header("設置時")]
    public bool startEnabled = false;

    bool isActive = false;
    bool isCapturing = false;
    Collider col;

    void Reset()
    {
        col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col != null)
            col.enabled = startEnabled;
        isActive = startEnabled;
        // TrapManager への登録は行わない（currentTrapCount で管理するため）
    }

    // Animation Event から呼ぶ
    public void OnPlacementAnimationEvent()
    {
        EnableTrap();
    }

    public void EnableTrap()
    {
        if (isActive) return;
        isActive = true;
        if (col != null)
            col.enabled = true;
        // TrapManager への登録は行わない（currentTrapCount で管理するため）
    }

    public void Initialize()
    {
        if (startEnabled)
            EnableTrap();
        else
        {
            if (col != null)
                col.enabled = false;
            isActive = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isCapturing) return;
        if (((1 << other.gameObject.layer) & monsterLayer) == 0 && !other.CompareTag(monsterTag)) return;

        // UniTask で非同期捕獲処理を実行（fire-and-forget）
        CaptureRoutine(other.gameObject).Forget();
    }

    async UniTask CaptureRoutine(GameObject monster)
    {
        isCapturing = true;
        isActive = false;

        var im = monster.GetComponent<IMonster>();
        if (im != null)
        {
            im.OnCaptured();
        }
        else
        {
            var monsterAnimator = monster.GetComponent<Animator>();
            if (monsterAnimator != null)
                monsterAnimator.SetTrigger("Captured");

            var mover = monster.GetComponent<MonoBehaviour>();
            if (mover != null)
                mover.enabled = false;
        }

        if (trapAnimator != null && !string.IsNullOrEmpty(captureTriggerName))
            trapAnimator.SetTrigger(captureTriggerName);

        await UniTask.Delay(TimeSpan.FromSeconds(postCaptureDelay));

        if (destroyOnCapture)
        {
            if (monster != null)
                Destroy(monster);
        }

        // PlayerTrapPlacer の設置数をデクリメント（存在する場合のみ）
        GameObject placer = FindGameObjectWithTag("Player");
         placer.GetComponent<PlaceTrap>().currentTrapCount--;

    
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public bool IsActive => isActive;
}