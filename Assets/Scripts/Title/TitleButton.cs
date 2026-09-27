using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleButton : MonoBehaviour
{
    public int button_type = 0;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (button_type == 0)
        {
            StartGame();
        }
        else if (button_type == 1)
        {
            QuitGame();
        }
    }

    void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}