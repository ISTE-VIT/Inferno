using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadScenes : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadElectricalScene()
    {
        SceneManager.LoadScene("ElectricalScene");
    }
    public void LoadMaterialScene()
    {
        SceneManager.LoadScene("MaterialScene");
    }
    public void LoadLabScene()
    {
        SceneManager.LoadScene("LabScene");
    }
}
