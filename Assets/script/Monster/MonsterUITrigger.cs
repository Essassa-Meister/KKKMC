using UnityEngine;
using UnityEngine.UI;

public class MonsterUITrigger : MonoBehaviour
{
    [Tooltip("UI GameObject to enable/disable (例: Canvas の Root)")]
    public GameObject uiRoot;

    [Tooltip("Player の Tag (デフォルト: Player)")]
    public string playerTag = "Player";

    [Tooltip("任意: HP を表示する Slider (なくても動作します)")]
    public Slider hpSlider;

    private Status status;

    void Start()
    {
        if (uiRoot != null) uiRoot.SetActive(false);
        status = GetComponent<Status>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (uiRoot != null) uiRoot.SetActive(true);

        if (status != null && hpSlider != null)
        {
            // 初期表示更新とイベント購読
            hpSlider.value = Mathf.Clamp01(status.currentHp / status.maxHp);
            status.OnHpChanged += OnHpChanged;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (uiRoot != null) uiRoot.SetActive(false);

        if (status != null && hpSlider != null)
        {
            status.OnHpChanged -= OnHpChanged;
        }
    }

    private void OnHpChanged(float currentHp, float maxHp)
    {
        if (hpSlider == null) return;
        if (maxHp <= 0f) hpSlider.value = 0f;
        else hpSlider.value = Mathf.Clamp01(currentHp / maxHp);
    }
}