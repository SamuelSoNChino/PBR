using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the loading of the background skin.
/// </summary>
public class BackgroundManager : NetworkBehaviour
{
    /// <summary>
    /// The background GameObject in the scene.
    /// </summary>
    [SerializeField] private GameObject background;

    /// <summary>
    /// Array of available background skins.
    /// </summary>
    [SerializeField] private Sprite[] backgroundSkins;

    /// <summary>
    /// Loads the selected background on Awake to ensure PanZoom calculates borders correctly on Start.
    /// </summary>
    private void Awake()
    {
        LoadOriginalBackground();
    }

    /// <summary>
    /// Loads the original background skin based on player preferences.
    /// </summary>
    public void LoadOriginalBackground()
    {
        // If background wasn't selected in options yet, selects a default value
        if (!PlayerPrefs.HasKey("backgroundSkin"))
        {
            PlayerPrefs.SetInt("backgroundSkin", 0);
        }
    }
}