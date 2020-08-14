using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip_Offsite : MonoBehaviour
{
    // Question mark part of the question spot
    private GameObject questionMark;
    // Tip manager that we send text and description to for display
    private TipManager tipManager;
    // Objective manager that we turn off when a tip is activated
    private ObjectiveManager_Offsite objectiveManager;
    // Tip text and description
    public string tipText;
    public string tipDescription;
    // Whether the question mark is visible or not
    private bool visible;
    // Question mark's current alpha
    private float alpha;
    // Question mark's materials
    private Material[] materials;

    void Start()
    {
        // Get the question mark child object
        questionMark = transform.GetChild(0).gameObject;
        // Get managers
        tipManager = GameObject.Find("Tip UI").GetComponent<TipManager>();
        objectiveManager = GameObject.Find("Objective UI").GetComponent<ObjectiveManager_Offsite>();
        visible = true;
        alpha = 1;
        materials = new Material[questionMark.transform.childCount];
        for (int i = 0; i < questionMark.transform.childCount; i++)
        {
            materials[i] = questionMark.transform.GetChild(i).gameObject.GetComponent<Renderer>().material;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            objectiveManager.Deactivate();
            tipManager.SetTip(tipText, tipDescription);
            visible = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tipManager.HideTip();
            visible = true;
        }
    }

    // Updates question mark alpha to smoothly fade in and out depending on visibility setting
    void Update()
    {
        Vector3 pos = questionMark.transform.localPosition;
        if (visible)
        {
            alpha = Mathf.Min(1, alpha + 0.05f);
            pos.y = Mathf.Max(0.2f, pos.y - 0.5f * Time.deltaTime);
        }
        else
        {
            alpha = Mathf.Max(0, alpha - 0.05f);
            pos.y = Mathf.Min(1.2f, pos.y + 0.5f * Time.deltaTime);
        }
        foreach (Material m in materials)
        {
            m.SetColor("_Color", new Color(m.color.r, m.color.g, m.color.b, alpha));
            m.SetColor("_EmissionColor", new Color(alpha, alpha, alpha));
        }
        questionMark.transform.localPosition = pos;
    }

    void LateUpdate()
    {
        // Rotate the question mark around
        questionMark.transform.Rotate(new Vector3(0, 30 * Time.deltaTime, 0));
    }
}
