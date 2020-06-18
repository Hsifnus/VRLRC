using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    //teleporter object
    public GameObject linkedTeleporter;

    public float cooldown;

    private void OnTriggerEnter(Collider collision)
    {
      GameObject player = collision.gameObject;
      if (player.CompareTag("Player"))
      {
        linkedTeleporter.GetComponent<Collider>().enabled = false;
        player.transform.SetPositionAndRotation(linkedTeleporter.transform.position, player.transform.rotation);
        Invoke("EnableTeleport", cooldown);
      }
    }

    private void EnableTeleport()
    {
      linkedTeleporter.GetComponent<Collider>().enabled = true;
    }
}