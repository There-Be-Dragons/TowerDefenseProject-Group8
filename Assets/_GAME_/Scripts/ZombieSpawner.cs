using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] public GameObject zombiePrefab;
    [SerializeField] public PathController path;
    [SerializeField] public ZombieController zombieController;

    [Header("Difficulty")]
    [SerializeField, Min(1)] private int firstNightThreat = 18;
    [SerializeField, Min(0)] private int threatPerNight = 4;
    [SerializeField, Min(1)] private int maxNightThreat = 120;
    [SerializeField, Min(1)] private int healthScalingStartNight = 5;
    [SerializeField, Min(0f)] private float healthGrowthPerNight = 0.04f;

    [Header("Pacing")]
    [SerializeField, Min(1)] private int groupsPerNight = 3;
    [SerializeField, Min(0f)] private float groupBreak = 1.5f;
    [SerializeField, Min(1)] private int maxAliveZombies = 120;

    [Header("Runners")]
    [SerializeField, Min(1)] private int runnerStartNight = 3;
    [SerializeField, Range(0f, 0.4f)] private float runnerThreatShare = 0.2f;
    [SerializeField, Min(0.01f)] private float runnerHealthMultiplier = 0.65f;
    [SerializeField, Min(0.01f)] private float runnerSpeedMultiplier = 1.5f;
    [SerializeField, Min(0.01f)] private float runnerSizeMultiplier = 0.85f;
    [SerializeField, Min(1f)] private float runnerDamageMultiplier = 1.5f;

    [Header("Bruisers")]
    [SerializeField, Min(1)] private int bruiserStartNight = 5;
    [SerializeField, Range(0f, 0.4f)] private float bruiserThreatShare = 0.2f;
    [SerializeField, Min(0.01f)] private float bruiserHealthMultiplier = 6f;
    [SerializeField, Min(0.01f)] private float bruiserSpeedMultiplier = 0.5f;
    [SerializeField, Min(0.01f)] private float bruiserSizeMultiplier = 1.15f;
    [SerializeField, Min(1f)] private float bruiserDamageMultiplier = 3f;

    private const int bruiserThreatCost = 3;
    private const float minimumSpawnInterval = 0.1f;
    private const float blockedSpawnRetry = 0.2f;

    private GameManager gameManager;
    private Zombie zombieTemplate;
    private int plannedNight = -1;
    private int normalRemaining;
    private int runnerRemaining;
    private int bruiserRemaining;
    private int spawnedThisNight;
    private int groupSize;
    private float spawnInterval;
    private float plannedGroupBreak;
    private float nextSpawnTime;

    public int NightThreatTarget { get; private set; }
    public int TargetZombiesPerNight { get; private set; }
    public int ZombiesSpawnedThisNight => spawnedThisNight;
    public int ZombiesRemainingInCycle => normalRemaining + runnerRemaining + bruiserRemaining;
    public float NightHealthMultiplier { get; private set; } = 1f;
    public float NightRewardMultiplier { get; private set; } = 1f;

    private void Start()
    {
        zombieTemplate = zombiePrefab.GetComponent<Zombie>();
        gameManager = GameManager.Instance;
        gameManager.OnNightStart.AddListener(NewNight);

        if (gameManager.isNight) NewNight();
    }

    private void Update()
    {
        if (!gameManager.isNight || Time.timeScale <= 0f || gameManager.timeLeft <= 0f) return;

        if (plannedNight != gameManager.nightNumber) NewNight();
        if (Time.time >= nextSpawnTime) SpawnWave();
    }

    public void NewNight()
    {
        if (!gameManager.isNight || plannedNight == gameManager.nightNumber) return;

        plannedNight = gameManager.nightNumber;
        int nightIndex = Mathf.Max(0, plannedNight - 1);

        NightThreatTarget = Mathf.FloorToInt(Mathf.Clamp(firstNightThreat + nightIndex * (float)threatPerNight,
            1f, Mathf.Max(1, maxNightThreat)));
        NightHealthMultiplier = 1f + Mathf.Max(0, plannedNight - healthScalingStartNight) * healthGrowthPerNight;
        NightRewardMultiplier = Mathf.Sqrt(Mathf.Min(1f, Mathf.Max(1, firstNightThreat) / (float)NightThreatTarget));

        ResetSpawnComposition();
        TargetZombiesPerNight = ZombiesRemainingInCycle;
        spawnedThisNight = 0;
        groupSize = Mathf.CeilToInt(TargetZombiesPerNight / (float)Mathf.Max(1, groupsPerNight));

        float groupDuration = gameManager.timeLeft * groupSize / TargetZombiesPerNight;
        plannedGroupBreak = groupSize > 1 ? Mathf.Min(groupBreak, groupDuration * 0.5f) : groupDuration;
        spawnInterval = Mathf.Max(minimumSpawnInterval,
            (groupDuration - plannedGroupBreak) / Mathf.Max(1, groupSize - 1));
        nextSpawnTime = Time.time;
    }

    private void ResetSpawnComposition()
    {
        bruiserRemaining = plannedNight >= bruiserStartNight
            ? Mathf.FloorToInt(NightThreatTarget * bruiserThreatShare / bruiserThreatCost) : 0;
        runnerRemaining = plannedNight >= runnerStartNight
            ? Mathf.FloorToInt(NightThreatTarget * runnerThreatShare) : 0;
        normalRemaining = NightThreatTarget - runnerRemaining - bruiserRemaining * bruiserThreatCost;
    }

    public void SpawnWave()
    {
        if (!gameManager.isNight || Time.timeScale <= 0f || gameManager.timeLeft <= 0f) return;
        if (plannedNight != gameManager.nightNumber) NewNight();
        if (Time.time < nextSpawnTime) return;

        if (zombieController.ActiveZombieCount >= maxAliveZombies ||
            !zombieController.TryGetSpawnPoint(zombieTemplate, path, out Vector3 position, out float offset))
        {
            nextSpawnTime = Time.time + blockedSpawnRetry;
            return;
        }

        if (ZombiesRemainingInCycle <= 0) ResetSpawnComposition();

        Zombie zombie = Instantiate(zombiePrefab, position, Quaternion.identity).GetComponent<Zombie>();

        if (bruiserRemaining > 0 && (spawnedThisNight % 6 == 5 || normalRemaining + runnerRemaining == 0))
        {
            zombie.ApplySpawnScaling(NightHealthMultiplier * bruiserHealthMultiplier, bruiserSpeedMultiplier,
                bruiserSizeMultiplier, bruiserDamageMultiplier, NightRewardMultiplier);
            bruiserRemaining--;
        }
        else if (runnerRemaining > 0 && (spawnedThisNight % 3 == 2 || normalRemaining == 0))
        {
            zombie.ApplySpawnScaling(NightHealthMultiplier * runnerHealthMultiplier, runnerSpeedMultiplier,
                runnerSizeMultiplier, runnerDamageMultiplier, NightRewardMultiplier);
            runnerRemaining--;
        }
        else
        {
            zombie.ApplySpawnScaling(NightHealthMultiplier, 1f, rewardMultiplier: NightRewardMultiplier);
            normalRemaining--;
        }

        zombie.Initialize(path, zombieController, position, offset);
        spawnedThisNight++;
        float delay = spawnedThisNight % groupSize == 0 ? plannedGroupBreak : spawnInterval;
        nextSpawnTime = Time.time + delay;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnNightStart.RemoveListener(NewNight);
    }
}
