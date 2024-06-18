using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private GameObject text;

    /// <summary>
    /// The slider object for selecting the number of tiles.
    /// </summary>
    [SerializeField] private GameObject slider;

    /// <summary>
    /// The dropdown object for selecting the background.
    /// </summary>
    [SerializeField] private GameObject dropdown;

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

        // Sets the slider value to the stored value in PlayerPrefs and Updates its text
        slider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfTiles");
        UpdateSliderText();

        if (PlayerPrefs.HasKey("backgroundSkin"))
        {
            dropdown.GetComponent<TMP_Dropdown>().SetValueWithoutNotify(PlayerPrefs.GetInt("backgroundSkin"));
        }
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
        // Saves the new value to PlayerPrefs
        PlayerPrefs.SetInt("numberOfTiles", (int)slider.GetComponent<Slider>().value);

        // Updates the slider text
        text.GetComponent<Text>().text = "Singleplayer tiles: " + PlayerPrefs.GetInt("numberOfTiles").ToString();
    }

    public void SetNewBackground()
    {
        int chosenBackground = dropdown.GetComponent<TMP_Dropdown>().value;
        PlayerPrefs.SetInt("backgroundSkin", chosenBackground);
    }
}