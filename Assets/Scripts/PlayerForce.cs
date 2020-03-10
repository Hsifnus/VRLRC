using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerForce : MonoBehaviour {

    // Directly scales the amplitude of forces exerted when player pulls on an object
    public float force_factor = 30.0f;
    // Directly scales the amplitude of force exerted when player throws an object
    public float throw_force_factor = 1.0f;
    // Maximum distance for force smoothing to occur
    public float smoothing_threshold = 1.3f;
    // Factor by which force is divided as a result of force smoothing
    public float smoothing_factor = 1.0f;
    // How quickly should the smoothing factor change per update
    public float smoothing_strength = 0.01f;
    // Maximum distance for an object to remain tethered to player's hand
    public float separation_threshold = 1.3f;
    // Minimum distance value for pull forces to be applied, preventing degenerate low-distance applications of force
    public float proximity_threshold = 0.01f;
    // Minimum distance for an object throw to count as a proper throw
    public float throw_threshold = 0.1f;
    // Directly scales how much of the object's prior velocity is maintained per update
    public float decay_factor = 0.95f;

    // Applies a pull force onto throwable object obj towards the player's hand
    // Returns true if the object should remain linked to the player's hand
    public bool ApplyForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        // Should we separate the object from the player due to separation?
        if (displacement.magnitude > separation_threshold)
        {
            return false;
        }
        // Check to see if object is far enough from the hand to not oscillate wildly around the hand
        if (displacement.magnitude > proximity_threshold)
        {
            if (displacement.magnitude > smoothing_threshold)
            {
                smoothing_factor = 1.0f;
            }
            else
            {
                smoothing_factor = Mathf.Max(smoothing_factor - smoothing_strength, 0.9f);
            }
            // Unit displacement between player and object
            Vector3 displacement_n = Vector3.Normalize(displacement);
            // Factor that scales both pull velocity and force
            float scaling_factor = Mathf.Min(displacement.magnitude, 1.0f);
            rigidbody.velocity.Scale(decay_factor * scaling_factor * new Vector3(1, 1, 1));
            rigidbody.AddForce(force_factor * scaling_factor * displacement_n / smoothing_factor, ForceMode.Force);
        }
        return true;
    }

    // Applies a sudden force onto an object when player releases hold of said object
    public void ApplyThrowForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        // Is the object far enough away from the hand for the release to count as a throw?
        if (displacement.magnitude > throw_threshold)
        {
            rigidbody.AddForce(throw_force_factor * displacement, ForceMode.Impulse);
        }
    }

}
