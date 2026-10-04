using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float dashMultiplier = 2.0f;

    [Header("Input System 設定")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    [Header("スタミナ設定")]
    [SerializeField] private float staminaDrainPerSecond = 8f;
    [SerializeField] private float staminaRecoverPerSecond = 6f;
    [SerializeField] private float minStaminaToDash = 0.1f;

    private Rigidbody rb;
    private Status status;

    private Vector2 rawInput;
    private bool wantsDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // プレイヤーが障害物にぶつかって倒れないよう、X・Z軸の回転を固定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (!TryGetComponent(out status))
        {
            Debug.LogWarning($"{nameof(Status)} コンポーネントが見つかりません。", this);
        }
    }

    private void OnEnable()
    {
        moveAction?.action?.Enable();
        dashAction?.action?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action?.Disable();
        dashAction?.action?.Disable();
    }

    private void Update()
    {
        // 1. 入力値の取得（毎フレーム）
        rawInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        // 2. ダッシュ・スタミナの更新
        bool isInputting = rawInput.sqrMagnitude > 0.001f;
        wantsDash = dashAction != null && dashAction.action.IsPressed() && isInputting;
        bool canDash = status != null && status.currentStamina > minStaminaToDash;

        if (wantsDash && canDash)
        {
            UpdateStamina(-staminaDrainPerSecond);
        }
        else
        {
            UpdateStamina(staminaRecoverPerSecond);
        }
    }

    private void FixedUpdate()
    {
        // 物理計算に関する移動処理は FixedUpdate で実行する
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 moveDir = new Vector3(rawInput.x, 0f, rawInput.y).normalized;

        bool canDash = status != null && status.currentStamina > minStaminaToDash;
        float currentSpeed = (wantsDash && canDash) ? walkSpeed * dashMultiplier : walkSpeed;

        // プレイヤーの向き（ローカル座標）に合わせた移動ベクトル
        Vector3 targetVelocity = transform.TransformDirection(moveDir) * currentSpeed;

        // Y軸（重力による落下速度）は維持したまま、水平移動（X・Z軸）だけ速度を上書き
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private void UpdateStamina(float changePerSecond)
    {
        if (status == null) return;

        status.currentStamina = Mathf.Clamp(
            status.currentStamina + changePerSecond * Time.deltaTime,
            0f,
            status.maxStamina
        );
    }
}