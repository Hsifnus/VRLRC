using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager_Client : MonoBehaviour
{
    // Level name text UI element
    public Text levelHeader;
    // Level name text data
    public string levelHeaderText;
    // Level objective text UI element
    public Text objective;
    // Level objective text data
    public string objectiveText;
    // Timer progress bar
    public Image timerProgress;
    // Time remaining text UI element
    public Text timeRemaining;
    // The maximum time allotted for a level, in seconds
    public float timeLimit;
    // The current amount of time remaining, in seconds
    private float timer;
    // Controllers whose button presses bring up the objective menu
    public GameObject controller1;
    public GameObject controller2;
    private SteamVR_TrackedController _controller1;
    private SteamVR_TrackedController _controller2;
    // Component that groups all of the UI components together and allows for fading in and out
    private CanvasGroup canvasGroup;
    // Current alpha of the objective menu UI
    private float alpha;
    // Whether the UI should be active or not
    private bool active;

    // Initialize descriptions and timer
    void Start()
    {
        levelHeader.text = levelHeaderText;
        objective.text = objectiveText;
        timer = timeLimit;
        _controller1 = controller1.GetComponent<SteamVR_TrackedController>();
        _controller1.MenuButtonClicked += ToggleActive;
        _controller2 = controller2.GetComponent<SteamVR_TrackedController>();
        _controller2.MenuButtonClicked += ToggleActive;
        UpdateTimer();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        alpha = 0f;
        active = false;
        UpdateAlpha();
    }

    // Invert the active value
    private void ToggleActive(object sender, ClickedEventArgs e)
    {
        active = !active;
    }

    // Updates canvas alphas to smoothly fade in and out depending on activeness
    void UpdateAlpha()
    {
        if (active)
        {
            alpha = Mathf.Min(1, alpha + 0.05f);
        }
        else
        {
            alpha = Mathf.Max(0, alpha - 0.05f);
        }
        canvasGroup.alpha = alpha;
    }

    // Maps current timer to time string and timer progress
    void UpdateTimer()
    {
        if (timeLimit > 0)
        {
            timerProgress.fillAmount = timer / timeLimit;
            int ceilTimer = Mathf.CeilToInt(timer);
            int minutes = ceilTimer / 60;
            int seconds = ceilTimer % 60;
            timeRemaining.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else
        {
            timerProgress.fillAmount = 0;
            timeRemaining.text = "ERROR!";
        }
    }

    // Update timer data
    void Update()
    {
        timer -= Time.deltaTime;
        UpdateTimer();
        UpdateAlpha();
    }
}
