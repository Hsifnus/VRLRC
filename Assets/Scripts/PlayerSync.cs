using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour {

    public GameObject vr_head;
    public GameObject vr_controller_left;
    public GameObject vr_controller_right;

    public GameObject player_head;
    public GameObject player_hand_left;
    public GameObject player_hand_right;


	// Use this for initialization
	void Start () {
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdatePosition(vr_head, player_head);
        UpdatePosition(vr_controller_left, player_hand_left);
        UpdatePosition(vr_controller_right, player_hand_right);
    }

    void UpdatePosition(GameObject source, GameObject target)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
}
