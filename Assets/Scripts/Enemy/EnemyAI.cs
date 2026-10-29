using UnityEngine;

// Simple enemy AI with two states:
//  Patrol - walk between waypoints.
//  Chase  - run at the player when they come within detectionRange.
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase }

    public Transform player;
    public Transform[] waypoints;
    public float patrolSpeed = 3f;
    public float chaseSpeed = 4.5f;
    public float detectionRange = 8f;
    public float loseRange = 12f;   // bigger than detectionRange so it doesn't flicker between states

    public State CurrentState { get; private set; } = State.Patrol;

    private Rigidbody rb;
    private Renderer rend;
    private int waypointIndex;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying || player == null)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float distance = Vector3.Distance(Flat(transform.position), Flat(player.position));

        // Decide which state we are in.
        if (CurrentState == State.Patrol && distance < detectionRange) CurrentState = State.Chase;
        else if (CurrentState == State.Chase && distance > loseRange) CurrentState = State.Patrol;

        // Act on the state.
        Vector3 target;
        float speed;
        if (CurrentState == State.Chase)
        {
            target = player.position;
            speed = chaseSpeed;
        }
        else
        {
            if (waypoints == null || waypoints.Length == 0) return;
            target = waypoints[waypointIndex].position;
            speed = patrolSpeed;

            // Reached this waypoint? Go to the next one (loops back to the first).
            if (Vector3.Distance(Flat(transform.position), Flat(target)) < 0.5f)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        Vector3 direction = (Flat(target) - Flat(transform.position)).normalized;
        rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);

        // Visual feedback: brighter red while chasing.
        if (rend != null)
            rend.material.color = CurrentState == State.Chase ? new Color(1f, 0.1f, 0.1f) : new Color(0.6f, 0.1f, 0.1f);
    }

    // Ignore height so the enemy only moves on the ground plane.
    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
