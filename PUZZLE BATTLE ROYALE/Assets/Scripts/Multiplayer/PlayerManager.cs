using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<Player> players = new();

    public void AddNewPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
    }

    public List<Player> GetAllPlayers()
    {
        return new List<Player>(players);
    }

    public Player FindPlayerByClientId(ulong clientId)
    {
        foreach (Player player in players)
        {
            if (player.ClientId == clientId)
            {
                return player;
            }
        }
        return null;
    }

}
