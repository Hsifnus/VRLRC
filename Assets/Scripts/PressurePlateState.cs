using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateState : MonoBehaviour
{
    public float weightCapacity = 1f;
    public float weightThreshold = 0f;
    public float updateInterval = 1f;
    private float activation;
    private float interpolatedActivation;
    private float weight;
    private float updateTime;
    private Hashtable rigidbodyCache;
    private BoxCollider boxCollider;

    void Start()
    {
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

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidbody = GetRigidbody(other);
        if (rigidbody != null)
        {
            weight += rigidbody.mass;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rigidbody = GetRigidbody(other);
        if (rigidbody != null)
        {
            weight -= rigidbody.mass;
        }
    }

    private void UpdateActivation()
    {
        updateTime -= Time.deltaTime; // Tick down the update timer
        if (updateTime < 0)
        {
            updateTime = updateInterval; // Update activation when timer expires and is renewed
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
    }
}
