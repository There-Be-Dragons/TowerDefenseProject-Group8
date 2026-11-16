using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Zombie : MonoBehaviour
{
    [SerializeField] public PathController path;
    [SerializeField] public float speed = 2f;
    [SerializeField] public bool canMove = true;
    [SerializeField] private float health = 50f;
    [SerializeField] private float radius = 0.35f;

    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;
    [SerializeField, Min(0f)] private float attackReach = 0.25f;

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
    private float nextAttack;
    private Color randomColor;
    private Color currentColor;
    private bool nightAppearance = true;
    private bool fading;
    private float scrapReward;
    private float scrapMultiplier = 1f;
    private bool moving;
    private float distanceTraveled;
    private Vector2 lastPosition;
    private Zombie blockedBy;

    public float Radius => radius;
    public Vector2 NextPosition => nextPosition;
    public bool IsAlive => initialized && health > 0f;
    internal Zombie BlockedBy => blockedBy;
    internal int MovementOrder { get; set; }

    private void UpdateDistanceTraveled()
    {
        if (!initialized) return;

        distanceTraveled += Vector2.Distance(lastPosition, rb.position);
        lastPosition = rb.position;
    }

    internal bool HasTravelPriorityOver(Zombie zombie)
    {
        UpdateDistanceTraveled();
        zombie.UpdateDistanceTraveled();

        if (distanceTraveled != zombie.distanceTraveled) return distanceTraveled > zombie.distanceTraveled;

        return MovementOrder < zombie.MovementOrder;
    }

    public Vector2 GetPredictedPosition(float seconds)
    {
        Vector2 position = rb.position;
        if (!initialized || !moving || !canMove || health <= 0f || speed <= 0f) return position;

        float distance = speed * Mathf.Max(0f, seconds);

        for (int i = waypointIndex; i < path.waypoints.Length; i++)
        {
            Vector2 waypoint = path.GetOffsetPoint(i, offset);
            float segmentLength = Vector2.Distance(position, waypoint);

            if (distance <= segmentLength) return Vector2.MoveTowards(position, waypoint, distance);

            position = waypoint;
            distance -= segmentLength;
        }

        return position;
    }

    public void ApplySpawnScaling(float healthMultiplier, float speedMultiplier, float sizeMultiplier = 1f, float damageMultiplier = 1f,
        float rewardMultiplier = 1f)
    {
        if (initialized) return;

        health *= Mathf.Max(0.01f, healthMultiplier);
        speed *= Mathf.Max(0.01f, speedMultiplier);
        attackDamage *= Mathf.Max(0f, damageMultiplier);
        scrapMultiplier = Mathf.Max(0f, rewardMultiplier);
        zombieSpriteRenderer.transform.localScale *= Mathf.Max(0.01f, sizeMultiplier);
    }

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

    public void UpdateAppearance(bool isNight, float blend)
    {
        canMove = isNight;

        if (nightAppearance != isNight)
        {
            nightAppearance = isNight;
            fading = true;
        }

        if (!fading) return;

        Color target = isNight ? randomColor : Color.black * 0.7f;
        currentColor = Color.Lerp(currentColor, target, blend);

        if (((Vector4)(currentColor - target)).sqrMagnitude < 0.0001f)
        {
            currentColor = target;
            fading = false;
        }

        zombieSpriteRenderer.color = currentColor;
    }

    public void Initialize(PathController newPath, ZombieController newController)
    {
        if (initialized) return;

        if (!newController.TryGetSpawnPoint(this, newPath, out Vector3 position, out float laneOffset))
        {
            Destroy(gameObject);
            return;
        }

        Initialize(newPath, newController, position, laneOffset);
    }

    public void Initialize(PathController newPath, ZombieController newController, Vector3 position, float laneOffset)
    {
        if (initialized) return;

        path = newPath;
        controller = newController;
        offset = laneOffset;
        scrapReward = health * newController.scrapPerHealth * scrapMultiplier;
        waypointIndex = 1;
        transform.position = position;
        rb.position = position;
        nextPosition = position;
        lastPosition = position;
        distanceTraveled = 0f;
        blockedBy = null;

        randomColor = RandomColor(bottomLeftColor, bottomRightColor, topLeftColor, topRightColor);
        currentColor = randomColor;
        zombieSpriteRenderer.color = randomColor;

        initialized = true;
        enabled = true;
        controller.Register(this);
    }

    public bool CanAttackGate(Bounds area)
    {
        if (!initialized || !canMove || health <= 0f || waypointIndex < path.waypoints.Length - 1) return false;

        Vector2 position = rb.position;
        Vector2 closest = new Vector2(Mathf.Clamp(position.x, area.min.x, area.max.x), Mathf.Clamp(position.y, area.min.y, area.max.y));
        float reach = radius + attackReach;

        return (closest - position).sqrMagnitude <= reach * reach;
    }

    public void AttackGate(GateController gate)
    {
        StopMoving();

        if (!canMove || health <= 0f || gate.IsDestroyed || Time.time < nextAttack) return;

        nextAttack = Time.time + attackInterval;
        gate.TakeDamage(attackDamage);
    }

    public void StopMoving()
    {
        UpdateDistanceTraveled();
        blockedBy = null;
        moving = false;
        rb.linearVelocity = Vector2.zero;

        if (initialized)
            controller.UpdatePosition(this, rb.position);

        nextPosition = rb.position;
    }

    public void Move()
    {
        UpdateDistanceTraveled();

        if (!initialized || !canMove || health <= 0f || waypointIndex >= path.waypoints.Length)
        {
            StopMoving();
            return;
        }

        Vector2 target = path.GetOffsetPoint(waypointIndex, offset);

        if ((target - rb.position).sqrMagnitude < 0.0025f)
        {
            waypointIndex++;
            StopMoving();
            return;
        }

        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);

        if (controller.TryGetBlocker(this, next, out Zombie blocker, resolveDeadlocks: true))
        {
            StopMoving();
            blockedBy = blocker;
            return;
        }

        blockedBy = null;
        controller.UpdatePosition(this, next);
        moving = true;
        nextPosition = next;
        rb.MovePosition(next);
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0f || damage <= 0f) return;

        health -= damage;

        if (health <= 0f)
        {
            StopMoving();
            GameManager.Instance.AddScrap(scrapReward);

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
