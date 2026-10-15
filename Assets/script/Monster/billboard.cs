using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;          // 未指定なら MainCamera を自動取得
    public bool lockY = true;            // Y 軸だけは固定して上下回転を防ぐ
    public float smoothSpeed = 0f;       // 0 = 即時回転、>0 = 補間

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 direction = targetCamera.transform.position - transform.position;

        // Y 軸だけ固定（頭上に常に水平に表示したい場合）
        if (lockY) direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);

        if (smoothSpeed > 0f)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        else
            transform.rotation = targetRot;
    }
}
