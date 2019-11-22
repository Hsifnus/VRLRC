using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    Vector3 position1;
    Vector3 position2;
    public GameObject pos1;
    public GameObject pos2;
    public float speed;
    Rigidbody platform;
    Vector3 goalpoint;
    Vector3 prevdisp;
    Vector3 curdisp;
    public float maxpause;
    private float pause;
    public bool collisionResponse;
    public bool throwableResponse;
    private bool collided;

    // Start is called before the first frame update
    void Start()
    {
        position1 = pos1.transform.position;
        position2 = pos2.transform.position;
        transform.position = position1;
        platform = GetComponent<Rigidbody>();
        goalpoint = position2;
        prevdisp = new Vector3();
        pause = 0;
        collided = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pause > 0)
        {
            pause -= Time.deltaTime;
            platform.velocity = new Vector3();
            return;
        }
        curdisp = transform.position - goalpoint;
        if (Vector3.Dot(curdisp, prevdisp) <0 || collided)
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
}
