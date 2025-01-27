using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the Tornado power, which displaces a player's snapped puzzle tiles in a random direction.
/// </summary>
public class TornadoPower : Power
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TornadoPower"/> class.
    /// </summary>
    public TornadoPower() : base("Tornado", 0, false, true, 10)
    {
    }

    /// <summary>
    /// Activates the Tornado power on the specified target player, displacing a random selection of their snapped puzzle tiles.
    /// </summary>
    /// <param name="targetPlayer">The player on whom the power is activated.</param>
    public override void Activate(Player userPlayer, Player targetPlayer)
    {
        PuzzleManager puzzleManager = GameObject.Find("Puzzle").GetComponent<PuzzleManager>();

        // Collects the IDs of all snapped puzzle tiles.
        List<int> snappedPuzzleTilesIdsToDisplace = new();
        foreach (int puzzleTileId in puzzleManager.GetPuzzleTileIds())
        {
            if (targetPlayer.GetPuzzleTileSnappedGridTile(puzzleTileId) != -1)
            {
                snappedPuzzleTilesIdsToDisplace.Add(puzzleTileId);
            }
        }

        // Limits the number of tiles to be displaced to 6, removing random tiles if there are more.
        int numberOfDisplacedTiles = 4;
        while (snappedPuzzleTilesIdsToDisplace.Count > numberOfDisplacedTiles)
        {
            int randomIndex = Random.Range(0, snappedPuzzleTilesIdsToDisplace.Count);
            snappedPuzzleTilesIdsToDisplace.RemoveAt(randomIndex);
        }

        // Displaces each selected tile by a random distance in a random direction.
        foreach (int snappedPuzzleTileIdToDisplace in snappedPuzzleTilesIdsToDisplace)
        {
            puzzleManager.UnsnapTileFromGrid(targetPlayer, snappedPuzzleTileIdToDisplace);
            puzzleManager.MovePuzzleTileToFront(targetPlayer, snappedPuzzleTileIdToDisplace);

            int minDisplacement = 100;
            int maxDisplacement = 300;

            // Randomly determines the direction of displacement along the X and Y axes.
            List<int> possiblePolarities = new() { 1, -1 };
            int xPolarity = possiblePolarities[Random.Range(0, 2)];
            int yPolarity = possiblePolarities[Random.Range(0, 2)];
            Vector3 displacementVector = new(xPolarity * Random.Range(minDisplacement, maxDisplacement), yPolarity * Random.Range(minDisplacement, maxDisplacement), 0);

            // Applies the displacement to the tile's position.
            Vector3 oldPosition = targetPlayer.GetPuzzleTilePosition(snappedPuzzleTileIdToDisplace);
            Vector3 newPosition = oldPosition + displacementVector;
            targetPlayer.ModifyPuzzleTilePosition(snappedPuzzleTileIdToDisplace, newPosition);
            puzzleManager.UpdateTilePositionForPlayers(targetPlayer, snappedPuzzleTileIdToDisplace, newPosition, true);
        }

        // Ensures that the tile the player is currently holding (if any) is moved to the front.
        if (targetPlayer.HeldPuzzleTileId != -1)
        {
            puzzleManager.MovePuzzleTileToFront(targetPlayer, targetPlayer.HeldPuzzleTileId);
        }
    }
}
