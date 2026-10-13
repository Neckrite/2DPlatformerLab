using UnityEngine;

/// <summary>
/// Flips the player's eye marker so the character looks where it runs.
/// </summary>
public class PlayerFaceDirection : MonoBehaviour
{
    Transform eye;

    void Awake()
    {
        eye = transform.Find("Eye");
    }

    void Update()
    {
        if (eye == null) return;
        float vx = GetComponent<Rigidbody2D>().velocity.x;
        if (Mathf.Abs(vx) < 0.1f) return;
        var s = eye.transform.localScale;
        s.x = Mathf.Abs(s.x) * (vx > 0f ? 1f : -1f);
        eye.transform.localScale = s;
    }
}
