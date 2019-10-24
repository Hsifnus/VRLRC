﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Controller_State : MonoBehaviour {

    public bool triggerHeld;
    public bool triggerEntered;
    public GameObject controller;
    private Player_Controller _controller;
    private LineRenderer lineRenderer;

    HashSet<GameObject> colliders, interactees, toSeparate;

    // Use this for initialization
    void Start () {
        _controller = controller.GetComponent<Player_Controller>();
        triggerHeld = false;
        triggerEntered = false;
        colliders = new HashSet<GameObject>();
        interactees = new HashSet<GameObject>();
        toSeparate = new HashSet<GameObject>();
        _controller.PlayerTriggerClicked += HandleTriggerClicked;
        _controller.PlayerTriggerUnclicked += HandleTriggerUnclicked;
        lineRenderer = GetComponent<LineRenderer>();
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
                if(!force.ApplyForce(obj))
                {
                    toSeparate.Add(obj);
                }
            }
        }
        foreach (GameObject obj in toSeparate)
        {
            interactees.Remove(obj);
            ObjectState state = obj.GetComponent<ObjectState>();
            if (state != null)
            {
                state.OnTriggerRelease(this.gameObject);
            }
        }
        toSeparate.Clear();
        List<Vector3> positions = new List<Vector3>();
        positions.Add(gameObject.transform.position);
        lineRenderer.positionCount = 1 + 2 * interactees.Count;
        foreach (GameObject obj in interactees)
        {
            positions.Add(obj.transform.position);
            positions.Add(gameObject.transform.position);
        }
        lineRenderer.SetPositions(positions.ToArray());
    }

}
