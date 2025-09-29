using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementEx : MonoBehaviour {

    public float moveSpeed = 5.0f;
    private Rigidbody2D rb;
    private Vector2 input;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();
    }

    void FixedUpdate() {
        rb.linearVelocity = input * moveSpeed;
    }
}
