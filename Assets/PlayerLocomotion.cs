using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour {

    public GameObject player;
    public GameObject controller;
    private SteamVR_TrackedController _controller;

    // Use this for initialization
    void Start () {
        Debug.Log("Starting!");
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        _controller.PadClicked += Point;
        _controller.PadUnclicked += Move;
    }

    void Point(object sender, ClickedEventArgs e)
    {
        Debug.Log("Pointing!");
    }

    void Move(object sender, ClickedEventArgs e)
    {
        Debug.Log("Not Pointing!");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
