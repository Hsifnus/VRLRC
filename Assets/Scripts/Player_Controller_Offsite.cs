using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;


public class Player_Controller_Offsite : MonoBehaviour
{

    public GameObject controller;
    private SDK_InputSimulator _controller;
    private bool triggerClicked;
    private ClickedEventArgs e;

    public event PlayerControllerEventHandler PlayerTriggerClicked;
    public event PlayerControllerEventHandler PlayerTriggerUnclicked;

    void Start()
    {
        _controller = controller.GetComponent<SDK_InputSimulator>();
        e = new ClickedEventArgs();
        triggerClicked = false;
    }

    private void HandleTriggerClicked(object sender)
    {
        Debug.Log("HandleTriggerClicked");
        PlayerControllerEventArgs args = new PlayerControllerEventArgs();
        args.controllerIndex = e.controllerIndex;
        args.flags = e.flags;
        args.padX = e.padX;
        args.padY = e.padY;

        if (PlayerTriggerClicked != null)
        {
            Debug.Log("HandleTriggerClicked w/ callback");
            PlayerTriggerClicked(this, args);
        }
    }

    private void HandleTriggerUnclicked(object sender)
    {
        Debug.Log("HandleTriggerUnclicked");
        PlayerControllerEventArgs args = new PlayerControllerEventArgs();
        args.controllerIndex = e.controllerIndex;
        args.flags = e.flags;
        args.padX = e.padX;
        args.padY = e.padY;

        if (PlayerTriggerUnclicked != null)
        {
            Debug.Log("HandleTriggerUnclicked w/ callback");
            PlayerTriggerUnclicked(this, args);
        }
    }
    
    void Update()
    {
        bool newTriggerClicked = Input.GetKey(_controller.triggerAlias);
        if (newTriggerClicked && !triggerClicked)
        {
            e.flags = 0;
            e.padX = Input.GetAxis("Horizontal");
            e.padY = Input.GetAxis("Vertical");
            HandleTriggerClicked(this);
        } else if (!newTriggerClicked && triggerClicked)
        {
            e.flags = 0;
            e.padX = Input.GetAxis("Horizontal");
            e.padY = Input.GetAxis("Vertical");
            HandleTriggerUnclicked(this);
        }
        triggerClicked = newTriggerClicked;
    }
}
