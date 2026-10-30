using UnityEngine;

[ExecuteInEditMode]
public class WorldSpaceConstantSize : MonoBehaviour
{
    public Camera targetCamera;
    public enum Mode { DistanceFactor, PixelHeight }
    public Mode mode = Mode.DistanceFactor;

    [Header("DistanceFactor mode")]
    public float scaleFactor = 0.02f; // 調整用。distance * scaleFactor が最終スケール

    [Header("PixelHeight mode")]
    public float desiredPixelHeight = 80f; // 画面上での高さ（ピクセル）
    public float referencePixelsPerUnit = 100f; // Canvas の Pixels Per Unit に合わせる

    void Reset()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // カメラに常に正対
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);

        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);

        if (mode == Mode.DistanceFactor)
        {
            float s = Mathf.Max(0.0001f, distance * scaleFactor);
            transform.localScale = Vector3.one * s;
        }
        else // PixelHeight
        {
            // パースペクティブカメラ想定。1ピクセルあたりのワールド高さを計算
            float worldHeightPerPixel = 2f * distance * Mathf.Tan(0.5f * targetCamera.fieldOfView * Mathf.Deg2Rad) / Screen.height;
            float desiredWorldHeight = worldHeightPerPixel * desiredPixelHeight;

            // RectTransform がある場合はその高さ（ピクセル）を使ってスケール計算
            var rt = GetComponent<RectTransform>();
            float elementPixelHeight = (rt != null) ? rt.rect.height : referencePixelsPerUnit; // rect.height はピクセル
            float elementUnitHeight = elementPixelHeight / referencePixelsPerUnit; // UIユニット換算
            float s = desiredWorldHeight / Mathf.Max(0.0001f, elementUnitHeight);
            transform.localScale = Vector3.one * s;
        }
    }
}