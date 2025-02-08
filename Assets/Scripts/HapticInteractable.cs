using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class Haptic
{
    [Range(0, 1)]
    public float intensity = 0.5f;
    public float duration = 0.1f;

    public void TriggerHaptic(BaseInteractionEventArgs eventArgs)
    {
        if (eventArgs.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            TriggerHaptic(controllerInteractor.xrController);
        }
    }

    public void TriggerHaptic(XRBaseController controller)
    {
        if (controller != null && intensity > 0)
        {
            controller.SendHapticImpulse(intensity, duration);
        }
    }
}

public class HapticInteractable : MonoBehaviour
{
    public Haptic hapticOnActivated;
    public Haptic hapticHoverEntered;
    public Haptic hapticHoverExited;
    public Haptic hapticSelectEntered;
    public Haptic hapticSelectExited;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        // Subscribing to events correctly in Unity 6
        interactable.activated.AddListener((args) => hapticOnActivated.TriggerHaptic(args));
        interactable.hoverEntered.AddListener((args) => hapticHoverEntered.TriggerHaptic(args));
        interactable.hoverExited.AddListener((args) => hapticHoverExited.TriggerHaptic(args));
        interactable.selectEntered.AddListener((args) => hapticSelectEntered.TriggerHaptic(args));
        interactable.selectExited.AddListener((args) => hapticSelectExited.TriggerHaptic(args));
    }

    void OnDestroy()
    {
        // Properly removing event listeners to prevent memory leaks
        interactable.activated.RemoveListener((args) => hapticOnActivated.TriggerHaptic(args));
        interactable.hoverEntered.RemoveListener((args) => hapticHoverEntered.TriggerHaptic(args));
        interactable.hoverExited.RemoveListener((args) => hapticHoverExited.TriggerHaptic(args));
        interactable.selectEntered.RemoveListener((args) => hapticSelectEntered.TriggerHaptic(args));
        interactable.selectExited.RemoveListener((args) => hapticSelectExited.TriggerHaptic(args));
    }
}
