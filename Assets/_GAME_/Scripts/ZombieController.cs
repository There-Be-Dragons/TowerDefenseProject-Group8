using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField] public int scrapPerZombie = 2;
    private readonly List<Zombie> zombies = new List<Zombie>();

    private void FixedUpdate()
    {
        foreach (Zombie zombie in zombies)
        {
            if (zombie.isActiveAndEnabled)
                zombie.Move();
        }
    }

    public void Register(Zombie zombie)
    {
        if (!zombies.Contains(zombie))
            zombies.Add(zombie);
    }

    public void Unregister(Zombie zombie)
    {
        zombies.Remove(zombie);
    }

    public bool TryGetSpawnPoint(Zombie zombie, PathController path, out Vector3 position, out float offset)
    {
        float maxOffset = Mathf.Max(0f, path.offset / 2f - zombie.Radius);

        for (int i = 0; i < 20; i++)
        {
            offset = Random.Range(-maxOffset, maxOffset);
            position = path.GetOffsetPoint(0, offset);

            if (!IsBlocked(zombie, position)) return true;
        }

        position = default;
        offset = 0f;
        return false;
    }

    public bool IsBlocked(Zombie movingZombie, Vector2 position)
    {
        foreach (Zombie zombie in zombies)
        {
            if (zombie == movingZombie || !zombie.gameObject.activeInHierarchy) continue;

            float distance = movingZombie.Radius + zombie.Radius;

            if ((zombie.NextPosition - position).sqrMagnitude < distance * distance)
                return true;
        }

        return false;
    }
}