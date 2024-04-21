using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float time;
    public int seconds;
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Puzzle").GetComponent<GameState>().gameActive)
        {
            time += Time.deltaTime;
            seconds = (int) time;
            GetComponent<Text>().text = "Time: " + seconds.ToString();
        }
    }
}
