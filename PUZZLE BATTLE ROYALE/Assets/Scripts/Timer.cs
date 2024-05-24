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

    void Start()
    {
        timerEnabled = false;
        gameObject.SetActive(true);
        timerText =  GetComponent<Text>();
    }

    public void EnableTimer()
    {
        timerEnabled = true;
    }

    public void DisableTimer()
    {
        timerEnabled = false;
    }
    public int GetCurrentTime()
    {
        return (int) time;
    }

    void Update()
    {
        if (timerEnabled)
        {
            time += Time.deltaTime;
            timerText.text = $"Time: {GetCurrentTime()}";
        }
    }
}
