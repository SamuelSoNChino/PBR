using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's powers, handling power selection, activation, cooldowns, and syncing across the network.
/// </summary>
public class PowerManager : NetworkBehaviour
{
    /// <summary>
    /// Reference to the PlayerManager component, used to manage players in the game.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// Called on the initialization of the script instance.
    /// </summary>
    private void Start()
    {
        LoadEquippedPowers();
    }

    // -----------------------------------------------------------------------
    // Powers
    // -----------------------------------------------------------------------

    /// <summary>
    /// List of all available powers in the game.
    /// </summary>
    private readonly List<Power> powers = new()
    {
        new TornadoPower(),
        new SecretPeekPower(),
        new ShieldPower(),
        new SlipperyGridPower(),
        new SoloLevelingPower(),
        new BerserkPower()
    };

    /// <summary>
    /// Gets the list of all available powers.
    /// </summary>
    public List<Power> Powers
    {
        get { return powers; }
    }

    /// <summary>
    /// Finds a power by its unique identifier.
    /// </summary>
    /// <param name="powerId">The unique identifier of the power.</param>
    /// <returns>The power corresponding to the specified ID, or null if not found.</returns>
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
    /// <param name="name">The name of the power.</param>
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
    /// List of sprites representing the icons of each power.
    /// </summary>
    [SerializeField] private List<Sprite> powerIconsSprites;

    /// <summary>
    /// Gets the sprite associated with a specific power.
    /// </summary>
    /// <param name="power">The power for which to get the sprite.</param>
    /// <returns>The sprite associated with the given power.</returns>
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
    /// Gets the list of equipped powers.
    /// </summary>
    public List<Power> EquippedPowers
    {
        get { return equippedPowers; }
    }

    /// <summary>
    /// The number of power slots available for the player to equip powers.
    /// </summary>
    private readonly int numberOfPowerSlots = 3;

    /// <summary>
    /// Loads the powers equipped by the player from saved preferences.
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

