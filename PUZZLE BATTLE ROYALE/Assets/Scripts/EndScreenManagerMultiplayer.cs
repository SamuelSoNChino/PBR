using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManagerMultiplayer : MonoBehaviour
{
    [SerializeField] private GameObject winningScreen;
    [SerializeField] private GameObject losingScreen;

    public void LoadWinningScreen()
    {
        winningScreen.SetActive(true);
    }

    public void LoadLosingScreen()
    {
        losingScreen.SetActive(true);
    }

    public void BackToMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu");
    }

    public void Rematch()
    {
        // TODO
    }
}
