using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

public class PlayerLocomotion_Offsite : MonoBehaviour
{

    public GameObject player;
    public GameObject head;
    public GameObject eye;
    public GameObject pivot;
    public GameObject controller;
    public float speed;
    public float jumpSpeed;
    private SDK_InputSimulator _controller;
    private Rigidbody markerBody;
    private Collider markerCollider;
    private bool moving;
    private List<Vector3> groundedDeltas = new List<Vector3>() {
        new Vector3(0, 0, 0), new Vector3(0.1f, 0, 0),
        new Vector3(0.075f, 0, 0.075f), new Vector3(0, 0, 0.1f),
        new Vector3(-0.075f, 0, 0.075f), new Vector3(-0.1f, 0, 0),
        new Vector3(-0.075f, 0, -0.075f), new Vector3(0, 0, -0.1f),
        new Vector3(0.075f, 0, -0.075f) };
    private float deltaX, deltaZ;
    public float stepSize = 0.05f;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Starting!");
        _controller = controller.GetComponent<SDK_InputSimulator>();
        Vector3 playerPos = player.transform.position;
        pivot.transform.position = new Vector3(playerPos.x, playerPos.y, playerPos.z);
        markerBody = pivot.GetComponent<Rigidbody>();
        markerCollider = pivot.GetComponent<BoxCollider>();
        markerBody.freezeRotation = true;
        moving = false;
        deltaX = deltaZ = 0f;
    }

    bool IsGrounded()
    {
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

    bool CanStep(Vector3 dir)
    {
        return !Physics.Raycast(pivot.transform.position + new Vector3(0, markerCollider.bounds.extents.y / 2), dir / dir.magnitude, stepSize);
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
    void Update()
    {
        if (Input.GetKey(_controller.buttonOneAlias))
        {
            ClickedEventArgs dummy = new ClickedEventArgs();
            Jump(this, dummy);
        }
        Valve.VR.VRControllerAxis_t padInput;
        padInput.x = Input.GetAxis("Horizontal") * 2;
        padInput.y = Input.GetAxis("Vertical") * 2;
        moving = padInput.x != 0 || padInput.y != 0;
        if (moving)
        {
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
            } else
            {
                controlAzimuth = padInput.y < 0 ? 270.0f : 90.0f;
            }
            float inputX = speed * padMagnitude * Mathf.Cos((controlAzimuth - eyeAzimuth) / radiansToDegrees);
            float inputZ = speed * padMagnitude * Mathf.Sin((controlAzimuth - eyeAzimuth) / radiansToDegrees);
            markerBody.velocity = new Vector3(inputX, markerBody.velocity.y, inputZ);
            Debug.Log("Marker Velocity: " + markerBody.velocity);
        }
        Vector3 pivotPos = pivot.transform.position;
        Vector3 headToPivot = new Vector3(head.transform.position.x - pivotPos.x, 0, head.transform.position.z - pivotPos.z);
        if (!moving && headToPivot.magnitude > 2 * stepSize && CanStep(headToPivot))
        {
            Vector3 step = headToPivot / headToPivot.magnitude * stepSize;
            deltaX += step.x;
            deltaZ += step.z;
            pivot.transform.position += step;
        }
        player.transform.position = new Vector3(pivotPos.x, pivotPos.y + 0.1f, pivotPos.z);
    }
}
