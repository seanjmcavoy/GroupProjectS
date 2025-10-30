using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Names")]
    public string levelSelectScene = "LevelSelect";
    public string helpScene = "Help";

    public void OnStartClicked()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    public void OnHelpClicked()
    {
        SceneManager.LoadScene(helpScene);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
