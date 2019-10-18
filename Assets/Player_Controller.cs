using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public struct PlayerControllerEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float padX, padY;
}

public delegate void PlayerControllerEventHandler(object sender, PlayerControllerEventArgs e);

public class Player_Controller : MonoBehaviour {

    public GameObject controller;
    private SteamVR_TrackedController _controller;

    public event PlayerControllerEventHandler PlayerTriggerClicked;
    public event PlayerControllerEventHandler PlayerTriggerUnclicked;

    // Use this for initialization
    void Start()
    {
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += HandleTriggerClicked;
        _controller.TriggerUnclicked += HandleTriggerUnclicked;
    }

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
            Debug.Log("HandleTriggerClicked w/ callback");
            PlayerTriggerClicked(this, args);
        }
    }

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
            Debug.Log("HandleTriggerUnclicked w/ callback");
            PlayerTriggerUnclicked(this, args);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
