using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip : MonoBehaviour
{
    // Question mark part of the question spot
    private GameObject questionMark;
    // Tip manager that we send text and description to for display
    private TipManager tipManager;
    // Objective manager that we turn off when a tip is activated
    private ObjectiveManager objectiveManager;
    // Tip text and description
    public string tipText;
    public string tipDescription;

    void Start()
    {
        // Get the question mark child object
        questionMark = transform.GetChild(0).gameObject;
        // Get managers
        tipManager = GameObject.Find("Tip UI").GetComponent<TipManager>();
        objectiveManager = GameObject.Find("Objective UI").GetComponent<ObjectiveManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            objectiveManager.Deactivate();
            questionMark.SetActive(false);
            tipManager.SetTip(tipText, tipDescription);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            questionMark.SetActive(true);
            tipManager.HideTip();
        }
    }

    void LateUpdate()
    {
        // Rotate the question mark around
        questionMark.transform.Rotate(new Vector3(0, 15 * Time.deltaTime, 0));
    }
}
