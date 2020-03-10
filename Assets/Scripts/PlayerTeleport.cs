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

    // Get spawn transform data, player rigidbody, and fade effect
    void Start()
    {
        spawnPosition = spawnPoint.transform.position;
        spawnRotation = spawnPoint.transform.rotation;
        respawnTime = -1.0f;
        fade = cameraHolder.GetComponent<SteamVR_Fade>();
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Start the respawn sequence if player hits water
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            // Mark player for respawning
            respawnTime = 2.0f;
            gameObject.transform.position -= new Vector3(0.0f, 0.1f, 0.0f);
        }
    }

    // Moves the player back to the spawn point instantly
    void Respawn()
    {
        gameObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        fade.OnStartFade(Color.clear, 1.0f, false);
    }

    // Control fade-to-color effect based on current respawn timer
    void Update()
    {
        if (respawnTime > 0.0f)
        {
            float lastRespawnTime = respawnTime;
            respawnTime -= Time.deltaTime;
            // Start fade effect 1.5s before respawn
            if (lastRespawnTime >= 1.5f && respawnTime < 1.5f)
            {
                fade.OnStartFade(new Color(0.0f, 0.03f, 0.1f), 1.0f, false);
            }
            // If timer expires...
            if (respawnTime <= 0.0f)
            {
                // ...respawn the player
                respawnTime = -1.0f;
                Respawn();
            } else
            {
                // ...otherwise, sink the player more
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, -sinkSpeed, playerRigidbody.velocity.z);
            }
        }
    }
}
