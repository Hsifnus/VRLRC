using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Controller_State : MonoBehaviour {

    public bool triggerHeld;
    public bool triggerEntered;
    public GameObject controller;
    private Player_Controller _controller;

    // Use this for initialization
    void Start () {
        _controller = controller.GetComponent<Player_Controller>();
        triggerHeld = false;
        triggerEntered = false;
        _controller.PlayerTriggerClicked += HandleTriggerClicked;
        _controller.PlayerTriggerUnclicked += HandleTriggerUnclicked;
    }

    private void HandleTriggerClicked(object sender, PlayerControllerEventArgs e)
    {
        //Debug.Log("Trigger Clicked");
        triggerHeld = true;
    }

    private void HandleTriggerUnclicked(object sender, PlayerControllerEventArgs e)
    {
        //Debug.Log("Trigger Unclicked");
        triggerHeld = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(_controller.triggerPressed);
        triggerEntered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        triggerEntered = false;
    }

}
