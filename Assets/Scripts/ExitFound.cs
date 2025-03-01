using System;
using UnityEngine;

public class ExitFound : MonoBehaviour
{
    // Declare an event to notify listeners when the exit is reached
    public static event Action OnExitReached;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnExitReached?.Invoke();
            Debug.Log("Exit reached!");
        }
    }
}