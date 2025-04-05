using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;

public class PerformanceTracker : MonoBehaviour
{
    public string backendURL = "";
    // public string sceneType = "office";
    public string difficulty = "Easy";

    public float startTime;
    private float extinguisherFoundTime = -1f;
    private float fireExtinguishedTime = -1f;
    private float alarmTriggeredTime = -1f;
    private float exitTime = -1f;

    private bool extinguisherFound = false;
    private bool fireExtinguished = false;
    private bool alarmTriggered = false;
    private bool exitReached = false;

    public FireGrowth fireGrowth;

    void Start()
    {
        startTime = Time.time;

        if (fireGrowth == null)
        {
            fireGrowth = FindObjectOfType<FireGrowth>();
        }

        if (fireGrowth != null)
        {
            fireGrowth.OnFireDestroyed += ExtinguishFire;
        }

        // Subscribe to the static events using the class names
        ExitFound.OnExitReached += ReachExit;
        AlarmTriggered.OnAlarmTriggered += TriggerAlarm;
    }

    private void OnDestroy()
    {
        if (fireGrowth != null)
        {
            fireGrowth.OnFireDestroyed -= ExtinguishFire;
        }

        // Unsubscribe from the static events using the class names
        ExitFound.OnExitReached -= ReachExit;
        AlarmTriggered.OnAlarmTriggered -= TriggerAlarm;
    }

    private void ExtinguishFire()
    {
        float elapsedTime = Time.time - startTime;
        OnFireExtinguished(elapsedTime);
    }

    private void ReachExit()
    {
        float elapsedTime = Time.time - startTime;
        OnExitReached(elapsedTime);
        Debug.Log("Exit reached!");
        SendDataToBackend();

        StartCoroutine(ReturnToMainMenuAfterDelay(2f));
    }

    private IEnumerator ReturnToMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainMenu");
    }

    private void TriggerAlarm()
    {
        float elapsedTime = Time.time - startTime;
        OnAlarmTriggered(elapsedTime);
        Debug.Log("Alarm triggered!");
    }

    public void OnExtinguisherFound(float time)
    {
        if (!extinguisherFound)
        {
            extinguisherFoundTime = time;
            extinguisherFound = true;
        }
    }

    public void OnFireExtinguished(float time)
    {
        if (!fireExtinguished)
        {
            fireExtinguishedTime = time;
            fireExtinguished = true;
            Debug.Log("Fire extinguished!");
        }
    }

    public void OnAlarmTriggered(float time)
    {
        if (!alarmTriggered)
        {
            alarmTriggeredTime = time;
            alarmTriggered = true;
            Debug.Log("Alarm triggered!");
        }
    }

    public void OnExitReached(float time)
    {
        if (!exitReached)
        {
            exitTime = time;
            exitReached = true;
        }
    }

    public void SendDataToBackend()
    {
        // Retrieve user email and age from PlayerPrefs
        string userEmail = PlayerPrefs.GetString("UserEmail", "unknown@example.com");
        string userAge = PlayerPrefs.GetString("UserAge", "69");
        string sceneType = PlayerPrefs.GetString("SceneType", "Default-scene");
        // float weightedScore = CalculatePerformanceScore(userAge);

        PerformanceData data = new PerformanceData
        {
            email = userEmail,
            age = userAge,
            sceneType = sceneType,
            difficulty = difficulty,
            timeToFindExtinguisher = extinguisherFoundTime >= 0 ? extinguisherFoundTime : 0,
            timeToExtinguishFire = fireExtinguishedTime >= 0 ? fireExtinguishedTime : 0,
            timeToTriggerAlarm = alarmTriggeredTime >= 0 ? alarmTriggeredTime : 0,
            timeToFindExit = exitTime >= 0 ? exitTime : 0,
            // performanceScore = weightedScore
        };

        string jsonData = JsonUtility.ToJson(data);
        StartCoroutine(PostRequest(backendURL, jsonData));
    }

    /*
    private float CalculatePerformanceScore(int age)
    {
        float score = 100;

        float alarmPenalty = alarmTriggeredTime >= 0 ? alarmTriggeredTime : 0;
        float extinguisherPenalty = extinguisherFoundTime >= 0 ? extinguisherFoundTime : 0;
        float firePenalty = fireExtinguishedTime >= 0 ? fireExtinguishedTime : 0;
        float exitPenalty = exitTime >= 0 ? exitTime : 0;

        if (age < 10)
        {
            score -= (alarmPenalty * 2) + (exitPenalty * 1.5f);
        }
        else if (age >= 10 && age < 50)
        {
            score -= (extinguisherPenalty * 1.2f) + (firePenalty * 1.5f) + (exitPenalty * 1.2f);
        }
        else
        {
            score -= (alarmPenalty * 1.5f) + (firePenalty * 1.8f) + (exitPenalty * 1.5f);
        }

        return Mathf.Max(score, 0);
    }
    */

    IEnumerator PostRequest(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data sent successfully.");
            }
            else
            {
                Debug.LogError("Error sending data: " + request.error);
            }
        }
    }

    [Serializable]
    public class PerformanceData
    {
        public string email;
        public string age;
        public string sceneType;
        public string difficulty;
        public float timeToFindExtinguisher;
        public float timeToExtinguishFire;
        public float timeToTriggerAlarm;
        public float timeToFindExit;
        // public float performanceScore;
    }
}
