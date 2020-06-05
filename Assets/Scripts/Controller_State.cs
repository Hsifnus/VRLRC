using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Updates controller state in response to controller inputs
public class Controller_State : MonoBehaviour {

    // Is controller trigger being held?
    public bool triggerHeld;
    // Is the hand touching another object?
    public bool triggerEntered;
    // The game object containing the Player_Controller script
    public GameObject controller;
    // The interface between the game and the SteamVR controller
    private Player_Controller _controller;
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
    
    // Colliders are objects that are currently colliding with this hand
    // Interactees are objects this hand is pulling
    // Objects that are too far from the hand are marked to be separated in current next update cycle
    HashSet<GameObject> colliders, interactees, toSeparate;

    // Initialize private parameters
    void Start () {
        objectIdx = -1;
        _controller = controller.GetComponent<Player_Controller>();
        triggerHeld = false;
        triggerEntered = false;
        colliders = new HashSet<GameObject>();
        interactees = new HashSet<GameObject>();
        toSeparate = new HashSet<GameObject>();
        _controller.PlayerTriggerClicked += HandleTriggerClicked;
        _controller.PlayerTriggerUnclicked += HandleTriggerUnclicked;
        lineRenderer = GetComponent<LineRenderer>();
        nearLinkColor = lineRenderer.material.color;
    }

    // Callback used by Player_Controller to bind clicking the trigger to adding interactees
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
            } else if (obj.CompareTag("Lever"))
            {
                interactees.Add(obj);
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
        // 1. Apply pull force to interactees
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
        // 2. Release any interactees marked for separation
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
        // 3. Use interactee positions to compute object-hand link endpoints
        List<Vector3> positions = new List<Vector3>();
        positions.Add(gameObject.transform.position);
        lineRenderer.positionCount = 1 + 2 * interactees.Count;
        bool isFar = false;
        foreach (GameObject obj in interactees)
        {
            Vector3 pos = obj.CompareTag("Lever") ? obj.GetComponent<LeverState>().GetHandlePos() : obj.transform.position;
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
    public void SetObjectIndex(int idx)
    {
        objectIdx = idx;
    }

}
