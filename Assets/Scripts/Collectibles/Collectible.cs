using UnityEngine;

// A spinning yellow cube. When the player walks into it, it adds score and disappears.
public class Collectible : MonoBehaviour
{
    public int points = 10;
    public float spinSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    // The collider is a trigger, so the player passes through it instead of bouncing off.
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;

        GameManager.Instance.AddScore(points);
        Destroy(gameObject);
    }
}
