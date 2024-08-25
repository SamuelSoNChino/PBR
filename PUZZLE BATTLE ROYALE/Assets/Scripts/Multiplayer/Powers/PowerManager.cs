using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : NetworkBehaviour
{
    [SerializeField] private PlayerManager playerManager;


    private void Start()
    {
        LoadEquippedPowers();
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

    [Rpc(SendTo.ClientsAndHost)]
    public void RequestEquippedPowersFromAllPlayersRpc()
    {
        SendEquippedPowersToServer();
    }

    // -----------------------------------------------------------------------
    // Power Buttons
    // -----------------------------------------------------------------------

    [SerializeField] private GameObject powerButtons;
    [SerializeField] private PeekManager peekManager;

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

    public void UpdatePowerButtonInteractability(Player player)
    {
        for (int i = 0; i < player.Powers.Count; i++)
        {
            Power power = player.GetPowerAtIndex(i);
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"{power.Name}: isOnCooldown = {player.IsPowerOnCooldown(power)}");
                Debug.Log($"{power.Name}: isPassive = {power.IsPassive}");
                Debug.Log($"{power.Name}: IsTargetable = {power.IsTargetable}");
                Debug.Log($"Player: IsPeeking = {player.IsPeeking}");
            }
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

    [Rpc(SendTo.ClientsAndHost)]
    public void DisablePowerButtonRpc(ulong clientId, int powerButtonIndex)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            powerButtons.transform.GetChild(powerButtonIndex).GetComponent<Button>().interactable = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePowerButtonRpc(ulong clientId, int powerButtonIndex)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            powerButtons.transform.GetChild(powerButtonIndex).GetComponent<Button>().interactable = true;
        }
    }

    public void UsePowerButton(int buttonIndex)
    {
        UsePowerButtonRpc(NetworkManager.Singleton.LocalClientId, buttonIndex);
    }

    [Rpc(SendTo.Server)]
    public void UsePowerButtonRpc(ulong clientId, int buttonIndex)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);
        Power power = player.GetPowerAtIndex(buttonIndex);

        if (!power.IsPassive && !player.IsPowerOnCooldown(power) && !(power.IsTargetable && !player.IsPeeking))
        {
            power.Activate();
            StartCoroutine(StartPowerCooldownTimer(player, power));
        }
    }

    // -----------------------------------------------------------------------
    // Power Cooldowns
    // -----------------------------------------------------------------------

    public event Action<Player> OnPowerCooldownStatusChange;

    public IEnumerator StartPowerCooldownTimer(Player player, Power power)
    {
        player.PutPowerOnCooldown(power);
        OnPowerCooldownStatusChange.Invoke(player);
        yield return new WaitForSeconds(power.CooldownDuration);
        player.PutPowerOffCooldown(power);
        OnPowerCooldownStatusChange.Invoke(player);
    }
}