using UnityEngine;

/// <summary>
/// Finish flag: touching it wins the level.
/// </summary>
public class Goal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        GameManager.Win();
    }
}
