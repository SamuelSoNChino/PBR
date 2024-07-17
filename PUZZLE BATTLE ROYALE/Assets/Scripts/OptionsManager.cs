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
    /// The slider object for selecting the number of tiles.
    /// </summary>
    [SerializeField] private GameObject NOTSlider;

    /// <summary>
    /// The slider object for selecting the number of players.
    /// </summary>
    [SerializeField] private GameObject NOPSlider;

    /// <summary>
    /// The dropdown object for selecting the background.
    /// </summary>
    [SerializeField] private GameObject dropdown;

    /// <summary>
    /// Initializes the slider and dropdown values.
    /// </summary>
    void Start()
    {
        if (!PlayerPrefs.HasKey("numberOfTiles"))
        {
            PlayerPrefs.SetInt("numberOfTiles", 5);
        }

        if (!PlayerPrefs.HasKey("numberOfPlayers"))
        {
            PlayerPrefs.SetInt("numberOfPlayers", 2);
        }

        NOTSlider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfTiles");
        UpdateNOTSlider();

        NOPSlider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfPlayers");
        UpdateNOPSlider();

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
    /// Updates PlayerPrefs value and slider text when the NOP slider value is changed.
    /// </summary>
    public void UpdateNOPSlider()
    {
        PlayerPrefs.SetInt("numberOfPlayers", (int)NOPSlider.GetComponent<Slider>().value);

        Text text = NOPSlider.transform.Find("SliderText").GetComponent<Text>();

        text.GetComponent<Text>().text = "Number of players: " + PlayerPrefs.GetInt("numberOfPlayers").ToString();
    }

    /// <summary>
    /// Updates PlayerPrefs value and slider text when the NOT slider value is changed.
    /// </summary>
    public void UpdateNOTSlider()
    {
        PlayerPrefs.SetInt("numberOfTiles", (int)NOTSlider.GetComponent<Slider>().value);

        Text text = NOTSlider.transform.Find("SliderText").GetComponent<Text>();

        text.GetComponent<Text>().text = "Number of Tiles: " + PlayerPrefs.GetInt("numberOfTiles").ToString();
    }

    /// <summary>
    /// Updates PlayerPrefs background value when the dropdown was interacted with.
    /// </summary>
    public void SetNewBackground()
    {
        int chosenBackground = dropdown.GetComponent<TMP_Dropdown>().value;
        PlayerPrefs.SetInt("backgroundSkin", chosenBackground);
    }
}