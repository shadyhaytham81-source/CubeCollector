using UnityEngine;

// Moves the player cube with WASD / arrow keys using physics,
// and ends the game if the player touches the enemy or a hazard.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;

    private Rigidbody rb;
    private Vector3 input;

    // Lets the demo/screenshot tool steer the player instead of the keyboard.
    [HideInInspector] public Vector3? overrideInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read keyboard input every frame (Update), apply it in FixedUpdate (physics).
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        input = new Vector3(x, 0f, z).normalized;

        if (overrideInput.HasValue) input = overrideInput.Value;
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Keep the current vertical speed, replace horizontal speed with our input.
        Vector3 velocity = input * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<EnemyAI>() != null)
            GameManager.Instance.Lose("The enemy caught you!");
        else if (collision.gameObject.GetComponent<MovingObstacle>() != null)
            GameManager.Instance.Lose("You hit a moving obstacle!");
    }
}
