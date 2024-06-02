using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startScreenText;
    [SerializeField] private float cyclingInterval = 0.5f;
    [SerializeField] private string matchmakingText = "Looking for an opponent";
    [SerializeField] private string countdownText = "Match starting in";
    [SerializeField] private int countdownStart = 3;
    [SerializeField] private float countdownInterval = 1f;
    [SerializeField] private float goInterval = 0.5f;

    private int dotCount = 0;
    private bool stopCycling = false;

    void Start()
    {
        StartCoroutine(MatchmakingCycle());
    }

    private IEnumerator MatchmakingCycle()
    {
        while (!stopCycling)
        {
            dotCount += 1;
            startScreenText.text = matchmakingText + new string('.', dotCount % 4);
            yield return new WaitForSeconds(cyclingInterval);
        }
    }

    public void StopMatchmakingCycle()
    {
        stopCycling = true;
    }

    public IEnumerator StartCountdown()
    {
        for (int i = countdownStart; i >= 0; i--)
        {
            startScreenText.text = $"{countdownText}: {i}";
            yield return new WaitForSeconds(countdownInterval);
        }

        startScreenText.text = "GO!";
        yield return new WaitForSeconds(goInterval);
        gameObject.SetActive(false);
    }
}
