using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleplayerManager : MonoBehaviour
{
    [SerializeField] Timer timer;
    [SerializeField] PuzzleGenerator puzzleGenerator;
    [SerializeField] TilesManager tilesManager;
    void Start()
    {
        StartCoroutine(StartGame());
    }
    IEnumerator StartGame()
    {
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(0));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());
        puzzleGenerator.GenerateTiles();
        tilesManager.ShuffleAllTiles();
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
