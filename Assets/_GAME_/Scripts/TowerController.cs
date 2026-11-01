
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public Transform turret;
    public Transform[] firePoints;
    public Projectile projectilePrefab;
    public LayerMask zombieLayers;
    [SerializeField] public GameObject towerArt;

    public float range = 5f;
    public float fireRate = 1f;
    public float turnSpeed = 180f;

    private float nextShot;

    private void Update()
    {
        if (!GameManager.Instance.isNight || turret == null || firePoints.Length == 0 || projectilePrefab == null || fireRate <= 0f || !towerArt.activeSelf) return;

        Zombie target = FindTarget();
        if (target == null) return;

        Vector2 direction = target.transform.position - turret.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var rand = Random.Range(-3.75f, 3.75f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90f + rand);

        turret.rotation = Quaternion.RotateTowards(turret.rotation, rotation, turnSpeed * Time.deltaTime);

        Projectile projectile1 = null, projectile2 = null;

        if (Quaternion.Angle(turret.rotation, rotation) > 5f || Time.time < nextShot) return;
        if(firePoints.Length >= 1)
        {
            projectile1 = Instantiate(projectilePrefab, firePoints[0].position, firePoints[0].rotation);
            projectile1.Fire(firePoints[0].up);
        }

        if(firePoints.Length >= 2)
        {
            projectile2 = Instantiate(projectilePrefab, firePoints[1].position, firePoints[1].rotation);
            projectile2.Fire(firePoints[1].up);
        }      

        

        nextShot = Time.time + 1f / fireRate;
    }

    private Zombie FindTarget()
    {
        Zombie closest = null;
        float distance = range * range;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, range, zombieLayers))
        {
            Zombie zombie = hit.GetComponentInParent<Zombie>();
            if (zombie == null) continue;

            float current = ((Vector2)(zombie.transform.position - transform.position)).sqrMagnitude;

            if (current < distance)
            {
                distance = current;
                closest = zombie;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
