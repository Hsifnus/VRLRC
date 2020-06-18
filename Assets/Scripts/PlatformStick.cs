using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStick : MonoBehaviour
{
    private GameObject passenger;
    private Vector3 offset;

    void OnTriggerStay(Collider collision)
    {
      passenger = collision.gameObject;
        if (passenger.CompareTag("Player"))
        {
          Vector3 platformPosition = passenger.transform.position;
          Vector3 playerPosition = gameObject.transform.position;
          offset = playerPosition - platformPosition;
          if (passenger != null)
          {
            passenger.transform.position = gameObject.transform.position - offset;
          }
        }
    }

    void OnTriggerExit(Collider collision)
    {
      passenger = collision.gameObject;
        if (passenger.CompareTag("Player"))
        {
          passenger = null;
        }
    }
}