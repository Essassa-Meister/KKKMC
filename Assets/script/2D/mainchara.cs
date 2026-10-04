using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class mainchara : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // キャラクターの移動速度
                                                   // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Rigidbody2D rb;
    private Vector2 move;
    void Start()
    {
       rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        Debug.Log("入力された値: " + move); // この1行を追加
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * moveSpeed);
    }
}
