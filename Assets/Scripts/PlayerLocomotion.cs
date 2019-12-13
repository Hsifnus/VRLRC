using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PlayerLocomotion : MonoBehaviour {

    public GameObject player;
    public GameObject eye;
    public GameObject pivot;
    public GameObject controller;
    public float speed;
    public float jumpSpeed;
    private SteamVR_TrackedController _controller;
    private SteamVR_PlayArea _playArea;
    private Rigidbody markerBody;
    private Collider markerCollider;
    private bool moving;
    private List<Vector3> groundedDeltas = new List<Vector3>() {
        new Vector3(0, 0, 0), new Vector3(0.1f, 0, 0),
        new Vector3(0.075f, 0, 0.075f), new Vector3(0, 0.1f, 0),
        new Vector3(-0.075f, 0, 0.075f), new Vector3(-0.1f, 0, 0),
        new Vector3(-0.075f, 0, -0.075f), new Vector3(0, -0.1f, 0),
        new Vector3(0.075f, 0, -0.075f) };

    // Use this for initialization
    void Start () {
        Debug.Log("Starting!");
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        _controller.PadClicked += Jump;
        _controller.PadTouched += Move;
        _controller.PadUntouched += Stop;
        _playArea = player.GetComponent<SteamVR_PlayArea>();
        Vector3 playAreaPos = _playArea.transform.position;
        pivot.transform.position = new Vector3(playAreaPos.x, playAreaPos.y + 0.25f, playAreaPos.z);
        markerBody = pivot.GetComponent<Rigidbody>();
        markerCollider = pivot.GetComponent<BoxCollider>();
        markerBody.freezeRotation = true;
        moving = false;
    }

    bool IsGrounded() {
        Vector3 pos = pivot.transform.position;
        foreach (Vector3 delta in groundedDeltas)
        {
            if (Physics.Raycast(pivot.transform.position + delta, -Vector3.up, markerCollider.bounds.extents.y + 0.1f))
            {
                return true;
            }
        }
        return false;
    }

    void Jump(object sender, ClickedEventArgs e)
    {
        Debug.Log("Jumping!");
        if (IsGrounded())
        {
            markerBody.velocity = new Vector3(markerBody.velocity.x, jumpSpeed, markerBody.velocity.z);
        }
    }

    void Move(object sender, ClickedEventArgs e)
    {
        Debug.Log("Moving!");
        moving = true;
    }

    void Stop(object sender, ClickedEventArgs e)
    {
        Debug.Log("Stopping!");
        moving = false;
    }


    // Update is called once per frame
    void Update () {
        if (moving)
        {
            Valve.VR.VRControllerAxis_t padInput = _controller.controllerState.rAxis0;
            float eyeAzimuth = eye.transform.rotation.eulerAngles.y;
            float padMagnitude = Mathf.Sqrt(padInput.x * padInput.x + padInput.y * padInput.y);
            float controlAzimuth = 0.0f;
            float radiansToDegrees = 360.0f / (2.0f * Mathf.PI);
            if (padInput.x != 0.0f)
            {
                controlAzimuth = Mathf.Atan(padInput.y / padInput.x) * radiansToDegrees;
                if (padInput.x < 0.0f)
                {
                    controlAzimuth += 180.0f;
                }
            }
            float inputX = speed * padMagnitude * Mathf.Cos((controlAzimuth - eyeAzimuth) / radiansToDegrees);
            float inputZ = speed * padMagnitude * Mathf.Sin((controlAzimuth - eyeAzimuth) / radiansToDegrees);
            markerBody.velocity = new Vector3(inputX, markerBody.velocity.y, inputZ);
        }
        Vector3 pivotPos = pivot.transform.position;
        _playArea.transform.position = new Vector3(pivotPos.x, pivotPos.y - 0.25f, pivotPos.z);
    }
}
