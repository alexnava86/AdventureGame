// =============================================================================
// UISettings.cs   —   Assets/Scripts/Utilities/
//
// ScriptableObject that holds every UI customisation preference.
// One instance lives at  Assets/Resources/Settings/UISettings.asset
// It is loaded at runtime via Resources.Load rather than dragged into
// Inspector fields, so no GameManager reference is required.
//
// PlayerPrefs stores persistent data as a single JSON blob under the key
// "UISettings".  All reads/writes go through Load() and Save().
// =============================================================================

using System;
using UnityEngine;

/// <summary>How much of the HUD the player wants visible.</summary>
public enum HudMode
{
    Continuous,        // always show the full HUD (default)
    Discrete,          // show HUD only briefly when taking damage or attacking
    Minimal,           // show only a chosen subset of HUD elements, always
    MinimalDiscrete,   // minimal subset, and only flashed on damage/attack
    Hidden             // no HUD elements at all
}

[CreateAssetMenu(fileName = "UISettings", menuName = "Game/UI Settings")]
public class UISettings : ScriptableObject
{
    // ── Art selection indices (used to index into Resources arrays) ───────────

    [Header("Style & Font indices (0-based)")]
    [Tooltip("One index drives the matching border, box, AND gradient so they always " +
             "stay a coordinated set. Selecting style 1 uses border 1, box 1, and gradient 1.")]
    public int styleIndex        = 0;
    public int fontIndex         = 0;

    // Backward-compatible accessors so existing code that asked for separate
    // border/box indices keeps working — they all resolve to the single styleIndex.
    public int textBorderIndex { get { return styleIndex; } set { styleIndex = value; } }
    public int textBoxIndex    { get { return styleIndex; } set { styleIndex = value; } }
    public int gradientIndex   { get { return styleIndex; } set { styleIndex = value; } }

    // ── Colors ────────────────────────────────────────────────────────────────

    [Header("Colors")]
    public Color textBorderColor         = Color.white;
    public Color textBoxColor            = new Color(0f, 0.5f, 0.5f, 1f);
    public Color textColor               = Color.white;
    public Color textShadowColor         = Color.black;
    public Color gradientColor           = new Color(0f, 0f, 0f, 0f);

    // ── Transparency / Toggles ────────────────────────────────────────────────

    [Header("Transparency")]
    [Range(0f, 1f)] public float textBoxTransparency      = 1f;
    [Range(0f, 1f)] public float gradientTransparency     = 0f;
    [Range(0f, 1f)] public float textShadowTransparency   = 1f;
    public bool gradientEnabled = false;

    // ── Audio ─────────────────────────────────────────────────────────────────

