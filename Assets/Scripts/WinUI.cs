using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;            // TextMeshPro
using System.IO;       // Path.GetFileNameWithoutExtension

[AddComponentMenu("PathQueue/UI/Win UI")]
[DisallowMultipleComponent]
public class WinUI : MonoBehaviour
{
    public static WinUI Instance { get; private set; }

    [Header("Assign in Inspector")]
    [SerializeField] GameObject panel;       // Set inactive by default
    [SerializeField] TMP_Text messageText;   // Optional might not have to use 
    [SerializeField] TMP_Text levelTitleText; //Level name text

    public bool IsOpen => panel && panel.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel) panel.SetActive(false);
    }

    public void Show(string msg = "You Won!")
    {
        if (levelTitleText)
        {
        string levelName = SceneManager.GetActiveScene().name;
        levelTitleText.text = levelName;
        }
        if (messageText) messageText.text = msg;
        if (panel) panel.SetActive(true);
        Time.timeScale = 0f; // pause gameplay
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToLevelSelect()
    {
        QuitToMenu("LevelSelect"); 
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int current = SceneManager.GetActiveScene().buildIndex;

        // Find the next scene whose name starts with "Level_"
        for (int i = current + 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.StartsWith("Level_"))
            {
                SceneManager.LoadScene(i);
                return;
            }
        }

        // No higher level found -> go back to Level_00
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == "Level_00")
            {
                SceneManager.LoadScene(i);
                return;
            }
        }

        // Fallback: reload current if Level_0_ wasn’t found
        SceneManager.LoadScene(current);
    }

    public void QuitToMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
