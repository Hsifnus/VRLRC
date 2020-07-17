using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles the process and visual effects of teleportation
public class PlayerTeleport : MonoBehaviour
{
    // Player's spawn point
    public GameObject spawnPoint;
    // Object containing the VR camera
    public GameObject cameraHolder;
    // How fast the player sinks in water or whatever else is in a pit
    public float sinkSpeed;
    // The player's rotation and position upon spawning
    private Quaternion spawnRotation;
    private Vector3 spawnPosition;
    // Player respawn timer
    private float respawnTime;
    // Controls the fade-to-color effect for VR camera
    private SteamVR_Fade fade;
    // Rigidbody of the player, to which sinking forces are applied
    private Rigidbody playerRigidbody;

    private bool isRespawning;

    // Get spawn transform data, player rigidbody, and fade effect
    void Start()
    {
        spawnPosition = spawnPoint.transform.position;
        spawnRotation = spawnPoint.transform.rotation;
        respawnTime = 2.0f;
        fade = cameraHolder.GetComponent<SteamVR_Fade>();
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        isRespawning = false;
    }

    // Start the respawn sequence if player hits water
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            // Mark player for respawning
            gameObject.transform.position -= new Vector3(0.0f, 0.1f, 0.0f);
            fade.OnStartFade(new Color(0.0f, 0.03f, 0.1f), 1.0f, false);
            if (isRespawning == false)
            {
              isRespawning = true;
              playerRigidbody.useGravity = false;
              playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, -sinkSpeed, playerRigidbody.velocity.z);
              Invoke("Respawn", respawnTime);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
      if (collision.gameObject.CompareTag("StaticPlatform"))
      {
        Vector3 platformPosition = collision.gameObject.transform.position;
        Vector3 playerPosition = gameObject.transform.position;
        platformPosition.y = 0;
        playerPosition.y = 0;
        Vector3 offset = (playerPosition - platformPosition).normalized;

        spawnPosition = gameObject.transform.position - offset;
        spawnRotation = gameObject.transform.rotation;
        print("Established new spawn point");
      }
    }

    // Moves the player back to the spawn point instantly
    public void Respawn()
    {
        gameObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        fade.OnStartFade(Color.clear, 1.0f, false);
        isRespawning = false;
        playerRigidbody.useGravity = true;
        print("Respawn Successful");
    }
}