using UnityEngine;

// Keeps the camera at a fixed angle above and behind the player, smoothly following.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 18f, -13f);
    public float smoothSpeed = 5f;

    void LateUpdate() // after all movement this frame
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.unscaledDeltaTime);
        transform.LookAt(target);
    }
}
