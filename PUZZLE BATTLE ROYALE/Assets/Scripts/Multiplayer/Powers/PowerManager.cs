using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    private Dictionary<string, Power> powers = new Dictionary<string, Power>();

    private void Start()
    {
        // Create instances of powers and store them in the dictionary
        powers.Add("Shield", new ShieldPower());
        // Add more powers as needed
    }

    public void ActivatePower(string powerName)
    {
        if (powers.ContainsKey(powerName))
        {
            powers[powerName].Activate();
        }
        else
        {
            Debug.LogWarning($"Power {powerName} not found!");
        }
    }
}