using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleplayerManager : MonoBehaviour
{
    private Timer timer;
    void Start()
    {
        timer = GameObject.Find("Timer").GetComponent<Timer>();
        GameObject.Find("Puzzle").GetComponent<PuzzleGenerator>().StartGenerating();
        timer.EnableTimer();
    }
    public void EndGame()
    {
        timer.DisableTimer();
        int finalTime = timer.GetCurrentTime();
        PlayerPrefs.SetInt("LastTime", finalTime);
        if (finalTime < PlayerPrefs.GetInt("BestTime") || PlayerPrefs.GetInt("BestTime") < 0)
        {
            PlayerPrefs.SetInt("BestTime", finalTime);
        }
        SceneManager.LoadScene("EndScreen");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
