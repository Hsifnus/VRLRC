using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    //teleporter object
    public GameObject linkedTeleporter;

    public float cooldown;

    private string state;
    public string initialState = "on";
    public float warpTime = 0.5f;
    public SteamVR_Fade fade;

    void Start()
    {
        state = initialState;
    }

    private void WarpPlayer(GameObject player)
    {
        player.transform.SetPositionAndRotation(linkedTeleporter.transform.position, player.transform.rotation);
        fade.OnStartFade(Color.clear, 0.5f, false);
    }

    // Carry player to linked teleport and allow them to leave on a cooldown
    private void OnTriggerEnter(Collider collision)
    {
      GameObject player = collision.gameObject;
      if (player.CompareTag("Player") && state != "off")
      {
        linkedTeleporter.GetComponent<Collider>().enabled = false;
        StartCoroutine(HandlePlayerTeleport(player));
      }
    }

    IEnumerator HandlePlayerTeleport(GameObject player)
    {
        fade.OnStartFade(new Color(1f, 1f, 1f), warpTime, false);
        yield return new WaitForSeconds(warpTime);
        WarpPlayer(player);
        yield return new WaitForSeconds(cooldown);
        EnableTeleport();
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