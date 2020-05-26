using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PlayerLocomotion : MonoBehaviour {

    // Objects containing player and their head, eye, and pivot
    public GameObject player;
    public GameObject head;
    public GameObject eye;
    public GameObject pivot;
    // Object containing SteamVR controller
    public GameObject controller;
    // Move and jump speed
    public float speed;
    public float jumpSpeed;
    // Whether this controller is the left controller or not
    public bool isLeft = false;
    // SteamVR controller
    private SteamVR_TrackedController _controller;
    // SteamVR play area (this is the Guardian area for Oculus)
    private SteamVR_PlayArea _playArea;
    // Rigidbody and collider of pivot
    private Rigidbody markerBody;
    private Collider markerCollider;
    // Whether the pivot is currently moving under controller input
    private bool moving;
    // List of offset vectors used in checking whether the pivot is grounded
    // These points roughly form a circle of radius 0.1f
    private List<Vector3> groundedDeltas = new List<Vector3>() {
        new Vector3(0, 0, 0), new Vector3(0.1f, 0, 0),
        new Vector3(0.075f, 0, 0.075f), new Vector3(0, 0, 0.1f),
        new Vector3(-0.075f, 0, 0.075f), new Vector3(-0.1f, 0, 0),
        new Vector3(-0.075f, 0, -0.075f), new Vector3(0, 0, -0.1f),
        new Vector3(0.075f, 0, -0.075f) };
    // Offset of pivot relative to center of play area
    private float deltaX, deltaZ;
    // How far pivot should step while adjusting to head position
    public float stepSize = 0.05f;
    // Yaw angle
    private float yaw;

    // Register movement functions to SteamVR Controller
    void Start () {
        _controller = controller.GetComponent<SteamVR_TrackedController>();
        if (!isLeft)
        {
            _controller.PadClicked += Jump;
        }
        _controller.PadTouched += Move;
        _controller.PadUntouched += Stop;
        _playArea = player.GetComponent<SteamVR_PlayArea>();
        Vector3 playAreaPos = _playArea.transform.position;
        pivot.transform.position = new Vector3(playAreaPos.x, playAreaPos.y + 0.25f, playAreaPos.z);
        markerBody = pivot.GetComponent<Rigidbody>();
        markerCollider = pivot.GetComponent<BoxCollider>();
        markerBody.freezeRotation = true;
        moving = false;
        deltaX = deltaZ = 0f;
        yaw = 0f;
    }

    // Does raycast check from a circle of testpoints near the pivot center to see if pivot is grounded
    bool IsGrounded() {
        Vector3 pos = pivot.transform.position;
        foreach (Vector3 delta in groundedDeltas)
        {
            Ray ray = new Ray(pivot.transform.position + delta, -Vector3.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, markerCollider.bounds.extents.y + 0.1f))
            {
                ObjectState state = hit.collider.gameObject.GetComponent<ObjectState>();
                // Cannot jump off of hands or objects we are pulling - prevents infinite jump exploit
                if (!hit.collider.gameObject.CompareTag("Hand") && (!state || state.getState() != ObjectState.State.Interacting))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Does raycast check from pivot center along direction of dir
    bool CanStep(Vector3 dir)
    {
        return !Physics.Raycast(pivot.transform.position + new Vector3(0, markerCollider.bounds.extents.y / 2), dir / dir.magnitude, stepSize);
    }
    
    // Makes the player jump
    void Jump(object sender, ClickedEventArgs e)
    {
        if (IsGrounded())
        {
            markerBody.velocity = new Vector3(markerBody.velocity.x, jumpSpeed, markerBody.velocity.z);
        }
    }

    // Lets the player start moving via controller input
    void Move(object sender, ClickedEventArgs e)
    {
        moving = true;
    }

    // Stops the player from moving farther via controller input
    void Stop(object sender, ClickedEventArgs e)
    {
        Debug.Log("Stopping!");
        moving = false;
    }


    void Update () {
        // 1. If moving, set velocity to the direction the controller stick is pointing
        if (moving)
        {
            // Get joystick pad input
            Valve.VR.VRControllerAxis_t padInput = _controller.controllerState.rAxis0;
            // Right controller handles translation
            if (!isLeft)
            {
                // XZ plane orientation of camera
                float eyeAzimuth = eye.transform.rotation.eulerAngles.y;
                // Magnitude of pad input
                float padMagnitude = Mathf.Sqrt(padInput.x * padInput.x + padInput.y * padInput.y);
                // Joystick orientation
                float controlAzimuth = 0.0f;
                // Radians-to-degrees conversion factor
                float radiansToDegrees = 360.0f / (2.0f * Mathf.PI);
                // Computes degree orientation of joystick from pad inputs
                if (padInput.x != 0.0f)
                {
                    controlAzimuth = Mathf.Atan(padInput.y / padInput.x) * radiansToDegrees;
                    if (padInput.x < 0.0f) // Atan has limited codomain, so we expand that to full angle
                    {
                        controlAzimuth += 180.0f;
                    }
                }
                else
                {
                    controlAzimuth = padInput.y < 0.0f ? 270.0f : 90.0f;
                }
                // Compute true X and Z velocity of player using eye and joystick angles, speed, and pad input magnitude
                float inputX = speed * padMagnitude * Mathf.Cos((controlAzimuth - eyeAzimuth) / radiansToDegrees);
                float inputZ = speed * padMagnitude * Mathf.Sin((controlAzimuth - eyeAzimuth) / radiansToDegrees);
                markerBody.velocity = new Vector3(inputX, markerBody.velocity.y, inputZ);
            }
            else // Left controller handles rotation
            {
                // Compute yaw for rotation
                yaw += speed * padInput.x / 7200;
                markerBody.MoveRotation(new Quaternion(0, Mathf.Sin(yaw), 0, Mathf.Cos(yaw)));
            }
        }
        if (!isLeft)
        {
            // 2. Use head-pivot displacement along XZ plane to sync pivot to be under head position
            Vector3 pivotPos = pivot.transform.position;
            Vector3 headToPivot = new Vector3(head.transform.position.x - pivotPos.x, 0, head.transform.position.z - pivotPos.z);
            if (headToPivot.magnitude > 2 * stepSize && CanStep(headToPivot))
            {
                Vector3 step = headToPivot / headToPivot.magnitude * stepSize;
                deltaX += step.x;
                deltaZ += step.z;
                pivot.transform.position += step;
                pivotPos = pivot.transform.position;
            }
            // 3. Bind the play are position to that of the pivot, with an offset
            _playArea.transform.position = new Vector3(pivotPos.x - deltaX, pivotPos.y - 0.25f, pivotPos.z - deltaZ);
        }
        else
        {
            _playArea.transform.rotation = pivot.transform.rotation;
        }
    }
}
