using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Data structure containing controller parameters
public struct PlayerControllerEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float padX, padY;
}

// Event type under which trigger press callbacks are implemented
public delegate void PlayerControllerEventHandler(object sender, PlayerControllerEventArgs e);

// Interface between SteamVR controller and game
public class Player_Controller : MonoBehaviour {

    // Game object containing SteamVR controller
    public GameObject controller;
    // Said SteamVR controller
    private SteamVR_TrackedController _controller;

    // Trigger press callbacks that the Controller_State script provides
    public event PlayerControllerEventHandler PlayerTriggerClicked;
    public event PlayerControllerEventHandler PlayerTriggerUnclicked;

    // Initialize SteamVR controller and register interface callbacks into said controller
    void Start()
    {
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += HandleTriggerClicked;
        _controller.TriggerUnclicked += HandleTriggerUnclicked;
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
