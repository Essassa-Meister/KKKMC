using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float dashMultiplier = 2.0f;
    [SerializeField] private float rotationSpeed = 720f; // 1秒間に回転する角度（度/秒）

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

        // 障害物等でプレイヤーが傾かないよう、X・Z軸の回転を固定
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
        // 物理移動および回転処理は FixedUpdate で実行
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 moveDir = new Vector3(rawInput.x, 0f, rawInput.y).normalized;

        bool canDash = status != null && status.currentStamina > minStaminaToDash;
        float currentSpeed = (wantsDash && canDash) ? walkSpeed * dashMultiplier : walkSpeed;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            // 1. カメラが見ている水平向きを取得し、画面上の入力方向（WASD）をワールド移動ベクトルに変換
            float cameraYaw = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
            Vector3 targetMoveDir = Quaternion.Euler(0f, cameraYaw, 0f) * moveDir;

            // 2. 一定速度（rotationSpeed）で目標の移動方向へスムーズに体を回転
            Quaternion targetRotation = Quaternion.LookRotation(targetMoveDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            // 3. 移動（重力速度を維持したまま、画面上の入力方向へ移動）
            Vector3 targetVelocity = targetMoveDir * currentSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            // 入力がない時は水平移動を即座に停止
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
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