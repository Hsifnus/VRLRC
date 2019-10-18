using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerForce : MonoBehaviour {

    public float force_factor = 20.0f;
    public float attach_threshold = 0.0f;
    public float smoothing_threshold = 2.0f;
    public float smoothing_factor = 1.0f;
    public float smoothing_strength = 0.01f;
    public float separation_threshold = 3.0f;
    public float throw_threshold = 0.1f;

    public void ApplyForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (displacement.magnitude > attach_threshold)
        {
            Vector3 displacement_n = Vector3.Normalize(displacement);
            if (displacement.magnitude > smoothing_threshold)
            {
                smoothing_factor = 1.0f;
            }
            else
            {
                smoothing_factor = Mathf.Min(smoothing_factor + smoothing_strength, 2.0f);
            }
            rigidbody.AddForce(force_factor * displacement_n / smoothing_factor);
            obj.transform.Translate(0.01f * displacement);
        } else
        {
            rigidbody.velocity = 10 * displacement;
        }
    }

    public void ApplyThrowForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        if (displacement.magnitude > throw_threshold)
        {
            rigidbody.AddForce(force_factor * displacement);
        }
    }

}
