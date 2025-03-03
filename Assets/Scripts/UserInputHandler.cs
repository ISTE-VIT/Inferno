using UnityEngine;
using TMPro;

public class UserDataManager : MonoBehaviour
{
    public TMP_InputField emailInputField; // TMP InputField for email

    // Call this method when user presses "Continue" after entering email
    public void SaveEmail()
    {
        string email = emailInputField.text;

        if (!string.IsNullOrEmpty(email))
        {
            PlayerPrefs.SetString("UserEmail", email);
            PlayerPrefs.Save();
            Debug.Log("Email saved: " + email);
        }
        else
        {
            Debug.LogWarning("Email field is empty!");
        }
    }

    // Call this method when an age button is clicked (pass age range as parameter)
    public void SaveAge(string ageRange)
    {
        PlayerPrefs.SetString("UserAge", ageRange);
        PlayerPrefs.Save();
        Debug.Log("Age group saved: " + ageRange);
    }

    // Call this method when a scene button is clicked (pass scene type as parameter)
    public void SaveSceneType(string sceneType)
    {
        PlayerPrefs.SetString("SceneType", sceneType);
        PlayerPrefs.Save();
        Debug.Log("Scene type saved: " + sceneType);
    }
}
