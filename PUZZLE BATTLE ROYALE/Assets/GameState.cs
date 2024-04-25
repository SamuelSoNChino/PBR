using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public bool gameActive;
    void Start()
    {
        gameActive = true;
    }

    public void EndGame()
    {
        int finalTime = GameObject.Find("Timer").GetComponent<Timer>().seconds;
        PlayerPrefs.SetInt("LastTime", finalTime);
        if (finalTime < PlayerPrefs.GetInt("BestTime") || PlayerPrefs.GetInt("BestTime") == 0)
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
