using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverState : MonoBehaviour
{

    public float initialActivation;
    private float activation;
    private Transform rootTransform;

    void Start()
    {
        initialActivation = Mathf.Clamp(initialActivation, 0f, 1f);
        activation = initialActivation;
        rootTransform = gameObject.transform.root.root.root;
    }

    private float GetAngle()
    {
        return (activation - 0.5f) * 4f * Mathf.PI / 9f;
    }

    public Vector3 GetHandlePos()
    {
        Vector3 zBasis = rootTransform.rotation * new Vector3(0, 0, 1);
        Vector3 yBasis = rootTransform.rotation * new Vector3(0, 1, 0);
        float angle = GetAngle();
        return rootTransform.position + 0.45f * Mathf.Sin(angle) * yBasis + 0.45f * Mathf.Cos(angle) * zBasis;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
