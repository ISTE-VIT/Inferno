using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ExtinguisherRopeExtension : MonoBehaviour
{
    [Header("Hose Setup")]
    public Transform valvePoint; // Hose attachment point on extinguisher
    public Transform nozzlePoint; // Nozzle's transform
    public LineRenderer lineRenderer; // Hose visual representation
    public float maxHoseLength = 2.0f; // Maximum hose length
    public Rigidbody nozzleRigidbody; // Rigidbody for physics-based nozzle movement

    [Header("XR Interaction")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable extinguisherGrab; // XR grab for extinguisher

    private void OnEnable()
    {
        if (extinguisherGrab != null)
            extinguisherGrab.selectEntered.AddListener(OnExtinguisherGrabbed);
    }

    private void OnDisable()
    {
        if (extinguisherGrab != null)
            extinguisherGrab.selectEntered.RemoveListener(OnExtinguisherGrabbed);
    }

    void Update()
    {
        if (valvePoint == null || nozzlePoint == null || lineRenderer == null || nozzleRigidbody == null)
            return;

        // Restrict hose length
        float currentLength = Vector3.Distance(valvePoint.position, nozzlePoint.position);
        if (currentLength > maxHoseLength)
        {
            Vector3 direction = (nozzlePoint.position - valvePoint.position).normalized;
            Vector3 restrictedPosition = valvePoint.position + direction * maxHoseLength;
            nozzleRigidbody.MovePosition(restrictedPosition);
        }

        // Update the hose's visual representation
        lineRenderer.SetPosition(0, valvePoint.position);
        lineRenderer.SetPosition(1, nozzlePoint.position);
    }

    private void OnExtinguisherGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Extinguisher grabbed by: " + args.interactorObject.transform.name);
    }
}
