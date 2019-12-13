using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeleport : MonoBehaviour
{
    public GameObject spawnPoint;
    public GameObject cameraHolder;
    public float sinkSpeed;
    private Quaternion spawnRotation;
    private Vector3 spawnPosition;
    private float respawnTime;
    private SteamVR_Fade fade;
    private Rigidbody playerRigidbody;

    void Start()
    {
        spawnPosition = spawnPoint.transform.position;
        spawnRotation = spawnPoint.transform.rotation;
        respawnTime = -1.0f;
        fade = cameraHolder.GetComponent<SteamVR_Fade>();
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            // Mark player for respawning
            respawnTime = 2.0f;
            fade.OnStartFade(Color.blue, 1.0f, false);
            gameObject.transform.position -= new Vector3(0.0f, 0.1f, 0.0f);
        }
    }

    void Respawn()
    {
        gameObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        fade.OnStartFade(Color.clear, 1.0f, false);
    }

    void Update()
    {
        if (respawnTime > 0.0f)
        {
            respawnTime -= Time.deltaTime;
            if (respawnTime <= 0.0f)
            {
                respawnTime = -1.0f;
                Respawn();
            } else
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, -sinkSpeed, playerRigidbody.velocity.z);
            }
        }
    }
}
