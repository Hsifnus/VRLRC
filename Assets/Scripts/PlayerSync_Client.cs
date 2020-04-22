using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync_Client : Photon.PunBehaviour, IPunCallbacks {

    // SteamVR head and controllers
    public GameObject vr_head;
    public GameObject vr_controller_left;
    public GameObject vr_controller_right;

    // Player head and controllers
    public GameObject player_head;
    public GameObject player_hand_left;
    public GameObject player_hand_right;

    // Connect VR parts to script
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate 5");
        vr_controller_left = GameObject.FindGameObjectsWithTag("BaseControllerLeft")[0];
        vr_controller_right = GameObject.FindGameObjectsWithTag("BaseControllerRight")[0];
        vr_head = GameObject.FindGameObjectsWithTag("MainCamera")[0];
    }

    // Bind positions of player parts to SteamVR parts
    void FixedUpdate () {
        if (photonView.isMine)
        {
            UpdatePosition(vr_head, player_head);
            UpdatePosition(vr_controller_left, player_hand_left);
            UpdatePosition(vr_controller_right, player_hand_right);
        }
    }

    void UpdatePosition(GameObject source, GameObject target)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
}
