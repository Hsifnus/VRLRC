using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectState : MonoBehaviour {

    enum State { Passive, Active, Interacting };

    private State objectState;
    private Color passiveColor = new Color(1.0f, 1.0f, 1.0f);
    private Color activeColor = new Color(0.2f, 0.6f, 1.0f);
    private Color interactingColor = new Color(0.2f, 1.0f, 0.7f);
    bool wasEmpty, wasInteracting;

    Renderer rend;
    HashSet<GameObject> activators, interactors;
    
	// Use this for initialization
	void Start () {
        objectState = State.Passive;
        rend = GetComponent<Renderer>();
        activators = new HashSet<GameObject>();
        interactors = new HashSet<GameObject>();
        wasEmpty = true;
        wasInteracting = false;
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        Debug.Log(other.gameObject.CompareTag("Hand"));
        if (other.gameObject.CompareTag("Hand"))
        {
            activators.Add(other.gameObject);
            if (objectState != State.Active && interactors.Count == 0)
            {
                objectState = State.Active;
            }
            Debug.Log("Activator count: " + activators.Count);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit");
        Debug.Log(other.gameObject.CompareTag("Hand"));
        if (other.gameObject.CompareTag("Hand"))
        {
            activators.Remove(other.gameObject);
            if (activators.Count == 0 && interactors.Count == 0)
            {
                objectState = State.Passive;
            }
            Debug.Log("Activator count: " + activators.Count);
        }
    }

    public void OnTriggerPress(GameObject controller)
    {
        Debug.Log("OnTriggerPress");
        if (objectState != State.Passive)
        {
            if (objectState == State.Active)
            {
                objectState = State.Interacting;
            }
            interactors.Add(controller);
            Debug.Log("Interactor count: " + interactors.Count);
        }
    }

    public void OnTriggerRelease(GameObject controller)
    {
        Debug.Log("OnTriggerRelease");
        if (objectState != State.Passive)
        {
            interactors.Remove(controller);
            if (interactors.Count == 0)
            {
                if (activators.Count == 0)
                {
                    objectState = State.Passive;
                } else
                {
                    objectState = State.Active;
                }
            }
            Debug.Log("Interactor count: " + interactors.Count);
        }
    }

    private void Update()
    {
        if (interactors.Count == 0 && activators.Count == 0 && (!wasEmpty || wasInteracting))
        {
            if (wasInteracting)
            {
                wasInteracting = false;
            }
            wasEmpty = true;
            rend.material.SetColor("_Color", passiveColor);
        }
        else if (interactors.Count == 0 && activators.Count > 0 && (wasEmpty || wasInteracting))
        {
            if (wasInteracting)
            {
                wasInteracting = false;
            }
            wasEmpty = false;
            rend.material.SetColor("_Color", activeColor);
        }
        else if (interactors.Count > 0 && !wasInteracting)
        {
            wasInteracting = true;
            rend.material.SetColor("_Color", interactingColor);
        }
    }

}
