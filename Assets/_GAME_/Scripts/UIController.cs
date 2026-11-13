using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameplayUI;

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayNightText;
    [SerializeField] private TMP_Text scrapText;

    [SerializeField] private GameObject endDayButton;

    private bool isStartMenuActive = true;
    private bool isPauseMenuActive = false;
    private bool isGameOverMenuActive = false;

    private static UIController instance;

    public static UIController Instance
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
        startMenu.SetActive(isStartMenuActive);
        pauseMenu.SetActive(isPauseMenuActive);
        gameOverMenu.SetActive(isGameOverMenuActive);
        gameplayUI.SetActive(GameManager.Instance.HasStarted);
        endDayButton.SetActive(true);
    }

    private void Update()
    {
        timeText.text = $"{GameManager.Instance.GetTimeLeft()}s";
        scrapText.text = "Scrap: " + GameManager.Instance.scrap;

        if (GameManager.Instance.isNight)
        {
            dayNightText.text = $"Night {GameManager.Instance.nightNumber}";
        }
        else
        {
            dayNightText.text = $"Day {GameManager.Instance.nightNumber}";
        }
    }

    public void ToggleEndDayButton(bool isActive)
    {
        endDayButton.SetActive(isActive);
    }

    public void ToggleGameplayUI(bool isActive)
    {
        gameplayUI.SetActive(isActive);
    }


    public void ToggleStartMenu(bool isActive)
    {        
        startMenu.SetActive(isActive);
    }

    public void TogglePauseMenu(bool isActive)
    {        
        pauseMenu.SetActive(isActive);
    }

    public void ToggleGameOverMenu(bool isActive)
    {        
        gameOverMenu.SetActive(isActive);
    }

}
