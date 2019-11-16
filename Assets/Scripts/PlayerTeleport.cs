using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeleport : MonoBehaviour
{
    public GameObject spawnPoint;
    public GameObject cameraHolder;
    private Quaternion spawnRotation;
    private Vector3 spawnPosition;
    private float respawnTime;
    private SteamVR_Fade fade;

    void Start()
    {
        spawnPosition = spawnPoint.transform.position;
        spawnRotation = spawnPoint.transform.rotation;
        respawnTime = -1.0f;
        fade = cameraHolder.GetComponent<SteamVR_Fade>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            // Mark player for respawning
            respawnTime = 1.5f;
            fade.OnStartFade(Color.black, 1.0f, false);
        }
    }

    void Respawn()
    {
        gameObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        fade.OnStartFade(Color.clear, 0.5f, false);
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
            }
        }
    }
}
