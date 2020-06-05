﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Updates controller state in response to controller inputs
public class Controller_State_Client : Photon.PunBehaviour {

    // Is controller trigger being held?
    public bool triggerHeld;
    // Is the hand touching another object?
    public bool triggerEntered;
    // The game object containing the Player_Controller script
    public GameObject controller;
    // The interface between the game and the SteamVR controller
    private Player_Controller_Client _controller;
    // Renders object-hand links
    private LineRenderer lineRenderer;
    // Color of non-stretched links
    private Color nearLinkColor;
    // Color of stretched links
    private Color farLinkColor = new Color(1.0f, 0.4f, 0.2f);
    // Was the link stretched last update? Used to prevent unneeded color updates
    private bool linkWasFar = false;
    // Object manager index
    private int objectIdx;
    // Local object manager
    private ObjectManager manager;
    
    // Colliders are objects that are currently colliding with this hand
    // Interactees are objects this hand is pulling
    // Objects that are too far from the hand are marked to be separated in current next update cycle
    HashSet<GameObject> colliders, interactees, toSeparate;

    // Initialize private parameters
    void Start () {
        _controller = controller.GetComponent<Player_Controller_Client>();
        triggerHeld = false;
        triggerEntered = false;
        colliders = new HashSet<GameObject>();
        interactees = new HashSet<GameObject>();
        toSeparate = new HashSet<GameObject>();
        _controller.PlayerTriggerClicked += HandleTriggerClicked;
        _controller.PlayerTriggerUnclicked += HandleTriggerUnclicked;
        lineRenderer = GetComponent<LineRenderer>();
        nearLinkColor = lineRenderer.material.color;
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<ObjectManager>();
    }

    // Callback used by Player_Controller to bind clicking the trigger to adding interactees
    private void HandleTriggerClicked(object sender, PlayerControllerEventArgs e)
    {
        Debug.Log("Trigger Clicked: " + objectIdx);
        triggerHeld = true;
        foreach (GameObject obj in colliders)
        {
            ObjectStateServer state = obj.GetComponent<ObjectStateServer>();
            if (state != null)
            {
                interactees.Add(obj);
                photonView.RPC("UpdateChangeLinks", PhotonTargets.All, state.GetObjectIndex(), false, false, false);
                PhotonView targetView = GameObject.FindGameObjectsWithTag("Manager")[0].GetComponent<PhotonView>();
                targetView.RPC("RelayOnTriggerPress", PhotonTargets.All, objectIdx, state.GetObjectIndex());
            }
        }
    }

    // Callback used by Player_Controller to bind unclicking the trigger to removing interactees
    private void HandleTriggerUnclicked(object sender, PlayerControllerEventArgs e)
    {
        Debug.Log("Trigger Unclicked");
        triggerHeld = false;
        PlayerForce force = GetComponent<PlayerForce>();
        foreach (GameObject obj in interactees)
        {
            ObjectStateServer state = obj.GetComponent<ObjectStateServer>();
            if (state != null)
            {
                PhotonView targetView = GameObject.FindGameObjectWithTag("Manager").GetComponent<PhotonView>();
                targetView.RPC("RelayOnTriggerRelease", PhotonTargets.All, objectIdx, state.GetObjectIndex());
            }
        }
        interactees.Clear();
        photonView.RPC("UpdateChangeLinks", PhotonTargets.All, 0, false, true, false);
    }

    // Add to colliders anything the hand touches
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggerHeld: " + triggerHeld);
        colliders.Add(other.gameObject);
        triggerEntered = true;
    }

    // Remove from colliders anything the hand is no longer touching
    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other.gameObject);
        triggerEntered = false;
    }
    
    private void LateUpdate()
    {
        // 3. Use interactee positions to compute object-hand link endpoints
        List<Vector3> positions = new List<Vector3>();
        positions.Add(gameObject.transform.position);
        lineRenderer.positionCount = 1 + 2 * interactees.Count;
        bool isFar = false;
        foreach (GameObject obj in interactees)
        {
            positions.Add(obj.transform.position);
            positions.Add(gameObject.transform.position);
            isFar = isFar || (obj.transform.position - gameObject.transform.position).magnitude >= 1.0f;
        }
        lineRenderer.SetPositions(positions.ToArray());
        // 4. Update link color depending on whether any one link is stretched or not
        if (isFar != linkWasFar)
        {
            linkWasFar = isFar;
            if (isFar)
            {
                lineRenderer.material.SetColor("_Color", farLinkColor);
                lineRenderer.material.SetColor("_EmissionColor", farLinkColor);
            }
            else
            {
                lineRenderer.material.SetColor("_Color", nearLinkColor);
                lineRenderer.material.SetColor("_EmissionColor", nearLinkColor);
            }
        }
    }
    
    // Sets the object index of the controller
    [PunRPC]
    public void SetObjectIndex(int idx)
    {
        objectIdx = idx;
        Debug.Log("SetObjectIndex: " + objectIdx);
    }

    // Gets the object index of the controller
    public int GetObjectIndex()
    {
        return objectIdx;
    }

    // Removes a set of interactees from the controller
    public void RemoveInteractee(int ctrl, int[] objs)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("removing interactee!");
            if (objectIdx == ctrl)
            {
                Debug.Log("ctrl: " + ctrl);
                foreach (int obj in objs)
                {
                    Debug.Log("obj: " + obj);
                    interactees.Remove(manager.GetThrowableObj(obj));
                    photonView.RPC("UpdateChangeLinks", PhotonTargets.All, obj, true, false, true);
                }
            }
        }
    }

    // Updates the controller state across all other clients
    [PunRPC]
    public void UpdateChangeLinks(int index, bool remove, bool clear, bool force)
    {
        if (force || !photonView.isMine)
        {
            Debug.Log("UpdateChangeLinks: " + index);
            GameObject obj = manager.GetThrowableObj(index);
            if (remove)
            {
                interactees.Remove(obj);
                Debug.Log("Interactee count: " + interactees.Count);
                foreach(GameObject interactee in interactees)
                {
                    Debug.Log("Index: " + interactee.GetComponent<ObjectStateServer>().GetObjectIndex());
                }
            }
            else if (clear)
            {
                interactees.Clear();
            }
            else
            {
                interactees.Add(obj);
            }
        }
    }
}
