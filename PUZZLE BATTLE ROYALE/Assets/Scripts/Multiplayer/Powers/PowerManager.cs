using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : NetworkBehaviour
{
    // -----------------------------------------------------------------------
    // UNIVERSAL
    // -----------------------------------------------------------------------

    private void Start()
    {
        LoadEquippedPowers();
        loadoutManager.UpdateLoadout();
    }

    // -----------------------------------------------------------------------
    // Powers
    // -----------------------------------------------------------------------

    private readonly List<Power> powers = new List<Power>
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



    // -----------------------------------------------------------------------
    // CLIENT
    // -----------------------------------------------------------------------

    // -----------------------------------------------------------------------
    // Power Buttons and Sprites
    // -----------------------------------------------------------------------

    [SerializeField] private List<Sprite> powerIconsSprites;
    private List<Button> powerButtons;

    public Sprite GetPowerSprite(Power power)
    {
        return powerIconsSprites[power.Id];
    }

    private void FindPowerButtons()
    {
        for (int i = 0; i < numberOfPowerSlots; i++)
        {
            powerButtons.Add(GameObject.Find($"PowerButton{i + 1}").GetComponent<Button>());
        }
    }

    // [Rpc(SendTo.ClientsAndHost)]
    // public void LoadEquippedPowersSpritesRpc(ulong clientId, List<int> equippedPowersSpritesIds)
    // {
    //     if (true)
    //     {

    //     }
    //     for (int i = 0; i < equippedPowersSpritesIds.Count; i++)
    //     {

    //     }
    // }

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

    // -----------------------------------------------------------------------
    // Loadout functionality
    // -----------------------------------------------------------------------

    [SerializeField] private LoadoutManager loadoutManager;

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
    // SERVER
    // -----------------------------------------------------------------------

    [SerializeField] private PlayerManager playerManager;

    // [Rpc(SendTo.Server)]
    // public void SendEquippedPowersToServerRpc(ulong clientId, List<Power> equippedPowers)
    // {
    //     Player player = playerManager.FindPlayerByClientId(clientId);
    //     foreach (Power power in equippedPowers)
    //     {
    //         if (!player.HasPower(power) && powers.Contains(power) && player.Powers.Count != 3)
    //         {
    //             player.AddPower(power);
    //         }
    //     }
    // }
}