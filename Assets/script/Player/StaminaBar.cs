using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Image fillImage; // 前景 Image。Image.Type = Filled, Fill Method = Horizontal に設定
    public Status status;   // 指定がなければ親から取得

    void Awake()
    {
        if (status == null) status = GetComponentInParent<Status>();
    }

    void OnEnable()
    {
        if (status != null) status.OnStaminaChanged += OnStaminaChanged;
    }

    void OnDisable()
    {
        if (status != null) status.OnStaminaChanged -= OnStaminaChanged;
    }

    void Start()
    {
        if (status != null) UpdateFill(status.currentStamina, status.maxStamina);
    }

    void OnStaminaChanged(float current, float max)
    {
        UpdateFill(current, max);
    }

    void UpdateFill(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(max <= 0 ? 0 : current / max);
    }
}