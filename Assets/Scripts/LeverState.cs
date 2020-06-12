using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverState : MonoBehaviour
{
    // The degree to which this lever is activated, and the initial value said degree begins at
    private float activation;
    public float initialActivation;
    // The Transform of the lever root object, since this script is assigned to the handle, not the root
    private Transform rootTransform;
    // The unit y vector (0, 1, 0) and z vector (0, 0, 1) of the lever's local coordinate space, converted into world direction vectors
    private Vector3 yBasis, zBasis;
    // The strongest pull strength allowed on the handle
    public float max_pull_strength = 10f;

    // Obtain root lever object transform, initialize activation value, and create basis vectors
    void Start()
    {
        initialActivation = Mathf.Clamp(initialActivation, 0f, 1f);
        activation = initialActivation;
        rootTransform = gameObject.transform.root.root.root;
        zBasis = rootTransform.rotation * new Vector3(0, 0, 1);
        yBasis = rootTransform.rotation * new Vector3(0, 1, 0);
    }

    // Returns the angle (in radians) of the handle
    private float GetAngle()
    {
        return (activation - 0.5f) * 8f * Mathf.PI / 9f;
    }

    // Returns the world position of the lever handle
    public Vector3 GetHandlePos()
    {
        float angle = GetAngle();
        return rootTransform.position + 0.7f * Mathf.Cos(angle) * yBasis + 0.7f * Mathf.Sin(angle) * zBasis;
    }

    // Updates activation value by computing the dot product between pull direction and lever handle's forward direction
    public void UpdateActivation(Vector3 pull)
    {
        float angle = GetAngle();
        Vector3 pullDir = new Vector3(0f, Vector3.Dot(yBasis, pull), Vector3.Dot(zBasis, pull));
        Vector3 handlePositiveDir = new Vector3(0f, Mathf.Sin(angle + Mathf.PI), Mathf.Cos(angle));
        float netPullStrength = Mathf.Clamp(Vector3.Dot(pullDir, handlePositiveDir), -max_pull_strength, max_pull_strength) / 100;
        activation = Mathf.Clamp(activation + netPullStrength, 0f, 1f);
    }

    // Update handle position and rotation according to activation
    void Update()
    {
        float angle = GetAngle();
        gameObject.transform.rotation = rootTransform.rotation * Quaternion.Euler(angle * 180 / Mathf.PI, 0, 0);
        Vector3 pos = gameObject.transform.position;
        Vector3 root = rootTransform.position;
        gameObject.transform.position = new Vector3(pos.x, root.y + 0.01f * Mathf.Cos(angle), root.z + 0.01f * Mathf.Sin(angle));
    }
}
