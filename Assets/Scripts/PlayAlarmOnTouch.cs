using UnityEngine;

public class PlayAlarm : MonoBehaviour
{
    public AudioSource alarmSound; 

    private void Start()
    {
        if (alarmSound == null)
        {
            Debug.LogError("Alarm sound is not assigned.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called.");

        if (other.CompareTag("Finger"))
        {
            Debug.Log("Finger entered the trigger.");

            if (alarmSound == null)
            {
                Debug.LogError("Alarm sound is not assigned.");
                return;
            }

            if (alarmSound.isPlaying)
            {
                Debug.Log("Alarm is playing. Stopping the alarm.");
                alarmSound.Stop();
            }
            else
            {
                Debug.Log("Alarm is not playing. Playing the alarm.");
                alarmSound.Play();
            }
        }
        else
        {
            Debug.Log("Other object entered the trigger: " + other.tag);
        }
    }
}