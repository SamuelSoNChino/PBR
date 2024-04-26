using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public GameObject text;
    public GameObject slider;
    void Start()
    {
        slider.GetComponent<Slider>().value = PlayerPrefs.GetInt("Tiles");
        UpdateSliderText();
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Update is called once per frame
    public void UpdateSliderText()
    {
        PlayerPrefs.SetInt("Tiles", (int) slider.GetComponent<Slider>().value);
        text.GetComponent<Text>().text = "Tiles " + PlayerPrefs.GetInt("Tiles").ToString();
    }
}
