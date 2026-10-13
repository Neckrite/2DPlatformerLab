using UnityEngine;

/// <summary>
/// Collectible coin: bobs in place, disappears when the player touches it.
/// </summary>
public class Coin : MonoBehaviour
{
    Vector3 basePos;
    float phase;

    void Awake()
    {
        basePos = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float t = Time.time;
        transform.position = basePos + new Vector3(0f, Mathf.Sin(t * 2.5f + phase) * 0.08f, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        GameManager.AddCoinToCounter();
        Destroy(gameObject);
    }
}
