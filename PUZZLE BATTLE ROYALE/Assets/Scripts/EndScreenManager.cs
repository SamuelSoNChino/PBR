using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("End Time").GetComponent<Text>().text += PlayerPrefs.GetInt("LastTime");
        GameObject.Find("Best Time").GetComponent<Text>().text += PlayerPrefs.GetInt("BestTime");
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void PlayAgain()
    {
        SceneManager.LoadScene("PuzzleSingleplayer");
    }

}
