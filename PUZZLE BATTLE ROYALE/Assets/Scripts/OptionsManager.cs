using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the options scene.
/// </summary>
public class OptionsManager : MonoBehaviour
{
    /// <summary>
    /// The text object displaying the number of tiles.
    /// </summary>
    [SerializeField] GameObject text;

    /// <summary>
    /// The slider object for selecting the number of tiles.
    /// </summary>
    [SerializeField] GameObject slider;

    /// <summary>
    /// Initializes the slider value and updates the slider text to the last saved state.
    /// </summary>
    void Start()
    {
        // If numberOfTiles wasn't set yet, sets it to the default value
        if (!PlayerPrefs.HasKey("numberOfTiles"))
        {
            PlayerPrefs.SetInt("numberOfTiles", 5);
        }
        slider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfTiles");
        UpdateSliderText();
    }

    /// <summary>
    /// Loads the menu scene.
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Updates PlayerPrefs value and slider text when the slider value is changed.
    /// </summary>
    public void UpdateSliderText()
    {
        PlayerPrefs.SetInt("numberOfTiles", (int)slider.GetComponent<Slider>().value);
        text.GetComponent<Text>().text = "Singleplayer tiles: " + PlayerPrefs.GetInt("numberOfTiles").ToString();
    }
}