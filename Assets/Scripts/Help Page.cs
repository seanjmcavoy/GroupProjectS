using UnityEngine;

public class HelpUIManager : MonoBehaviour
{
    public GameObject page1;
    public GameObject page2;

    public void ShowPage1()
    {
        page1.SetActive(true);
        page2.SetActive(false);
    }

    public void ShowPage2()
    {
        page1.SetActive(false);
        page2.SetActive(true);
    }
}
