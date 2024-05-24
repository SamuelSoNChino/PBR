using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject slider;
    void Start()
    {
        slider.GetComponent<Slider>().value = PlayerPrefs.GetInt("Tiles");
        UpdateSliderText();
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void UpdateSliderText()
    {
        PlayerPrefs.SetInt("Tiles", (int) slider.GetComponent<Slider>().value);
        text.GetComponent<Text>().text = "Singleplayer tiles: " + PlayerPrefs.GetInt("Tiles").ToString();
    }
}
