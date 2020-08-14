using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateState : MonoBehaviour
{
    // The weight at which the pressure plate hits maximum activation.
    public float weightCapacity = 1f;
    // The weight at which the pressure plate hits minimum activation.
    public float weightThreshold = 0f;
    // How often the pressure plate updates activation.
    // This helps prevent the plate from jittering due to blocks shifting around in position.
    public float updateInterval = 1f;
    // The amount the pressure plate is pressed.
    private float activation;
    // The displayed amount the pressure plate is pressed, which goes towards the actual activation value in a smoother, linear fashion.
    private float interpolatedActivation;
    // The amount of weight currently present on the pressure plate.
    private float weight;
    // Timer that shows how much time is left until the next activation update.
    private float updateTime;
    // Flag that determines whether the weight has changed or not.
    private bool weightHasChanged;
    // Rigidbody component cache that helps lessen GetComponent calls.
    private Hashtable rigidbodyCache;
    // The pressure plate's collider, which has to adjust along with the squishing of the plate itself.
    private BoxCollider boxCollider;
    // Reference to the scenes' puzzle manager, which gives a channel through which the pressure plate
    // can affect puzzle logic in the scene.
    private PuzzleManager puzzleManager;
    // Optional progress bar that can be used to display the activation of the pressure plate more clearly.
    public GameObject progressBar;
    // Actual bar object
    private GameObject _progressBar;

    void Start()
    {
        puzzleManager = GameObject.Find("Puzzle Manager").GetComponent<PuzzleManager>();
        updateTime = updateInterval;
        activation = 0f;
        interpolatedActivation = 0f;
        weight = 0f;
        rigidbodyCache = new Hashtable();
        boxCollider = gameObject.GetComponent<BoxCollider>();
        if (weightCapacity <= weightThreshold)
        {
            throw new UnityException("Weight capacity must be greater than weight threshold!\nCurrent capacity: " + weightCapacity + "\nCurrent threshold: " + weightThreshold);
        }
        if (weightCapacity < 0 || weightThreshold < 0)
        {
            throw new UnityException("Weight capacity or capacity cannot be negative!\nCurrent capacity: " + weightCapacity + "\nCurrent threshold: " + weightThreshold);
        }
        if (progressBar != null)
        {
            _progressBar = progressBar.transform.GetChild(4).gameObject;
        }
    }

    private Rigidbody GetRigidbody(Collider other) // Fetches the rigidbody of the collider via a cache to reduce the number of GetComponent calls
    {
        if (rigidbodyCache.Contains(other.gameObject)) // Cache hit, use cached rigidbody
        {
            return (Rigidbody) rigidbodyCache[other.gameObject];
        }
        else // Cache miss, obtain rigidbody from collider and add rigidbody to cache
        {
            Rigidbody rigidbody = other.gameObject.GetComponent<Rigidbody>();
            rigidbodyCache[other.gameObject] = rigidbody;
            return rigidbody;
        }
    }

    // Add the object's weight to pressure plate if object connects.
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        Rigidbody rigidbody = GetRigidbody(other);
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Throwable")) && rigidbody != null)
        {
            weight += rigidbody.mass;
        }
    }

    // Remove the object's weight from pressure plate if object no longer connects.
    private void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;
        Rigidbody rigidbody = GetRigidbody(other);
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Throwable")) && rigidbody != null)
        {
            weight -= rigidbody.mass;
        }
    }
    
    // Update the pressure plate's activation value depending on whether an update tick has been reached or not.
    private void UpdateActivation()
    {
        updateTime -= Time.deltaTime; // Tick down the update timer
        if (updateTime < 0)
        {
            updateTime = updateInterval; // Update activation when timer expires and is renewed
            float priorActivation = activation;
            if (weight < weightThreshold) // Activation floors at 0 if weight is below weightThreshold
            {
                activation = 0f;
            }
            else if (weight < weightCapacity) // Activation increases linearly if between weightThreshold and weightCapacity
            {
                activation = (weight - weightThreshold) / (weightCapacity - weightThreshold);
            }
            else // Activation caps at 1 if weight is above weightCapacity
            {
                activation = 1f;
            }
            if (activation != priorActivation) // Request a puzzle manager update if activation changes during this tick
            {
                puzzleManager.RequestUpdate();
            }
        }
        // Update interpolated activation towards current activation
        float activationDiff = interpolatedActivation - activation;
        if (activationDiff < -0.1)
        {
            interpolatedActivation += 0.1f;
        }
        else if (activationDiff <= 0.1)
        {
            interpolatedActivation = activation;
        }
        else
        {
            interpolatedActivation -= 0.1f;
        }
    }

    // Main update loop
    void Update()
    {
        UpdateActivation();
        // Squish pressure plate and its collider according to current interpolated activation
        Vector3 scale = gameObject.transform.localScale;
        scale.y = 1 - interpolatedActivation * 0.9f;
        gameObject.transform.localScale = scale;
        Vector3 colliderScale = boxCollider.size;
        colliderScale.y = 0.005f * scale.y;
        boxCollider.size = colliderScale;
        // Update progress bar appropriately if it exists
        if (_progressBar != null)
        {
            float height = 0.05f + 0.5f * activation;
            Transform transform = _progressBar.transform;
            transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
            float offY = 0.25f * (activation - 1f);
            transform.localPosition = new Vector3(transform.localPosition.x, offY, transform.localPosition.z);
        }
    }

    // Gets current activation value
    public float GetActivation()
    {
        return activation;
    }
}
