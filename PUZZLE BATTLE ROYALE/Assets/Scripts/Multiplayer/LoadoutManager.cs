using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MonoBehvaiour that manages the loadout scene and power selection there.
/// </summary>
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

    /// <summary>
    /// List of available powers.
    /// </summary>
    private readonly List<Power> powers = new()
    {
        new TornadoPower(),
        new SecretPeekPower(),
        new ShieldPower(),
        new SlipperyGridPower(),
        new SoloLevelingPower(),
    };

    /// <summary>
    /// Gets the list of available powers.
    /// </summary>
    public List<Power> Powers
    {
        get { return powers; }
    }

    /// <summary>
    /// Finds a power by its unique ID.
    /// </summary>
    /// <param name="powerId">The ID of the power to find.</param>
    /// <returns>The power with the specified ID, or null if not found.</returns>
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

    /// <summary>
    /// Finds a power by its name.
    /// </summary>
    /// <param name="name">The name of the power to find.</param>
    /// <returns>The power with the specified name, or null if not found.</returns>
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

    /// <summary>
    /// List of sprites representing the power icons.
    /// </summary>
    [SerializeField] private List<Sprite> powerIconsSprites;

    /// <summary>
    /// Gets the sprite associated with a specific power.
    /// </summary>
    /// <param name="power">The power to get the sprite for.</param>
    /// <returns>The sprite representing the power.</returns>
    public Sprite GetPowerSprite(Power power)
    {
        return powerIconsSprites[power.Id];
    }

    // -----------------------------------------------------------------------
    // Equipped Powers
    // -----------------------------------------------------------------------

    /// <summary>
    /// List of currently equipped powers.
    /// </summary>
    private List<Power> equippedPowers = new();

    /// <summary>
    /// Gets the list of currently equipped powers.
    /// </summary>
    public List<Power> EquippedPowers
    {
        get { return equippedPowers; }
    }

    /// <summary>
    /// The number of power slots available for equipping.
    /// </summary>
    private readonly int numberOfPowerSlots = 3;

    /// <summary>
    /// Equips a new power in a specific slot.
    /// </summary>
    /// <param name="powerButtonIndex">The index of the power slot.</param>
    /// <param name="newPower">The new power to equip.</param>
    public void EquipPower(int powerButtonIndex, Power newPower)
    {
        equippedPowers.RemoveAt(powerButtonIndex);
        equippedPowers.Insert(powerButtonIndex, newPower);
        PlayerPrefs.SetInt($"equippedPower{powerButtonIndex}Id", newPower.Id);
    }

    /// <summary>
    /// Loads the equipped powers from saved preferences.
    /// </summary>
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

    /// <summary>
    /// UI element for selecting the number of players.
    /// </summary>
    [SerializeField] private GameObject numberOfPlayersSlider;

    /// <summary>
    /// Prefab for creating power entry buttons.
    /// </summary>
    [SerializeField] private GameObject powerEntryButtonPrefab;

    /// <summary>
    /// UI container for the power buttons.
    /// </summary>
    [SerializeField] private GameObject powerButtons;

    /// <summary>
    /// UI container for the power selection screen.
    /// </summary>
    [SerializeField] private GameObject powerSelection;

    /// <summary>
    /// Updates the number of players slider value and text.
    /// </summary>
    public void UpdateNumberOfPlayersSlider()
    {
        PlayerPrefs.SetInt("numberOfPlayers", (int)numberOfPlayersSlider.GetComponent<Slider>().value);

        Text text = numberOfPlayersSlider.transform.Find("SliderText").GetComponent<Text>();

        text.GetComponent<Text>().text = "Number of players: " + PlayerPrefs.GetInt("numberOfPlayers").ToString();
    }

    /// <summary>
    /// Updates the loadout display with the equipped powers.
    /// </summary>
    public void UpdateLoadout()
    {
        for (int i = 0; i < equippedPowers.Count; i++)
        {
            Image powerButtonImage = powerButtons.transform.Find($"Power{i + 1}").GetComponent<Image>();
            powerButtonImage.sprite = GetPowerSprite(equippedPowers[i]);
        }
    }

    /// <summary>
    /// Displays the power selection screen.
    /// </summary>
    /// <param name="powerButtonIndex">The index of the power button being selected.</param>
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

    /// <summary>
    /// Hides the power selection screen.
    /// </summary>
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

    /// <summary>
    /// Starts the multiplayer puzzle scene.
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene("PuzzleMultiplayer");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}