using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles the teleportation of objects through the Teleporter
public class Teleporter : MonoBehaviour
{
    //Teleporter to teleport to
    public GameObject linkedTeleporter;
    //Current puzzle state of teleporter
    private string state;
    //Initial puzzle state of teleporter
    public string initialState = "on";
    //How long the warp animation takes
    public float warpTime = 0.5f;
    //VR fade effect manager
    public SteamVR_Fade fade;
    //Location of linked Teleporter
    Vector3 linkedPosition;
    //Teleporter script attatched to the linked Teleporter
    Teleporter linkedScript;
    //Teleportable objects which should not be teleported
    HashSet<string> teleportBlacklist;
    //GameObject's material
    Material material;
    //Teleporter textures
    public Texture enabledTexture, disabledTexture;

    //Get linked Teleporter data and create blacklist
    void Start()
    {
        material = gameObject.GetComponent<Renderer>().material;
      state = initialState;
      linkedPosition = linkedTeleporter.transform.position;
      linkedScript = linkedTeleporter.GetComponent<Teleporter>();
      teleportBlacklist = new HashSet<string>();
        if (state == "off")
        {
            material.SetTexture("_MainTex", disabledTexture);
            material.SetFloat("_InvFade", 0.5f);
        }
        else if (state == "on")
        {
            material.SetTexture("_MainTex", enabledTexture);
            material.SetFloat("_InvFade", 3f);
        }
    }

    //Teleports the object entering if teleportable and not blacklisted
    private void OnTriggerEnter(Collider collision)
    {
      GameObject obj = collision.gameObject;
      if (IsTeleportable(obj) && !teleportBlacklist.Contains(obj.name) && state != "off")
      {
        //Adds object to linked teleporter blacklist so object does not instantly teleport back
        linkedScript.teleportBlacklist.Add(obj.name);
        //Play fade effect if player is being teleported
        if (obj.CompareTag("Player")) {
          StartCoroutine(HandlePlayerTeleport(obj));
        } else
        {
          obj.transform.SetPositionAndRotation(linkedTeleporter.transform.position, obj.transform.rotation);
        }
      }
    }

    //Removes teleportable objects leaving Teleporter from the blacklist
    private void OnTriggerExit(Collider collision)
    {
      GameObject obj = collision.gameObject;
      if (IsTeleportable(obj) && teleportBlacklist.Contains(obj.name))
      {
        teleportBlacklist.Remove(obj.name);
      }
    }

    IEnumerator HandlePlayerTeleport(GameObject player)
    {
      //Start fade to white
      fade.OnStartFade(new Color(1f, 1f, 1f), warpTime, false);
      yield return new WaitForSeconds(warpTime);
        //Clear fade and warp the player once warp time has elapsed
        Debug.Log(linkedTeleporter.transform.position);
      player.transform.SetPositionAndRotation(linkedTeleporter.transform.position, player.transform.rotation);
        Debug.Log(player.transform.position);
      fade.OnStartFade(Color.clear, 0.5f, false);
    }

    //Determines whether an object is of a teleportable type
    private bool IsTeleportable(GameObject obj)
    {
      return obj.CompareTag("Player") || obj.CompareTag("Throwable");
    }

    //Set state of teleporter
    public void SetState(string newState)
    {
        state = newState;
        if (state == "off")
        {
            material.SetTexture("_MainTex", disabledTexture);
            material.SetFloat("_InvFade", 0.5f);
        }
        else if (state == "on")
        {
            material.SetTexture("_MainTex", enabledTexture);
            material.SetFloat("_InvFade", 3f);
        }
    }
}