using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField, Min(0f)] public float scrapPerHealth = 0.0045f;
    [SerializeField] private GateController gate;

    private const float cellSize = 1f;
    private readonly List<Zombie> zombies = new List<Zombie>();
    private readonly Dictionary<Vector2Int, List<Zombie>> cells = new Dictionary<Vector2Int, List<Zombie>>();
    private readonly Stack<List<Zombie>> spareCells = new Stack<List<Zombie>>();
    private float largestRadius;

    public int ActiveZombieCount => zombies.Count;

    private void Update()
    {
        bool isNight = GameManager.Instance.isNight;
        float blend = 2f * Time.deltaTime;

        foreach (Zombie zombie in zombies)
        {
            if (zombie.isActiveAndEnabled)
                zombie.UpdateAppearance(isNight, blend);
        }
    }

    private void FixedUpdate()
    {
        Bounds area = gate.Area;
        bool isNight = GameManager.Instance.isNight;

        largestRadius = 0f;
        foreach (Zombie zombie in zombies)
            largestRadius = Mathf.Max(largestRadius, zombie.Radius);

        foreach (Zombie zombie in zombies)
        {
            if (!zombie.isActiveAndEnabled) continue;

            if (!isNight)
            {
                zombie.StopMoving();
                continue;
            }

            if (zombie.CanAttackGate(area))
                zombie.AttackGate(gate);
            else
                zombie.Move();
        }
    }

    public void Register(Zombie zombie)
    {
        if (zombies.Contains(zombie)) return;

        zombies.Add(zombie);
        largestRadius = Mathf.Max(largestRadius, zombie.Radius);
        AddToCell(zombie, GetCell(zombie.NextPosition));
    }

    public void Unregister(Zombie zombie)
    {
        if (!zombies.Remove(zombie)) return;
        RemoveFromCell(zombie, GetCell(zombie.NextPosition));
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
        float searchRadius = movingZombie.Radius + largestRadius;
        Vector2Int min = GetCell(position - Vector2.one * searchRadius);
        Vector2Int max = GetCell(position + Vector2.one * searchRadius);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                if (!cells.TryGetValue(new Vector2Int(x, y), out List<Zombie> nearby)) continue;

                foreach (Zombie zombie in nearby)
                {
                    if (zombie == movingZombie || !zombie.gameObject.activeInHierarchy) continue;

                    float distance = movingZombie.Radius + zombie.Radius;

                    if ((zombie.NextPosition - position).sqrMagnitude < distance * distance)
                        return true;
                }
            }
        }

        return false;
    }

    public void UpdatePosition(Zombie zombie, Vector2 position)
    {
        Vector2Int previous = GetCell(zombie.NextPosition);
        Vector2Int next = GetCell(position);

        if (previous == next) return;

        RemoveFromCell(zombie, previous);
        AddToCell(zombie, next);
    }

    private Vector2Int GetCell(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.y / cellSize));
    }

    private void AddToCell(Zombie zombie, Vector2Int cell)
    {
        if (!cells.TryGetValue(cell, out List<Zombie> nearby))
        {
            nearby = spareCells.Count > 0 ? spareCells.Pop() : new List<Zombie>();
            cells.Add(cell, nearby);
        }

        nearby.Add(zombie);
    }

    private void RemoveFromCell(Zombie zombie, Vector2Int cell)
    {
        List<Zombie> nearby = cells[cell];
        nearby.Remove(zombie);

        if (nearby.Count == 0)
        {
            cells.Remove(cell);
            spareCells.Push(nearby);
        }
    }
}
