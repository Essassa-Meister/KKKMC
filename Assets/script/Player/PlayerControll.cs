//プレイヤーのオブジェクトにStatusとCharacterControllerをアタッチする必要があります
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float dashMultiplier = 2.0f;

    [Header("スタミナ")]
    [Tooltip("Shiftでダッシュ中に1秒ごとに減る量")]
    [SerializeField] private float staminaDrainPerSecond = 8f;
    [Tooltip("非ダッシュ時の回復量（1秒あたり）")]
    [SerializeField] private float staminaRecoverPerSecond = 6f;
    [Tooltip("ダッシュ可能な最小スタミナ（0だとダッシュ不可）")]
    [SerializeField] private float minStaminaToDash = 0.1f;

    private UnityEngine.CharacterController controller;
    private Status status;

    void Start()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        status = GetComponent<Status>();
        if (status == null)
        {
            Debug.LogWarning("Status コンポーネントが見つかりません。スタミナ参照が必要です。");
        }
    }

    void Update()
    {
        // Input System に対応する入力読み取り（Gamepad 優先、なければ Keyboard）
        Vector2 raw2 = Vector2.zero;
        if (Gamepad.current != null)
        {
            raw2 = Gamepad.current.leftStick.ReadValue();
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) raw2.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) raw2.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) raw2.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) raw2.x += 1f;
        }

        Vector3 input = new Vector3(raw2.x, 0f, raw2.y);
        Vector3 moveDir = input.normalized;

        bool wantsDash = false;
        if (Gamepad.current != null)
        {
            wantsDash = (Gamepad.current.leftShoulder.isPressed || Gamepad.current.rightShoulder.isPressed) && moveDir.sqrMagnitude > 0.001f;
        }
        else if (Keyboard.current != null)
        {
            wantsDash = (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) && moveDir.sqrMagnitude > 0.001f;
        }

        bool canDash = status != null && status.currentStamina > minStaminaToDash;

        float speed = walkSpeed;
        if (wantsDash && canDash)
        {
            speed *= dashMultiplier;
            DrainStamina();
        }
        else
        {
            RecoverStamina();
        }

        Vector3 worldMove = transform.TransformDirection(moveDir) * speed * Time.deltaTime;
        controller.Move(worldMove);
    }

    private void DrainStamina()
    {
        if (status == null) return;
        status.currentStamina -= staminaDrainPerSecond * Time.deltaTime;
        if (status.currentStamina <= 0f)
        {
            status.currentStamina = 0f;
        }
    }

    private void RecoverStamina()
    {
        if (status == null) return;
        status.currentStamina += staminaRecoverPerSecond * Time.deltaTime;
        if (status.currentStamina > status.maxStamina)
        {
            status.currentStamina = status.maxStamina;
        }
    }
}