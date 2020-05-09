using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Interface between SteamVR controller and game
public class Player_Controller_Client : Photon.PunBehaviour, IPunCallbacks {

    // Game object containing SteamVR controller
    public GameObject controller;
    // Said SteamVR controller
    private SteamVR_TrackedController _controller;

    // Trigger press callbacks that the Controller_State_Client script provides
    public event PlayerControllerEventHandler PlayerTriggerClicked;
    public event PlayerControllerEventHandler PlayerTriggerUnclicked;

    private bool isLeft;

    // Initialize SteamVR controller and register interface callbacks into said controller
    override public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate 3");
        isLeft = gameObject.GetComponent<PlayerLocomotion_Client>().isLeft;
        controller = isLeft ? GameObject.FindGameObjectWithTag("BaseControllerLeft") : GameObject.FindGameObjectWithTag("BaseControllerRight");
        if (controller) InitController();
    }

    private void InitController()
    {
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        if (photonView.isMine)
        {
            _controller.TriggerClicked += HandleTriggerClicked;
            _controller.TriggerUnclicked += HandleTriggerUnclicked;
        }
    }

    private void Update()
    {
        if (!controller) {
            controller = isLeft ? GameObject.FindGameObjectWithTag("BaseControllerLeft") : GameObject.FindGameObjectWithTag("BaseControllerRight");
            if (controller) InitController();
        }
    }

    // Handles SteamVR trigger click
    private void HandleTriggerClicked(object sender,ClickedEventArgs e)
    {
        Debug.Log("HandleTriggerClicked");
        PlayerControllerEventArgs args = new PlayerControllerEventArgs();
        args.controllerIndex = e.controllerIndex;
        args.flags = e.flags;
        args.padX = e.padX;
        args.padY = e.padY;

        if (PlayerTriggerClicked != null)
        {
            // Pass args into Controller_State callback
            Debug.Log("HandleTriggerClicked w/ callback");
            PlayerTriggerClicked(this, args);
        }
    }

    // Handles SteamVR trigger unclick
    private void HandleTriggerUnclicked(object sender, ClickedEventArgs e)
    {
        Debug.Log("HandleTriggerUnclicked");
        PlayerControllerEventArgs args = new PlayerControllerEventArgs();
        args.controllerIndex = e.controllerIndex;
        args.flags = e.flags;
        args.padX = e.padX;
        args.padY = e.padY;

        if (PlayerTriggerUnclicked != null)
        {
            // Pass args into Controller_State callback
            Debug.Log("HandleTriggerUnclicked w/ callback");
            PlayerTriggerUnclicked(this, args);
        }
    }
}
