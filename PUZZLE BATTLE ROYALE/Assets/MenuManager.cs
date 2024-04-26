using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("Tiles"))
        {
            PlayerPrefs.SetInt("Tiles", 5);
        }
        if (!PlayerPrefs.HasKey("BestTime"))
        {
            PlayerPrefs.SetInt("BestTime", -1);
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Puzzle");
    }
    public void GoToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}
