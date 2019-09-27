using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectState : MonoBehaviour {

    enum State { Passive, Active, Interacting };

    private State objectState;
    bool wasEmpty;

    Renderer rend;
    HashSet<GameObject> activators;
    
	// Use this for initialization
	void Start () {
        objectState = State.Passive;
        rend = GetComponent<Renderer>();
        activators = new HashSet<GameObject>();
        wasEmpty = true;
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (other.gameObject.tag == "Hand")
        {
            activators.Add(other.gameObject);
            Debug.Log("Activator count: " + activators.Count);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit");
        if (other.gameObject.tag == "Hand")
        {
            activators.Remove(other.gameObject);
            Debug.Log("Activator count: " + activators.Count);
        }
    }

    private void Update()
    {
        if (activators.Count == 0 && !wasEmpty)
        {
            wasEmpty = true;
            rend.material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f));
        }
        else if (activators.Count > 0 && wasEmpty) {
            wasEmpty = false;
            rend.material.SetColor("_Color", new Color(0.2f, 0.6f, 1.0f));
        }
    }

}
