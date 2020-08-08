using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : MonoBehaviour
{
    // Tip name text UI element
    public Text tipHeader;
    // Tip description text UI element
    public Text description;
    // Component that groups all of the UI components together and allows for fading in and out
    private CanvasGroup canvasGroup;
    // Current alpha of the objective menu UI
    private float alpha;
    // Whether the UI should be active or not
    private bool active;

    // Initialize descriptions and timer
    void Start()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        alpha = 0f;
        active = false;
        UpdateAlpha();
    }

    // Sets the title text and description of the tip and activates its UI.
    public void SetTip(string title, string desc)
    {
        tipHeader.text = title;
        description.text = desc;
        active = true;
    }

    // Hides the tip UI if it is currently active.
    public void HideTip()
    {
        active = false;
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

    // Update timer data
    void Update()
    {
        UpdateAlpha();
    }
}
