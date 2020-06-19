using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    //teleporter object
    public GameObject linkedTeleporter;

    public float cooldown;

    private string state;
    public string initialState;

    void Start()
    {
        state = initialState;
    }

    // Carry player to linked teleport and allow them to leave on a cooldown
    private void OnTriggerEnter(Collider collision)
    {
      GameObject player = collision.gameObject;
      if (player.CompareTag("Player") && state != "off")
      {
        linkedTeleporter.GetComponent<Collider>().enabled = false;
        player.transform.SetPositionAndRotation(linkedTeleporter.transform.position, player.transform.rotation);
        Invoke("EnableTeleport", cooldown);
      }
    }

    // Enables teleported back from the linked teleporter
    private void EnableTeleport()
    {
      linkedTeleporter.GetComponent<Collider>().enabled = true;
    }

    // Sets teleporter state
    public void SetState(string newState)
    {
        state = newState;
    }
}