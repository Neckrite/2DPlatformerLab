using UnityEngine;

/// <summary>
/// Smooth camera follow.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.2f, -10f);
    public float smooth = 6f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 want = target.position + offset;
        float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, want, k);
    }
}
