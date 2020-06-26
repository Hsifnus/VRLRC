using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class ObjectiveManager_Offsite : MonoBehaviour
{
    public Text levelHeader;
    public string levelHeaderText;
    public Text objective;
    public string objectiveText;
    public Image timerProgress;
    public Text timeRemaining;
    public float timeLimit;
    private float timer;
    public GameObject controller;
    private SDK_InputSimulator _controller;
    private CanvasGroup canvasGroup;
    private float alpha;
    private bool active;

    // Initialize descriptions and timer
    void Start()
    {
        levelHeader.text = levelHeaderText;
        objective.text = objectiveText;
        timer = timeLimit;
        UpdateTimer();
        _controller = controller.GetComponent<SDK_InputSimulator>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        alpha = 0f;
        active = false;
        UpdateAlpha();
    }

    void ToggleActive()
    {
        active = !active;
    }

    // Updates canvas alphas to smoothly fade in and out depending on activeness
    void UpdateAlpha()
    {
        if (active)
        {
            alpha = Mathf.Min(1, alpha + 0.05f);
        } else
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
        } else
        {
            timerProgress.fillAmount = 0;
            timeRemaining.text = "ERROR!";
        }
    }

    // Update timer data
    void Update()
    {
        if (Input.GetKeyDown(_controller.buttonTwoAlias))
        {
            ToggleActive();
        }
        timer -= Time.deltaTime;
        UpdateTimer();
        UpdateAlpha();
    }
}
