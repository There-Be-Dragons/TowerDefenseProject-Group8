using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] public GameObject zombiePrefab;
    [SerializeField] public PathController path;
    [SerializeField] public ZombieController zombieController;

    [SerializeField] public float spawnInterval = 1f;
    [SerializeField] public int numberOfSpawners = 1;

    private bool isSpawning;

    private void Update()
    {
                
        if (!isSpawning && GameManager.Instance.isNight) SpawnWave();
    }

    private void Start()
    {
        GameManager.Instance.OnNightStart.AddListener(NewNight);
    }

    public void NewNight()
    {
        spawnInterval -= GameManager.Instance.nightNumber * 0.1f;
        if (GameManager.Instance.nightNumber % 10 == 0)
        {            
            spawnInterval *= 1.5f;
        }

        if(GameManager.Instance.nightNumber % 4 == 0)
        {
            numberOfSpawners += 1;            
        }

    }

    public void SpawnWave()
    {
        if (isSpawning || !GameManager.Instance.isNight) return;       

        isSpawning = true;
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for (int i = 0; i < numberOfSpawners; i++)
        {
            if (!GameManager.Instance.isNight) break;

            GameObject zombie = Instantiate(zombiePrefab, path.waypoints[0].position, Quaternion.identity);
            zombie.GetComponent<Zombie>().Initialize(path, zombieController);
        }
          

        yield return new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));

        isSpawning = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isSpawning = false;
    }
}