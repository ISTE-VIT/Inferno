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
    public AudioSource sprayAudio; // Audio source for spray sound
    public InputActionReference triggerAction;
    public float maxSprayDuration = 10f;

    [Header("Pin Mechanics")]
    public bool isPinPulled = false;
    private Vector3 initialPinPosition;
    public float pinPullThreshold = 0.1f;

    private float sprayTimer = 0f;
    private bool isSpraying = false;
    private bool isHoldingExtinguisher = false;

    private void Start()
    {
        if (fireSuppressant != null)
        {
            fireSuppressant.Stop();
        }
        else
        {
            Debug.LogError("Particle system (fireSuppressant) is not assigned!");
        }

        if (sprayAudio != null)
        {
            sprayAudio.Stop();
        }
        else
        {
            Debug.LogError("Spray audio source is not assigned!");
        }

        if (triggerAction != null)
        {
            triggerAction.action.Enable();
        }
        else
        {
            Debug.LogError("Trigger Action is not assigned!");
        }

        if (pinObject != null)
        {
            pinRigidbody = pinObject.GetComponent<Rigidbody>();
            if (pinRigidbody != null)
            {
                pinRigidbody.isKinematic = true;
            }
            initialPinPosition = pinObject.transform.position;
            pinObject.selectEntered.AddListener(OnPinGrabbed);
            pinObject.selectExited.AddListener(OnPinReleased);
        }

        if (extinguisher != null)
        {
            extinguisher.selectEntered.AddListener(OnExtinguisherGrabbed);
            extinguisher.selectExited.AddListener(OnExtinguisherReleased);
        }
    }

    private void Update()
    {
        HandleSpray();
        CheckPinPull();
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
                    if (fireSuppressant != null && !fireSuppressant.isPlaying)
                    {
                        fireSuppressant.Play();
                    }
                    if (sprayAudio != null && !sprayAudio.isPlaying)
                    {
                        sprayAudio.Play();
                    }
                }
                sprayTimer += Time.deltaTime;
            }
            else
            {
                StopSpray();
            }
        }
        else
        {
            StopSpray();
        }
    }

    private void StopSpray()
    {
        if (fireSuppressant != null && fireSuppressant.isPlaying)
        {
            fireSuppressant.Stop();
        }
        if (sprayAudio != null && sprayAudio.isPlaying)
        {
            sprayAudio.Stop();
        }
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
            }
        }
    }

    private void OnExtinguisherGrabbed(SelectEnterEventArgs args)
    {
        isHoldingExtinguisher = true;
    }

    private void OnExtinguisherReleased(SelectExitEventArgs args)
    {
        isHoldingExtinguisher = false;
        StopSpray();
    }

    private void OnPinGrabbed(SelectEnterEventArgs args)
    {
        if (pinRigidbody != null)
        {
            pinRigidbody.isKinematic = false;
        }
        pinObject.transform.SetParent(null);
    }

    private void OnPinReleased(SelectExitEventArgs args)
    {
        float pullDistance = Vector3.Distance(pinObject.transform.position, initialPinPosition);

        if (pullDistance >= pinPullThreshold)
        {
            Destroy(pinObject.gameObject);
            isPinPulled = true;
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