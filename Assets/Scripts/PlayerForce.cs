using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerForce : MonoBehaviour {

    public float force_factor = 30.0f;
    public float throw_force_factor = 1.0f;
    public float smoothing_threshold = 1.3f;
    public float smoothing_factor = 1.0f;
    public float smoothing_strength = 0.01f;
    public float separation_threshold = 1.3f;
    public float proximity_threshold = 0.01f;
    public float throw_threshold = 0.1f;
    public float decay_factor = 0.95f;

    public bool ApplyForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (displacement.magnitude > separation_threshold)
        {
            return false;
        }
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
            Vector3 displacement_n = Vector3.Normalize(displacement);
            float scaling_factor = Mathf.Min(displacement.magnitude, 1.0f);
            rigidbody.velocity.Scale(decay_factor * scaling_factor * new Vector3(1, 1, 1));
            rigidbody.AddForce(force_factor * scaling_factor * displacement_n / smoothing_factor, ForceMode.Force);
        }
        return true;
    }

    public void ApplyThrowForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (displacement.magnitude > throw_threshold)
        {
            rigidbody.AddForce(throw_force_factor * displacement, ForceMode.Impulse);
        }
    }

}