    [Header("Audio")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume   = 1f;

    // ── Gameplay ──────────────────────────────────────────────────────────────

    [Header("HUD")]
    [Tooltip("How much of the HUD shows during play. See HudMode for what each means.")]
    public HudMode hudMode = HudMode.Continuous;

    [Header("Gameplay (Debug)")]
    [Tooltip("Affects enemy AI difficulty, spawn frequency, and treasure rates. " +
             "Not exposed to the player in production — set via DevMode/DebugMenu only.")]
    [Range(1, 5)] public int difficulty = 3;

    // =========================================================================
    // Singleton accessor
    // =========================================================================

    private static UISettings _instance;

    /// <summary>
    /// Returns the loaded UISettings asset, loading it from Resources if needed.
    /// Always call this rather than holding your own reference.
    /// </summary>
    public static UISettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<UISettings>("Settings/UISettings");
                if (_instance == null)
                {
                    Debug.LogError("[UISettings] No asset found at Resources/Settings/UISettings. " +
                                   "Create one via Assets ▸ Create ▸ Game ▸ UI Settings.");
                    _instance = CreateInstance<UISettings>();   // runtime fallback
                }
                _instance.Load();
            }
            return _instance;
        }
    }

    // =========================================================================
    // Persistence via PlayerPrefs JSON
    // =========================================================================

    private const string PREFS_KEY = "UISettings";

    /// <summary>Saves current values to PlayerPrefs as JSON.</summary>
    public void Save()
    {
        var data = new SaveData(this);
        PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    /// <summary>Loads values from PlayerPrefs. No-op if no saved data exists yet.</summary>
    public void Load()
    {
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var data = JsonUtility.FromJson<SaveData>(json);
            data.Apply(this);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UISettings] Could not deserialise saved prefs: {e.Message}. " +
                              "Using defaults.");
        }
    }

    /// <summary>Resets all fields to their default values and saves.</summary>
    public void ResetToDefaults()
    {
        styleIndex             = 0;
        fontIndex              = 0;
        textBorderColor        = Color.white;
        textBoxColor           = new Color(0f, 0.5f, 0.5f, 1f);
        textColor              = Color.white;
        textShadowColor        = Color.black;
        gradientColor          = new Color(0f, 0f, 0f, 0f);
        textBoxTransparency    = 1f;
        gradientTransparency   = 0f;
        textShadowTransparency = 1f;
        gradientEnabled        = false;
        musicVolume            = 1f;
        sfxVolume              = 1f;
        difficulty             = 3;
        hudMode                = HudMode.Continuous;
        Save();
    }

    // =========================================================================
    // Inner serialisable DTO — only plain types, no UnityEngine.Object refs
    // JsonUtility can't serialise Color directly via class fields in some
    // Unity versions, so we store as float arrays.
    // =========================================================================

    [Serializable]
    private class SaveData
    {
        public int   styleIndex, fontIndex, difficulty;
        public int   hudMode;   // stored as int (enum index) for JSON
        public float[] textBorderColor, textBoxColor, textColor,
                       textShadowColor, gradientColor;
        public float textBoxTransparency, gradientTransparency, textShadowTransparency;
        public float musicVolume, sfxVolume;
        public bool  gradientEnabled;

        public SaveData(UISettings s)
        {
            styleIndex             = s.styleIndex;
            fontIndex              = s.fontIndex;
            difficulty             = s.difficulty;
            hudMode                = (int)s.hudMode;
            textBorderColor        = ColorToArr(s.textBorderColor);
            textBoxColor           = ColorToArr(s.textBoxColor);
            textColor              = ColorToArr(s.textColor);
            textShadowColor        = ColorToArr(s.textShadowColor);
            gradientColor          = ColorToArr(s.gradientColor);
            textBoxTransparency    = s.textBoxTransparency;
            gradientTransparency   = s.gradientTransparency;
            textShadowTransparency = s.textShadowTransparency;
            musicVolume            = s.musicVolume;
            sfxVolume              = s.sfxVolume;
            gradientEnabled        = s.gradientEnabled;
        }

        public void Apply(UISettings s)
        {
            s.styleIndex             = styleIndex;
            s.fontIndex              = fontIndex;
            s.difficulty             = difficulty;
            s.hudMode                = (HudMode)hudMode;
            s.textBorderColor        = ArrToColor(textBorderColor);
            s.textBoxColor           = ArrToColor(textBoxColor);
            s.textColor              = ArrToColor(textColor);
            s.textShadowColor        = ArrToColor(textShadowColor);
            s.gradientColor          = ArrToColor(gradientColor);
            s.textBoxTransparency    = textBoxTransparency;
            s.gradientTransparency   = gradientTransparency;
            s.textShadowTransparency = textShadowTransparency;
            s.musicVolume            = musicVolume;
            s.sfxVolume              = sfxVolume;
            s.gradientEnabled        = gradientEnabled;
        }

        private static float[] ColorToArr(Color c) => new[] { c.r, c.g, c.b, c.a };
        private static Color   ArrToColor(float[] a) =>
            a != null && a.Length == 4 ? new Color(a[0], a[1], a[2], a[3]) : Color.white;
    }
}
