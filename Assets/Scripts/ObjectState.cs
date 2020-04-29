using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectState : Photon.PunBehaviour {

    // Passive - No hands are touching or interacting with this object
    // Active - There exists hands making contact with this object, but no hands are interacting
    // Interacting - There exists hands that are interacting with (i.e. pulling on) this object
    public enum State { Passive, Active, Interacting };

    // Current state of the object
    private State objectState;
    // Object manager index
    private int objectIdx;
    // Color constants for each of the three possible states
    private Color passiveColor = new Color(1.0f, 1.0f, 1.0f);
    private Color activeColor = new Color(0.2f, 0.6f, 1.0f);
    private Color interactingColor = new Color(0.2f, 1.0f, 0.7f);
    // Respawn timer used when object falls through water
    private float respawnTimer = -1.0f;
    // Spawn point transform data
    private Vector3 spawnLocation;
    private Quaternion spawnRotation;
    // Object rigidbody, used for sinking
    private Rigidbody objRigidbody;
    private float sinkSpeed = 0.3f;
    // Whether the object had an empty activators array or nonempty interactors array
    // Both are used to prevent unnecessary object color updates
    bool wasEmpty, wasInteracting;

    // Object's material renderer
    Renderer rend;
    // Activators are hands that are in contact with the object
    // Interactors are hands that are interacting with the object
    HashSet<GameObject> activators, interactors;
    // Object Manager
    ObjectManager manager;

    // Initialize private parameters
    void Start() {
        objectIdx = -1;
        spawnLocation = gameObject.transform.position;
        spawnRotation = gameObject.transform.rotation;
        objRigidbody = gameObject.GetComponent<Rigidbody>();
        objectState = State.Passive;
        rend = GetComponent<Renderer>();
        activators = new HashSet<GameObject>();
        interactors = new HashSet<GameObject>();
        wasEmpty = true;
        wasInteracting = false;
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<ObjectManager>();
    }

    // Respawns the object at its spawn point, resetting internal state in the process
    private void Respawn()
    {
        gameObject.transform.position = spawnLocation;
        gameObject.transform.rotation = spawnRotation;
        objRigidbody.angularVelocity = new Vector3();
        objRigidbody.velocity = new Vector3();
        objectState = State.Passive;
        activators = new HashSet<GameObject>();
        interactors = new HashSet<GameObject>();
        wasEmpty = true;
        wasInteracting = false;
    }

    // Add activator if object is touched by a player hand
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        Debug.Log(other.gameObject.CompareTag("Hand"));
        if (other.gameObject.CompareTag("Hand"))
        {
            activators.Add(other.gameObject);
            photonView.RPC("UpdateChangeLinks", PhotonTargets.All,
                other.gameObject.GetComponent<Controller_State_Client>().GetObjectIndex(),
                false, false);
            if (objectState != State.Active && interactors.Count == 0)
            {
                objectState = State.Active;
                CallUpdateObjectState(true);
            }
            Debug.Log("Activator count: " + activators.Count);
        }
        if (other.gameObject.CompareTag("Water"))
        {
            respawnTimer = 2.0f;
            gameObject.transform.position -= new Vector3(0, 0.2f, 0);
        }
    }

    // Remove activator if object is no longer touched by a player hand
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit");
        Debug.Log(other.gameObject.CompareTag("Hand"));
        if (other.gameObject.CompareTag("Hand"))
        {
            activators.Remove(other.gameObject);
            photonView.RPC("UpdateChangeLinks", PhotonTargets.All,
                other.gameObject.GetComponent<Controller_State_Client>().GetObjectIndex(),
                false, true);
            if (activators.Count == 0 && interactors.Count == 0)
            {
                objectState = State.Passive;
                CallUpdateObjectState(true);
            }
            Debug.Log("Activator count: " + activators.Count);
        }
    }

    // Add interactor if activator presses trigger
    public void OnTriggerPress(GameObject controller)
    {
        Debug.Log("OnTriggerPress");
        if (objectState != State.Passive)
        {
            if (objectState == State.Active)
            {
                objectState = State.Interacting;
                CallUpdateObjectState(false);
            }
            interactors.Add(controller);
            photonView.RPC("UpdateChangeLinks", PhotonTargets.All, 
                controller.GetComponent<Controller_State_Client>().GetObjectIndex(),
                true, false);
            Debug.Log("Interactor count: " + interactors.Count);
        }
    }

    // Remove interactor if said interactor releases trigger
    public void OnTriggerRelease(GameObject controller)
    {
        Debug.Log("OnTriggerRelease");
        if (objectState != State.Passive)
        {
            interactors.Remove(controller);
            photonView.RPC("UpdateChangeLinks", PhotonTargets.All,
                controller.GetComponent<Controller_State_Client>().GetObjectIndex(),
                true, true);
            if (interactors.Count == 0)
            {
                if (activators.Count == 0)
                {
                    objectState = State.Passive;
                } else
                {
                    objectState = State.Active;
                }
                CallUpdateObjectState(false);
            }
            Debug.Log("Interactor count: " + interactors.Count);
        }
    }

    private void CallUpdateObjectState(bool force)
    {
        photonView.RPC("UpdateObjectState", PhotonTargets.All, Convert.ChangeType(objectState, objectState.GetTypeCode()), force);
    }

    private void Update()
    {
        // 1. Sink then respawn object with a delay if it fell into water
        if (respawnTimer > 0.0f)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0.0f)
            {
                Respawn();
                respawnTimer = -1.0f;
                objRigidbody.velocity = new Vector3();
            } else
            {
                objRigidbody.velocity = new Vector3(objRigidbody.velocity.x, -sinkSpeed, objRigidbody.velocity.z);
                gameObject.transform.position -= new Vector3(0, 0.1f, 0);
            }
        }
        // 2. Update object color depending on current object state
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

    // Returns the current state of the object
    public State getState()
    {
        return objectState;
    }

    // Returns the current interactors of the object
    public HashSet<GameObject> GetInteractors()
    {
        return interactors;
    }

    // Sets the object index of the object
    public void SetObjectIndex(int idx)
    {
        objectIdx = idx;
    }

    // Gets the object index of the object
    public int GetObjectIndex()
    {
        return objectIdx;
    }

    // Updates the object state across all clients
    [PunRPC]
    public void UpdateObjectState(int state, bool force)
    {
        if (force || !PhotonNetwork.isMasterClient)
        {
            objectState = (State) state;
        }
    }

    // Updates the object state across all clients
    [PunRPC]
    public void UpdateChangeLinks(int index, bool interact, bool remove)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            GameObject hand = manager.GetController(index);
            if (interact)
            {
                if (remove)
                {
                    interactors.Remove(hand);
                } else
                {
                    interactors.Add(hand);
                }
            } else
            {
                if (remove)
                {
                    activators.Remove(hand);
                }
                else
                {
                    activators.Add(hand);
                }
            }
        }
    }
}
