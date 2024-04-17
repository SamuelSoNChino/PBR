using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float time;
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
            GetComponent<Text>().text = "Time: " + ((int) time).ToString();
        }
    }
}
