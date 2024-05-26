using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float time = 0;
    private bool timerEnabled;
    private Text timerText;

    // Sets default values
    void Start()
    {
        timerEnabled = false;
        gameObject.SetActive(true);
        timerText = GetComponent<Text>();
    }

    public void EnableTimer()
    {
        timerEnabled = true;
    }

    public void DisableTimer()
    {
        timerEnabled = false;
    }
    // Returns time in seconds
    public int GetCurrentTime()
    {
        return (int) time;
    }
    void Update()
    {
        // Updates the time variable and updates the timer text in seconds
        if (timerEnabled)
        {
            time += Time.deltaTime;
            timerText.text = $"Time: {GetCurrentTime()}";
        }
    }
}
