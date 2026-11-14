using UnityEngine;

[DefaultExecutionOrder(100)]
public class ForestParallax2D : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0, 1)] public float cameraTravel = .5f;
    Vector3 initialPosition, initialCamera;

    void Start()
    {
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        initialPosition = transform.position;
        if (cameraTransform) initialCamera = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;
        Vector3 travel = cameraTransform.position - initialCamera;
        transform.position = initialPosition + new Vector3(travel.x, travel.y * .2f, 0) * cameraTravel;
    }
}
