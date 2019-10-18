using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Controller_State : MonoBehaviour {

    public bool triggerHeld;
    public bool triggerEntered;
    public GameObject controller;
    private Player_Controller _controller;

    HashSet<GameObject> colliders, interactees;

    // Use this for initialization
    void Start () {
        _controller = controller.GetComponent<Player_Controller>();
        triggerHeld = false;
        triggerEntered = false;
        colliders = new HashSet<GameObject>();
        interactees = new HashSet<GameObject>();
        _controller.PlayerTriggerClicked += HandleTriggerClicked;
        _controller.PlayerTriggerUnclicked += HandleTriggerUnclicked;
    }

    private void HandleTriggerClicked(object sender, PlayerControllerEventArgs e)
    {
        Debug.Log("Trigger Clicked");
        triggerHeld = true;
        foreach (GameObject obj in colliders)
        {
            ObjectState state = obj.GetComponent<ObjectState>();
            if (state != null)
            {
                interactees.Add(obj);
                state.OnTriggerPress(this.gameObject);
            }
        }
    }

    private void HandleTriggerUnclicked(object sender, PlayerControllerEventArgs e)
    {
        Debug.Log("Trigger Unclicked");
        triggerHeld = false;
        PlayerForce force = GetComponent<PlayerForce>();
        foreach (GameObject obj in interactees)
        {
            ObjectState state = obj.GetComponent<ObjectState>();
            if (state != null)
            {
                state.OnTriggerRelease(this.gameObject);
                if (force != null)
                {
                    force.ApplyThrowForce(obj);
                }
            }
        }
        interactees.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggerHeld: " + triggerHeld);
        colliders.Add(other.gameObject);
        triggerEntered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other.gameObject);
        triggerEntered = false;
    }

    private void LateUpdate()
    {
        PlayerForce force = GetComponent<PlayerForce>();
        if (force != null)
        {
            foreach (GameObject obj in interactees)
            {
                force.ApplyForce(obj);
            }
        }
    }

}
