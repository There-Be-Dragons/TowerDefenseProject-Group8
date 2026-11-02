using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Zombie : MonoBehaviour
{
    [SerializeField] public PathController path;
    [SerializeField] public float speed = 2f;
    [SerializeField] public bool canMove = true;
    [SerializeField] private float health = 50f;
    [SerializeField] private float radius = 0.35f;

    [SerializeField] private GameObject zombieDeathEffectPrefab;
    [SerializeField] private SpriteRenderer[] zombieSpriteRenderers;
    [SerializeField] private Color bottomLeftColor = Color.green;
    [SerializeField] private Color bottomRightColor = Color.green;
    [SerializeField] private Color topLeftColor = Color.green;
    [SerializeField] private Color topRightColor = Color.green;
    private SpriteRenderer zombieSpriteRenderer;

    private Rigidbody2D rb;
    private ZombieController controller;
    private int waypointIndex;
    private float offset;
    private bool initialized;
    private Vector2 nextPosition;

    public float Radius => radius;
    public Vector2 NextPosition => nextPosition;

    private Color randomColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;
        var r = Random.Range(0, zombieSpriteRenderers.Length);
        zombieSpriteRenderer = zombieSpriteRenderers[r];
        zombieSpriteRenderers[r].gameObject.SetActive(true);
    }

    private void Update()
    {
        canMove = GameManager.Instance.isNight;
        if (!GameManager.Instance.isNight)
            zombieSpriteRenderer.color = Color.Lerp(zombieSpriteRenderer.color, Color.black * 0.7f, 2 * Time.deltaTime);
        else
            zombieSpriteRenderer.color = Color.Lerp(zombieSpriteRenderer.color, randomColor, 2 * Time.deltaTime);


    }

    public void Initialize(PathController newPath, ZombieController newController)
    {
        if (initialized) return;

        path = newPath;
        controller = newController;

        if (!controller.TryGetSpawnPoint(this, path, out Vector3 position, out offset))
        {
            Destroy(gameObject);
            return;
        }

        waypointIndex = 1;
        transform.position = position;
        rb.position = position;
        nextPosition = position;

        randomColor = RandomColor(bottomLeftColor, bottomRightColor, topLeftColor, topRightColor);
        zombieSpriteRenderer.color = randomColor;

        initialized = true;
        enabled = true;
        controller.Register(this);
    }

    public void Move()
    {
        if (!initialized || !canMove || waypointIndex >= path.waypoints.Length)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = path.GetOffsetPoint(waypointIndex, offset);

        if (Vector2.Distance(rb.position, target) < 0.05f)
        {
            waypointIndex++;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);

        if (controller.IsBlocked(this, next))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        nextPosition = next;
        rb.MovePosition(next);
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f || damage <= 0f) return;

        health -= damage;

        if (health <= 0f)
        {
            GameManager.Instance.AddScrap(controller.scrapPerZombie);
            
            var deathEffect = Instantiate(zombieDeathEffectPrefab, transform.position, Quaternion.identity);

            Destroy(deathEffect, 1f);
            Destroy(gameObject);
        }
    }

    public static Color RandomColor(Color bottomLeft, Color bottomRight, Color topLeft, Color topRight)
    {
        float x = Random.value;
        float y = Random.value;

        Color bottom = Color.Lerp(bottomLeft, bottomRight, x);
        Color top = Color.Lerp(topLeft, topRight, x);

        return Color.Lerp(bottom, top, y);
    }

    private void OnDestroy()
    {
        if (initialized && controller != null)
            controller.Unregister(this);
    }
}