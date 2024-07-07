using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player-related operations such as adding new players and retrieving player information.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// A list to store the players.
    /// </summary>
    private List<Player> players = new();

    /// <summary>
    /// Adds a new player to the list of players.
    /// </summary>
    /// <param name="newPlayer">The new player to add.</param>
    public void AddNewPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
    }

    /// <summary>
    /// Removes a player from the list of players.
    /// </summary>
    /// <param name="clientId">Id of the client to remove.</param>
    public void RemovePlayer(ulong clientId)
    {
        Player player = FindPlayerByClientId(clientId);
        players.Remove(player);
    }

    /// <summary>
    /// Retrieves a list of all players.
    /// </summary>
    /// <returns>A list of all players.</returns>
    public List<Player> GetAllPlayers()
    {
        return new List<Player>(players);
    }

    /// <summary>
    /// Finds a player by their client ID.
    /// </summary>
    /// <param name="clientId">The client ID of the player to find.</param>
    /// <returns>The player with the specified client ID, or null if not found.</returns>
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
