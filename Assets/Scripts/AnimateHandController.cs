using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandController : MonoBehaviour
{
    public InputActionReference gripInputActionReference;
    public InputActionReference triggerInputActionReference;

    [SerializeField] Animator _handAnimator;
    private float _gripValue;
    private float _triggerValue;


    void Update()
    {
        if (_handAnimator == null)
        {
            _handAnimator = transform.GetChild(0).GetComponent<Animator>();
        }

        AnimateGrip();
        AnimateTrigger();
    }

    void AnimateGrip()
    {
        _gripValue = gripInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat("Grip", _gripValue);
    }

    void AnimateTrigger()
    {
        _triggerValue = triggerInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat("Trigger", _triggerValue);
    }
}
