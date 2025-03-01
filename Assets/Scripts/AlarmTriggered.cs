using System;
using UnityEngine;

public class AlarmTriggered : MonoBehaviour
{
    // Declare an event to notify listeners when the alarm is triggered
    public static event Action OnAlarmTriggered;

    // Reference to the AudioSource component
    public AudioSource alarmAudioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finger"))
        {
            OnAlarmTriggered?.Invoke();
            Debug.Log("Alarm triggered!");

            // Play the alarm sound
            if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
            {
                alarmAudioSource.Play();
            }
        }
    }
}