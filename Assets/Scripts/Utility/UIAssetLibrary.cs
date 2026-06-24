// =============================================================================
// UIAssetLibrary.cs   —   Assets/Scripts/Utilities/
//
// Loads all UI art assets from Resources at startup.
// Add art by dropping files into the correct subfolder — no Inspector wiring.
//
// Expected folder layout (relative to Assets/Resources/):
//   Art/UI/Borders/   ← dialogue border sprites
//   Art/UI/Boxes/     ← text box background sprites
//   Art/UI/Gradients/ ← gradient overlay sprites
//   Art/UI/Fonts/     ← Font assets  (.asset / TMP_FontAsset)
//
// All arrays are sorted alphabetically so index 0 is always the same asset
// regardless of Unity's internal import order, keeping PlayerPrefs stable.
// =============================================================================

using System.Linq;
using UnityEngine;

public class UIAssetLibrary : MonoBehaviour
{
    // ── Resource paths ────────────────────────────────────────────────────────
    private const string PATH_BORDERS   = "Art/UI/Borders";
    private const string PATH_BOXES     = "Art/UI/Boxes";
    private const string PATH_GRADIENTS = "Art/UI/Gradients";
    private const string PATH_FONTS     = "Art/UI/Fonts";

    // ── Singleton ─────────────────────────────────────────────────────────────
    public static UIAssetLibrary Instance { get; private set; }

    // ── Loaded arrays ─────────────────────────────────────────────────────────
    public Sprite[] Borders   { get; private set; }
    public Sprite[] Boxes     { get; private set; }
    public Sprite[] Gradients { get; private set; }
    public Font[]   Fonts     { get; private set; }

    // ── Convenience accessors (clamped so index is never out of range) ─────────
    public Sprite GetBorder(int i)   => Borders   != null && Borders.Length   > 0 ? Borders  [Mathf.Clamp(i, 0, Borders.Length   - 1)] : null;
    public Sprite GetBox(int i)      => Boxes     != null && Boxes.Length     > 0 ? Boxes    [Mathf.Clamp(i, 0, Boxes.Length     - 1)] : null;
    public Sprite GetGradient(int i) => Gradients != null && Gradients.Length > 0 ? Gradients[Mathf.Clamp(i, 0, Gradients.Length - 1)] : null;
    public Font   GetFont(int i)     => Fonts     != null && Fonts.Length     > 0 ? Fonts    [Mathf.Clamp(i, 0, Fonts.Length     - 1)] : null;

    public int BorderCount   => Borders   != null ? Borders.Length   : 0;
    public int BoxCount      => Boxes     != null ? Boxes.Length     : 0;
    public int GradientCount => Gradients != null ? Gradients.Length : 0;
    public int FontCount     => Fonts     != null ? Fonts.Length     : 0;

    // =========================================================================
    // MonoBehaviour
    // =========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    // =========================================================================
    // Loading
    // =========================================================================

    private void LoadAll()
    {
        Borders   = LoadSorted<Sprite>(PATH_BORDERS);
        Boxes     = LoadSorted<Sprite>(PATH_BOXES);
        Gradients = LoadSorted<Sprite>(PATH_GRADIENTS);
        Fonts     = LoadSorted<Font>(PATH_FONTS);

        Debug.Log($"[UIAssetLibrary] Loaded  " +
                  $"{Borders.Length} borders  |  {Boxes.Length} boxes  |  " +
                  $"{Gradients.Length} gradients  |  {Fonts.Length} fonts");
    }

    private static T[] LoadSorted<T>(string path) where T : Object
    {
        T[] all = Resources.LoadAll<T>(path);
        if (all == null || all.Length == 0) return new T[0];
        return all.OrderBy(a => a.name).ToArray();
    }
}
