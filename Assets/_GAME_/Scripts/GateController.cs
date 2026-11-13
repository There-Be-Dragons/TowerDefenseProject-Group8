using UnityEngine;
using UnityEngine.Events;

public class GateController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D bounds;
    [SerializeField] private float health = 500f;
    [SerializeField] private UnityEvent onDestroyed = new UnityEvent();

    public Bounds Area => bounds.bounds;
    public float Health => health;
    public bool IsDestroyed => health <= 0f;

    public void TakeDamage(float damage)
    {
        if (IsDestroyed || damage <= 0f) return;

        health = Mathf.Max(0f, health - damage);

        if (IsDestroyed)
            onDestroyed.Invoke();
    }
}

