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

    private float bindTimer = 1.0f;

    // Connect VR parts to script
    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate 5");
        vr_controller_left = GameObject.FindGameObjectWithTag("BaseControllerLeft");
        vr_controller_right = GameObject.FindGameObjectWithTag("BaseControllerRight");
        vr_head = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Bind positions of player parts to SteamVR parts
    void FixedUpdate () {
        bindTimer -= Time.deltaTime;
        if (bindTimer < 0)
        {
            bindTimer = 1.0f;
            vr_head = GameObject.FindGameObjectWithTag("MainCamera");
            vr_controller_left = GameObject.FindGameObjectWithTag("BaseControllerLeft");
            vr_controller_right = GameObject.FindGameObjectWithTag("BaseControllerRight");
        }
        if (photonView.isMine)
        {
            if (vr_head) UpdatePosition(vr_head, player_head);
            if (vr_controller_left) UpdatePosition(vr_controller_left, player_hand_left);
            if (vr_controller_right) UpdatePosition(vr_controller_right, player_hand_right);
        }
    }

    void UpdatePosition(GameObject source, GameObject target)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
}
