using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwitchSceneButton : MonoBehaviour
{
    public int scene;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartLevel);
    }

    void StartLevel()
    {
        SceneManager.LoadScene(scene);
    }
}