using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Houses the logic behind platform movement
public class PlatformMovement_Client : Photon.PunBehaviour
{
    // Location of pos1
    Vector3 position1;
    // Location of pos2
    Vector3 position2;
    // GameObject used as start point
    public GameObject pos1;
    // GameObject used as end point
    public GameObject pos2;
    // How fast the platform moves between points
    public float speed;
    // Platform's rigidbody
    Rigidbody platform;
    // The destination point of the platform
    Vector3 goalpoint;
    // The platform's previous displacement from destination
    Vector3 prevdisp;
    // The platform's current displacement from destination
    Vector3 curdisp;
    // Full length of platform pause period
    public float maxpause;
    // Timer that is initialized to maxpause every time platform starts pausing
    private float pause;
    // Should this platform pause and turn around in response to hitting walls?
    public bool collisionResponse;
    // Should this platform pause and turn around in response to hitting throwable objects?
    public bool throwableResponse;
    // Whether this platform has collided with an obstruction en route to its destination
    private bool collided;

    public bool startOnContact;

    private string state;
    public string initialState;

    // Initialize platform parameters
    void Start()
    {
        state = initialState;
        position1 = pos1.transform.position;
        position2 = pos2.transform.position;
        transform.position = position1;
        platform = GetComponent<Rigidbody>();
        goalpoint = position2;
        prevdisp = new Vector3();
        pause = 0;
        collided = false;
    }

    // Run platform pause timer, determine when it is time to pause, and update displacements
    void Update()
    {
        if (!CanMove())
        {
            platform.velocity = new Vector3();
            return;
        }
        if (pause > 0)
        {
            pause -= Time.deltaTime;
            platform.velocity = new Vector3();
            return;
        }
        curdisp = transform.position - goalpoint;
        if (Vector3.Dot(curdisp, prevdisp) < 0 || collided)
        {
            pause = maxpause;
            platform.velocity = new Vector3();
            goalpoint = goalpoint == position1 ? position2 : position1;
            prevdisp = Vector3.Dot(curdisp, prevdisp) >= 0 ? transform.position - goalpoint : curdisp;
            collided = false;
            platform.velocity = new Vector3();
            return;
        }
        prevdisp = curdisp;
        platform.velocity = -speed * curdisp.normalized;
    }

    // Set the collided flag accordingly upon collision
    private void OnCollisionEnter(Collision collision)
    {
        GameObject thing = collision.gameObject;
        if (!collisionResponse || thing.CompareTag("Hand") || thing.CompareTag("Player"))
        {
            return;
        }
        if (!throwableResponse && thing.CompareTag("Throwable"))
        {
            return;
        }
        collided = true;
    }

    // Can the platform move?
    private bool CanMove() {
        return !(startOnContact || state == "off" || (goalpoint == position1 && state == "moveToPos2") || (goalpoint == position2 && state == "moveToPos1"));
    }

    // Trigger the platform to move on contact with player if startOnContact is enabled
    private void OnTriggerEnter(Collider collision)
    {
      GameObject player = collision.gameObject;
      if (player.CompareTag("Player"))
      {
        if (startOnContact)
        {
            startOnContact = false;
        }
      }
    }

    // Sets the state of the platform
    [PunRPC]
    public void SetState(string newState, bool master)
    {
        if (master && PhotonNetwork.isMasterClient)
        {
            state = newState;
            photonView.RPC("SetState", PhotonTargets.All, newState, false);
        } else if (!master && !PhotonNetwork.isMasterClient)
        {
            state = newState;
        }
    }
}