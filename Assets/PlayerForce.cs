using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerForce : MonoBehaviour {

    public float force_factor = 30.0f;
    public float smoothing_threshold = 3.0f;
    public float smoothing_factor = 1.0f;
    public float smoothing_strength = 0.01f;
    public float separation_threshold = 3.0f;

    public void ApplyForce(GameObject obj)
    {
        Vector3 displacement = this.gameObject.transform.position - obj.transform.position;
        Vector3 displacement_n = Vector3.Normalize(displacement);
        if (displacement.magnitude > smoothing_threshold)
        {
            smoothing_factor = 1.0f;
        }
        else
        {
            smoothing_factor = Mathf.Max(smoothing_strength, 2.0f);
        }
        obj.GetComponent<Rigidbody>().AddForce(force_factor * displacement_n / smoothing_factor);
        obj.transform.Translate(0.1f * displacement);
    }

}
