using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    [SerializeField] private GameObject numberOfPlayersSlider;
    
    void Start()
    {
        if (!PlayerPrefs.HasKey("numberOfPlayers"))
        {
            PlayerPrefs.SetInt("numberOfPlayers", 2);
        }

        numberOfPlayersSlider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfPlayers");
        UpdatenumberOfPlayersSlider();
    }

    public void UpdatenumberOfPlayersSlider()
    {
        PlayerPrefs.SetInt("numberOfPlayers", (int)numberOfPlayersSlider.GetComponent<Slider>().value);

        Text text = numberOfPlayersSlider.transform.Find("SliderText").GetComponent<Text>();

        text.GetComponent<Text>().text = "Number of players: " + PlayerPrefs.GetInt("numberOfPlayers").ToString();
    }

    public void Play()
    {
        SceneManager.LoadScene("PuzzleMultiplayer");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