    /// <summary>
    /// Sends the equipped powers from a client to the server.
    /// </summary>
    /// <param name="clientId">The ID of the client sending the powers.</param>
    /// <param name="serializedEquippedPowersIds">A comma-separated string of power IDs representing the equipped powers.</param>
    [Rpc(SendTo.Server)]
    public void SendEquippedPowersToServerRpc(ulong clientId, string serializedEquippedPowersIds)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);

        List<string> equippedPowersIds = new(serializedEquippedPowersIds.Split(","));

        foreach (string powerIdString in equippedPowersIds)
        {
            int powerId = int.Parse(powerIdString);
            Power power = FindPowerById(powerId);
            if (!player.HasPower(power) && powers.Contains(power) && player.Powers.Count != numberOfPowerSlots)
            {
                player.AddPower(power);
            }
        }
    }

    /// <summary>
    /// Sends the equipped powers from the local player to the server.
    /// </summary>
    private void SendEquippedPowersToServer()
    {
        List<int> equippedPowersIds = new();
        foreach (Power power in equippedPowers)
        {
            equippedPowersIds.Add(power.Id);
        }
        string serializedEquippedPowersIds = string.Join(",", equippedPowersIds);
        SendEquippedPowersToServerRpc(NetworkManager.Singleton.LocalClientId, serializedEquippedPowersIds);
    }

    /// <summary>
    /// Requests the equipped powers from all players and syncs them with the server.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void RequestEquippedPowersFromAllPlayersRpc()
    {
        SendEquippedPowersToServer();
    }

    // -----------------------------------------------------------------------
    // Power Buttons
    // -----------------------------------------------------------------------

    /// <summary>
    /// The parent GameObject containing all the power buttons.
    /// </summary>
    [SerializeField] private GameObject powerButtons;

    /// <summary>
    /// Reference to the PeekManager component, used to manage the peeking functionality in the game.
    /// </summary>
    [SerializeField] private PeekManager peekManager;

    /// <summary>
    /// Initializes the power buttons for a specific client, setting up their icons and interactability based on the powers.
    /// </summary>
    /// <param name="clientId">The ID of the client to initialize the buttons for.</param>
    /// <param name="serializedPowersIds">A comma-separated string of power IDs to initialize the buttons with.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void InitializePowerButtonsRpc(ulong clientId, string serializedPowersIds)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            List<string> powersIds = new(serializedPowersIds.Split(","));
            for (int i = 0; i < powerButtons.transform.childCount; i++)
            {
                Power power = FindPowerById(int.Parse(powersIds[i]));
                Sprite powerSprite = GetPowerSprite(power);
                powerButtons.transform.GetChild(i).GetComponent<Image>().sprite = powerSprite;

                if (power.IsPassive || power.IsTargetable)
                {
                    powerButtons.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    /// <summary>
    /// Initializes the power buttons for all players in the game.
    /// </summary>
    public void InitializePowerButtonsForAllPlayers()
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            List<int> powerIds = new();
            foreach (Power power in player.Powers)
            {
                powerIds.Add(power.Id);

                if (!power.IsPassive)
                {
                    OnPowerCooldownStatusChange += UpdatePowerButtonInteractability;
                    peekManager.OnPlayerPeekingStatusChanged += UpdatePowerButtonInteractability;
                }
            }
            string serializedPowersIds = string.Join(",", powerIds);
            InitializePowerButtonsRpc(player.ClientId, serializedPowersIds);
        }
    }

    /// <summary>
    /// Updates the interactability of power buttons based on the player's power cooldown status and peeking status.
    /// </summary>
    /// <param name="player">The player whose power button interactability is being updated.</param>
    public void UpdatePowerButtonInteractability(Player player)
    {
        for (int i = 0; i < player.Powers.Count; i++)
        {
            Power power = player.GetPowerAtIndex(i);
            if (power.IsPassive)
            {
                continue;
            }

            if (!player.IsPowerOnCooldown(power) && !(power.IsTargetable && !player.IsPeeking))
            {
                EnablePowerButtonRpc(player.ClientId, i);
                continue;
            }
            DisablePowerButtonRpc(player.ClientId, i);
        }
    }

    /// <summary>
    /// Disables the power button for a specific power index on the client.
    /// </summary>
    /// <param name="clientId">The ID of the client.</param>
    /// <param name="powerButtonIndex">The index of the power button to disable.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void DisablePowerButtonRpc(ulong clientId, int powerButtonIndex)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            powerButtons.transform.GetChild(powerButtonIndex).GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// Enables the power button for a specific power index on the client.
    /// </summary>
    /// <param name="clientId">The ID of the client.</param>
    /// <param name="powerButtonIndex">The index of the power button to enable.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePowerButtonRpc(ulong clientId, int powerButtonIndex)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            powerButtons.transform.GetChild(powerButtonIndex).GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Uses the power associated with the button at the specified index.
    /// </summary>
    /// <param name="buttonIndex">The index of the button representing the power to use.</param>
    public void UsePowerButton(int buttonIndex)
    {
        UsePowerButtonRpc(NetworkManager.Singleton.LocalClientId, buttonIndex);
    }

    /// <summary>
    /// Uses the power associated with the button at the specified index on the server.
    /// </summary>
    /// <param name="clientId">The ID of the client using the power.</param>
    /// <param name="buttonIndex">The index of the button representing the power to use.</param>
    [Rpc(SendTo.Server)]
    public void UsePowerButtonRpc(ulong clientId, int buttonIndex)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);
        Power power = player.GetPowerAtIndex(buttonIndex);

        if (power.IsPassive || player.IsPowerOnCooldown(power))
        {
            return;
        }

        if (power.IsTargetable)
        {
            if (player.IsPeeking)
            {
                power.Activate(player.TargetOfPeekPlayer);
                StartCoroutine(StartPowerCooldownTimer(player, power));
            }
            return;
        }
        power.Activate();
        StartCoroutine(StartPowerCooldownTimer(player, power));
    }

    // -----------------------------------------------------------------------
    // Power Cooldowns
    // -----------------------------------------------------------------------

    /// <summary>
    /// Event triggered when the power cooldown status changes for a player.
    /// </summary>
    public event Action<Player> OnPowerCooldownStatusChange;

    /// <summary>
    /// Starts the cooldown timer for a power and updates its status when the cooldown ends.
    /// </summary>
    /// <param name="player">The player whose power is on cooldown.</param>
    /// <param name="power">The power that is on cooldown.</param>
    /// <returns>An IEnumerator used for the cooldown coroutine.</returns>
    public IEnumerator StartPowerCooldownTimer(Player player, Power power)
    {
        player.PutPowerOnCooldown(power);
        OnPowerCooldownStatusChange.Invoke(player);
        yield return new WaitForSeconds(power.CooldownDuration);
        player.PutPowerOffCooldown(power);
        OnPowerCooldownStatusChange.Invoke(player);
    }
}
