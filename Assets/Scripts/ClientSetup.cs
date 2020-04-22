using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSetup : Photon.PunBehaviour, IPunCallbacks
{
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate 1");
        GameObject player = gameObject.transform.GetChild(0).gameObject;
        player.GetComponent<PlayerSync_Client>().OnPhotonInstantiate(info);
        GameObject pivot = player.transform.GetChild(0).gameObject;
        pivot.GetComponent<PlayerTeleport_Client>().OnPhotonInstantiate(info);
        GameObject leftPlayerControl = gameObject.transform.GetChild(1).gameObject;
        leftPlayerControl.GetComponent<Player_Controller_Client>().OnPhotonInstantiate(info);
        leftPlayerControl.GetComponent<PlayerLocomotion_Client>().OnPhotonInstantiate(info);
        GameObject rightPlayerControl = gameObject.transform.GetChild(2).gameObject;
        rightPlayerControl.GetComponent<Player_Controller_Client>().OnPhotonInstantiate(info);
        rightPlayerControl.GetComponent<PlayerLocomotion_Client>().OnPhotonInstantiate(info);
    }
}
