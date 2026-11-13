using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float dayLength = 60f;
    [SerializeField] private float nightLength = 90f;
    public int scrap;
    [SerializeField] private int startingScrap = 50;
    

    public bool isNight;
    public int nightNumber;
    public float timeLeft;

    private static GameManager instance;

    public UnityEvent OnNightStart;


    private bool hasStarted = false;
    private bool isGameOver;
    private float scrapProgress;

    public bool HasStarted => hasStarted;
    public bool CanBuild => hasStarted && !isGameOver && !isNight && Time.timeScale > 0f;

    public static GameManager Instance
    {
        get
        {            
            return instance;
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        isGameOver = true;
        Pause();
        Debug.Log("Game Over");
        UIController.Instance.TogglePauseMenu(false);
        UIController.Instance.ToggleGameOverMenu(true);
    }

    public void StartGame()
    {
        if (hasStarted) return;

        UIController.Instance.ToggleStartMenu(false);
        UIController.Instance.ToggleGameplayUI(true);
        StartDay();
        hasStarted = true;
        Resume();
        
    }

    public void PauseGame()
    {
        if (!hasStarted || isGameOver) return;

        Pause();
        UIController.Instance.TogglePauseMenu(true);
    }

    public void ResumeGame()
    {
        if (!hasStarted || isGameOver) return;

        UIController.Instance.TogglePauseMenu(false);
        Resume();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    public int GetTimeLeft()
    {
        return Mathf.CeilToInt(timeLeft);
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {        
        scrap = startingScrap;
        Pause();
        UIController.Instance.ToggleStartMenu(true);    

    }

    private void Update()
    {
        if (!hasStarted || isGameOver) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Time.timeScale > 0f) PauseGame();
            else ResumeGame();
        }

        if (Time.timeScale <= 0f) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            if (isNight) StartDay();
            else StartNight();
        }
    }

    

    public void EndDay()
    {
        if (CanBuild && timeLeft > 2f)
        {
            timeLeft = 2f;
            UIController.Instance.ToggleEndDayButton(false);
        }
    }

    public void AddScrap(int amount)
    {
        scrap += amount;
        scrap = Mathf.Max(scrap, 0);
    }

    public void AddScrap(float amount)
    {
        if (amount <= 0f) return;

        scrapProgress += amount;
        int wholeScrap = Mathf.FloorToInt(scrapProgress + 0.0001f);
        scrapProgress = Mathf.Max(0f, scrapProgress - wholeScrap);
        AddScrap(wholeScrap);
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
        UIController.Instance.ToggleEndDayButton(true);
    }

    public void StartNight()
    {
        isNight = true;
        nightNumber++;
        timeLeft = nightLength;
        OnNightStart?.Invoke();
    }

}
