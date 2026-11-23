using UnityEngine;
using UnityEngine.Events;

public class GateController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D bounds;
    [SerializeField] private float health = 500f;
    [SerializeField] private UnityEvent onDestroyed = new UnityEvent();
    [SerializeField] private RectTransform healthBackground;
    [SerializeField] private RectTransform healthForeground;

    private float maxHealth;
    public Bounds Area => bounds.bounds;
    public float Health => health;
    public bool IsDestroyed => health <= 0f;

    private void Awake()
    {
        maxHealth = health;
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed || damage <= 0f) return;

        health = Mathf.Max(0f, health - damage);

        if (IsDestroyed)
            onDestroyed.Invoke();
    }

    private void LateUpdate()
    {
        if (healthBackground == null || healthForeground == null || maxHealth <= 0f)
            return;

        float width = healthBackground.rect.width * (health / maxHealth);
        healthForeground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}

