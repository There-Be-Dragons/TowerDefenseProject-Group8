using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public GameObject towerArt;
    public Transform turret;
    public Transform[] firePoints;
    public ParticleSystem[] muzzleFlashes;
    public Projectile projectilePrefab;
    public LayerMask zombieLayers;

    public float range = 5f;
    public float fireRate = 1f;
    public float turnSpeed = 180f;
    [Min(0f)] public float spreadAngle = 3.75f;

    private float nextShot;
    private Zombie target;
    private readonly List<Collider2D> hits = new List<Collider2D>();

    private void Update()
    {
        if (!GameManager.Instance.isNight || Time.timeScale <= 0f || !towerArt.activeInHierarchy || fireRate <= 0f ||
            projectilePrefab.speed <= 0f || firePoints.Length == 0) return;

        if (target == null || !target.IsAlive || !target.isActiveAndEnabled ||
            ((Vector2)target.transform.position - (Vector2)transform.position).sqrMagnitude > range * range)
            target = FindTarget();
        if (target == null) return;

        Vector2 origin = Vector2.zero;
        foreach (Transform firePoint in firePoints) origin += (Vector2)firePoint.position;
        origin /= firePoints.Length;

        if (!TryGetAimPoint(target, origin, out Vector2 aimPoint)) return;

        Vector2 direction = aimPoint - (Vector2)turret.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + Random.Range(-spreadAngle, spreadAngle));

        turret.rotation = Quaternion.RotateTowards(turret.rotation, rotation, turnSpeed * Time.deltaTime);

        if (Quaternion.Angle(turret.rotation, rotation) > 5f || Time.time < nextShot) return;

        float recoilAngle = Mathf.DeltaAngle(angle, turret.eulerAngles.z);
        bool fired = false;
        for (int i = 0; i < firePoints.Length; i++)
        {
            Transform firePoint = firePoints[i];
            if (!TryGetAimPoint(target, firePoint.position, out aimPoint)) continue;

            direction = aimPoint - (Vector2)firePoint.position;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rotation = Quaternion.Euler(0f, 0f, angle + recoilAngle);

            Projectile projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
            projectile.Fire(rotation * Vector3.up);

            if (muzzleFlashes != null && i < muzzleFlashes.Length && muzzleFlashes[i] != null)
            {
                muzzleFlashes[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                muzzleFlashes[i].Play();
            }

            fired = true;
        }

        if (fired) nextShot = Time.time + 1f / fireRate;
    }

    private bool TryGetAimPoint(Zombie zombie, Vector2 origin, out Vector2 aimPoint)
    {
        float earliest = 0f;
        float latest = projectilePrefab.lifetime;
        float projectileSpeed = projectilePrefab.speed;
        aimPoint = zombie.GetPredictedPosition(0f);

        if (projectileSpeed <= 0f || latest <= 0f) return false;

        Vector2 latestPosition = zombie.GetPredictedPosition(latest);
        if ((latestPosition - origin).sqrMagnitude > projectileSpeed * projectileSpeed * latest * latest) return false;

        for (int i = 0; i < 16; i++)
        {
            float time = (earliest + latest) * 0.5f;
            Vector2 position = zombie.GetPredictedPosition(time);
            float travelDistance = projectileSpeed * time;

            if ((position - origin).sqrMagnitude > travelDistance * travelDistance) earliest = time;
            else latest = time;
        }

        aimPoint = zombie.GetPredictedPosition(latest);
        return (aimPoint - origin).sqrMagnitude > 0.0001f;
    }

    private Zombie FindTarget()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(zombieLayers);
        filter.useTriggers = Physics2D.queriesHitTriggers;

        Physics2D.OverlapCircle(transform.position, range, filter, hits);

        Zombie closest = null;
        float distance = range * range;
        Vector2 position = transform.position;

        foreach (Collider2D hit in hits)
        {
            Zombie zombie = hit.GetComponentInParent<Zombie>();
            if (zombie == null || !zombie.IsAlive || !zombie.isActiveAndEnabled) continue;

            float current = ((Vector2)zombie.transform.position - position).sqrMagnitude;

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
