using UnityEngine;

// A hazard cube that slides back and forth. Touching it makes the player lose
// (that check lives in PlayerController.OnCollisionEnter).
[RequireComponent(typeof(Rigidbody))]
public class MovingObstacle : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(6f, 0f, 0f); // how far it travels from its start
    public float speed = 1f;                              // cycles per second-ish

    private Rigidbody rb;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // moved by script, not pushed by physics
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Mathf.PingPong goes 0 -> 1 -> 0 -> 1 ... over time.
        float t = Mathf.PingPong(Time.time * speed, 1f);
        rb.MovePosition(startPosition + moveOffset * t);
    }
}
