using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync_Offsite : MonoBehaviour {

    // SteamVR head and controllers
    public GameObject vr_head;
    public GameObject vr_controller_left;
    public GameObject vr_controller_right;

    // Player head and controllers
    public GameObject player_head;
    public GameObject player_hand_left;
    public GameObject player_hand_right;

    // UIs that stick to the player
    public GameObject objective_ui;
    public GameObject tip_ui;

    // Bind positions of player parts to SteamVR parts
    void FixedUpdate () {
        UpdatePosition(vr_head, player_head);
        // Offsite demo fixes player hand positions - no more need to manually move hands around anymore!
        UpdatePosition(vr_head, player_hand_left);
        UpdatePosition(vr_head, player_hand_right);
        player_hand_left.transform.position += player_head.transform.rotation * new Vector3(0f, 0.2f, 0.3f);
        player_hand_left.transform.position += player_head.transform.rotation * new Vector3(0f, 0.2f, 0.3f);
        // Update UI position to be in front of player camera
        UpdatePosition(player_head, objective_ui);
        objective_ui.transform.position += player_head.transform.rotation * new Vector3(0f, -0.1f, 0.4f);
        UpdatePosition(player_head, tip_ui);
        tip_ui.transform.position += player_head.transform.rotation * new Vector3(0f, -0.1f, 0.4f);
    }

    void UpdatePosition(GameObject source, GameObject target)
    {
        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
}
