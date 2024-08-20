using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    private void Start()
    {
        if (!PlayerPrefs.HasKey("numberOfPlayers"))
        {
            PlayerPrefs.SetInt("numberOfPlayers", 2);
        }

        numberOfPlayersSlider.GetComponent<Slider>().value = PlayerPrefs.GetInt("numberOfPlayers");
        UpdateNumberOfPlayersSlider();

        LoadEquippedPowers();
        UpdateLoadout();
    }

    // -----------------------------------------------------------------------
    // Powers
    // -----------------------------------------------------------------------

    private readonly List<Power> powers = new()
    {
        new TornadoPower(),
        new SecretPeekPower(),
        new ShieldPower(),
        new SlipperyGridPower()
    };

    public List<Power> Powers
    {
        get { return powers; }
    }

    public Power FindPowerById(int powerId)
    {
        foreach (Power power in powers)
        {
            if (power.Id == powerId)
            {
                return power;
            }
        }
        return null;
    }

    public Power FindPowerByName(string name)
    {
        foreach (Power power in powers)
        {
            if (power.Name == name)
            {
                return power;
            }
        }
        return null;
    }

    [SerializeField] private List<Sprite> powerIconsSprites;

    public Sprite GetPowerSprite(Power power)
    {
        return powerIconsSprites[power.Id];
    }

    // -----------------------------------------------------------------------
    // Equipped Powers
    // -----------------------------------------------------------------------

    private List<Power> equippedPowers = new();
    public List<Power> EquippedPowers
    {
        get { return equippedPowers; }
    }

    private readonly int numberOfPowerSlots = 3;

    public void EquipPower(int powerButtonIndex, Power newPower)
    {
        equippedPowers.RemoveAt(powerButtonIndex);
        equippedPowers.Insert(powerButtonIndex, newPower);
        PlayerPrefs.SetInt($"equippedPower{powerButtonIndex}Id", newPower.Id);
    }

    private void LoadEquippedPowers()
    {
        for (int i = 0; i < numberOfPowerSlots; i++)
        {
            if (PlayerPrefs.HasKey($"equippedPower{i}Id"))
            {
                equippedPowers.Add(FindPowerById(PlayerPrefs.GetInt($"equippedPower{i}Id")));
            }
        }

        if (equippedPowers.Count == numberOfPowerSlots)
        {
            return;
        }

        for (int i = 0; i < numberOfPowerSlots - equippedPowers.Count; i++)
        {
            foreach (Power power in powers)
            {
                if (!equippedPowers.Contains(power))
                {
                    equippedPowers.Add(power);
                    PlayerPrefs.SetInt($"equippedPower{i}Id", power.Id);
                    break;
                }
            }
        }
    }

    // -----------------------------------------------------------------------
    // UI Functionality
    // -----------------------------------------------------------------------

    [SerializeField] private GameObject numberOfPlayersSlider;
    [SerializeField] private GameObject powerEntryButtonPrefab;
    [SerializeField] private GameObject powerButtons;
    [SerializeField] private GameObject powerSelection;

    public void UpdateNumberOfPlayersSlider()
    {
        PlayerPrefs.SetInt("numberOfPlayers", (int)numberOfPlayersSlider.GetComponent<Slider>().value);

        Text text = numberOfPlayersSlider.transform.Find("SliderText").GetComponent<Text>();

        text.GetComponent<Text>().text = "Number of players: " + PlayerPrefs.GetInt("numberOfPlayers").ToString();
    }

    public void UpdateLoadout()
    {
        for (int i = 0; i < equippedPowers.Count; i++)
        {
            Image powerButtonImage = powerButtons.transform.Find($"Power{i + 1}").GetComponent<Image>();
            powerButtonImage.sprite = GetPowerSprite(equippedPowers[i]);
        }
    }

    public void ShowPowerSelection(int powerButtonIndex)
    {
        if (powerSelection.activeSelf)
        {
            HidePowerSelection();
        }

        powerSelection.SetActive(true);

        foreach (Power power in powers)
        {
            GameObject powerEntryButton = Instantiate(powerEntryButtonPrefab);
            powerEntryButton.transform.SetParent(powerSelection.transform.GetChild(0).transform);

            Image powerEntryButtonImage = powerEntryButton.transform.Find("PowerEntryButton").GetComponent<Image>();
            powerEntryButtonImage.sprite = GetPowerSprite(power);

            TextMeshProUGUI powerEntryBUttonText = powerEntryButton.transform.Find("PowerEntryText").GetComponent<TextMeshProUGUI>();
            powerEntryBUttonText.text = power.Name;

            Button powerEntryButtonButton = powerEntryButton.transform.Find("PowerEntryButton").GetComponent<Button>();
            if (equippedPowers.Contains(power))
            {
                powerEntryButtonButton.interactable = false;
            }
            else
            {
                powerEntryButtonButton.onClick.AddListener(() => EquipPower(powerButtonIndex, power));
                powerEntryButtonButton.onClick.AddListener(() => HidePowerSelection());
            }
        }
    }

    public void HidePowerSelection()
    {
        foreach (Transform powerEntry in powerSelection.transform.GetChild(0).transform)
        {
            if (powerEntry.name != "CancelButton")
            {
                Destroy(powerEntry.gameObject);
            }
        }
        powerSelection.SetActive(false);
        UpdateLoadout();
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
