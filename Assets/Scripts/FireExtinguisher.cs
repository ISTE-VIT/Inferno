using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class FireExtinguisher : MonoBehaviour
{
    [Header("XR Components")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable extinguisher;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable pinObject;
    private Rigidbody pinRigidbody;

    [Header("Fire Suppression System")]
    public ParticleSystem fireSuppressant;
    public InputActionReference triggerAction;
    public float maxSprayDuration = 10f;
    
    [Header("Fire Detection")]
    public LayerMask fireLayer; // Layer for fire objects
    public float sprayRadius = 2f; // Detection radius for fire extinction
    private bool hasExtinguishedFire = false;
    
    [Header("Audio")]
    public AudioSource sprayAudio;

    [Header("Pin Mechanics")]
    public bool isPinPulled = false;
    private Vector3 initialPinPosition;
    public float pinPullThreshold = 0.1f;

    private float sprayTimer = 0f;
    private bool isSpraying = false;
    private bool isHoldingExtinguisher = false;
    private PerformanceTracker performanceTracker;

    private void Start()
    {
        performanceTracker = FindObjectOfType<PerformanceTracker>();
        if (performanceTracker == null) Debug.LogError("PerformanceTracker not found in scene!");

        if (fireSuppressant != null) fireSuppressant.Stop();
        else Debug.LogError("Particle system (fireSuppressant) is not assigned!");

        if (sprayAudio == null) Debug.LogError("AudioSource (sprayAudio) is not assigned!");

        if (triggerAction != null) triggerAction.action.Enable();
        else Debug.LogError("Trigger Action is not assigned!");

        if (pinObject != null)
        {
            pinRigidbody = pinObject.GetComponent<Rigidbody>();
            if (pinRigidbody != null) pinRigidbody.isKinematic = true;
            else Debug.LogError("Pin object is missing a Rigidbody component!");
            initialPinPosition = pinObject.transform.position;
            pinObject.selectEntered.AddListener(OnPinGrabbed);
            pinObject.selectExited.AddListener(OnPinReleased);
        }

        if (extinguisher != null)
        {
            extinguisher.selectEntered.AddListener(OnExtinguisherGrabbed);
            extinguisher.selectExited.AddListener(OnExtinguisherReleased);
        }
        else Debug.LogError("Extinguisher XRGrabInteractable is not assigned!");
    }

    private void Update()
    {
        HandleSpray();
        CheckPinPull();
        CheckFireExtinguished();
    }

    private void CheckFireExtinguished()
    {
        if (isSpraying && !hasExtinguishedFire)
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                fireSuppressant.transform.position,
                sprayRadius,
                fireLayer
            );
        }
    }

    private void HandleSpray()
    {
        if (isHoldingExtinguisher && isPinPulled)
        {
            float triggerValue = triggerAction.action.ReadValue<float>();

            if (triggerValue > 0.5f && sprayTimer < maxSprayDuration)
            {
                if (!isSpraying)
                {
                    isSpraying = true;
                    Debug.Log("Spraying!");
                    
                    if (fireSuppressant != null && !fireSuppressant.isPlaying)
                        fireSuppressant.Play();

                    if (sprayAudio != null && !sprayAudio.isPlaying)
                        sprayAudio.Play();
                }

                sprayTimer += Time.deltaTime;
            }
            else
            {
                StopSpraying();
            }
        }
        else
        {
            StopSpraying();
        }
    }

    private void StopSpraying()
    {
        if (fireSuppressant != null) fireSuppressant.Stop();
        if (sprayAudio != null && sprayAudio.isPlaying) sprayAudio.Stop();
        isSpraying = false;
    }

    private void CheckPinPull()
    {
        if (pinObject != null && pinObject.isSelected)
        {
            float pullDistance = Vector3.Distance(pinObject.transform.position, initialPinPosition);
            if (pullDistance >= pinPullThreshold)
            {
                Destroy(pinObject.gameObject);
                isPinPulled = true;
                Debug.Log("Pin Pulled!");
            }
        }
    }

    private void OnExtinguisherGrabbed(SelectEnterEventArgs args)
    {
        isHoldingExtinguisher = true;
        performanceTracker.OnExtinguisherFound(Time.time - performanceTracker.startTime);
        Debug.Log("Extinguisher Grabbed!");
    }

    private void OnExtinguisherReleased(SelectExitEventArgs args)
    {
        isHoldingExtinguisher = false;
        Debug.Log("Extinguisher Released!");
    }

    private void OnPinGrabbed(SelectEnterEventArgs args)
    {
        if (pinRigidbody != null) pinRigidbody.isKinematic = false;
        pinObject.transform.SetParent(null);
    }

    private void OnPinReleased(SelectExitEventArgs args)
    {
        float pullDistance = Vector3.Distance(pinObject.transform.position, initialPinPosition);

        if (pullDistance >= pinPullThreshold)
        {
            Destroy(pinObject.gameObject);
            isPinPulled = true;
            Debug.Log("Pin Successfully Pulled!");
        }
        else
        {
            if (pinRigidbody != null)
            {
                pinRigidbody.useGravity = true;
                pinRigidbody.isKinematic = false;
            }
        }
    }
}
