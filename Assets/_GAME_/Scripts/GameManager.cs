using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float dayLength = 60f;
    [SerializeField] private float nightLength = 90f;
    public int scrap;
    [SerializeField] private int startingScrap = 50;
    [SerializeField] private GameObject endDayButton;

    public bool isNight;
    public int nightNumber;
    public float timeLeft;

    private static GameManager instance;

    public UnityEvent OnNightStart;

    public static GameManager Instance
    {
        get
        {            
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartDay();
        scrap = startingScrap;
        endDayButton.SetActive(true);
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            if (isNight) StartDay();
            else StartNight();
        }
    }

    public void EndDay()
    {
        if (!isNight && timeLeft > 2f)
        {
            timeLeft = 2f;
            endDayButton.SetActive(false);
        }
    }

    public void AddScrap(int amount)
    {
        scrap += amount;
        scrap = Mathf.Max(scrap, 0);
    }

    public void SpendScrap(int amount)
    {
        scrap -= amount;
        scrap = Mathf.Max(scrap, 0);
    }

    public void StartDay()
    {
        isNight = false;
        timeLeft = dayLength;
        endDayButton.SetActive(true);
    }

    public void StartNight()
    {
        isNight = true;
        nightNumber++;
        timeLeft = nightLength;
        OnNightStart?.Invoke();
    }

    private void OnGUI()
    {
        string phase = isNight ? "Night" : "Day";
        GUI.Label(new Rect(10, 10, 300, 25), $"{phase} {nightNumber} - {Mathf.CeilToInt(timeLeft)}");
        GUI.Label(new Rect(10, 36, 300, 25), $"Scrap: {scrap}");
    }
}
