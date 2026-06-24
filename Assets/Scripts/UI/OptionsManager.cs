// =============================================================================
// OptionsManager.cs   —   Assets/Scripts/UI/
//
// Drop this on a Canvas/Panel that represents the options screen.
// Attach the UI elements listed in the Inspector section.
// The GameManager does NOT need to hold any settings; UISettings.Instance
// is the single source of truth, persisted via PlayerPrefs.
//
// Input is handled through the new Unity Input System.
// Hook up the generated PlayerBaseInput actions in Awake() below.
// The screen also works fully with mouse only.
//
// Design pattern retained from the original: a private IOptionBehaviour
// interface + per-option concrete classes means Update() has NO if-chains —
// it calls currentOption.OnLeft() / OnRight() / OnConfirm() / OnCancel() and
// each option class handles its own logic.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OptionsManager : MonoBehaviour
{
    // =========================================================================
    // Inspector
    // =========================================================================

    [Header("Navigation")]
    [Tooltip("Highlight image that tracks the currently selected row.")]
    public Image rowCursor;

    [Header("Preview — live dialogue sample")]
    public Image  previewBorder;
    public Image  previewBox;
    public Image  previewGradient;
    public Text   previewText;

    [Header("Border row")]
    public Image[] borderThumbnails;

    [Header("Box row")]
    public Image[] boxThumbnails;

    [Header("Gradient row")]
    public Image   gradientPreview;
    public Slider  gradientTransparencySlider;
    public Toggle  gradientToggle;

    [Header("Colors — Text Border")]
    public Slider borderColorR, borderColorG, borderColorB;

    [Header("Colors — Text Box")]
    public Slider boxColorR, boxColorG, boxColorB;
    public Slider boxTransparencySlider;

    [Header("Colors — Text")]
    public Slider textColorR, textColorG, textColorB;

    [Header("Colors — Text Shadow")]
    public Slider shadowColorR, shadowColorG, shadowColorB;
    public Slider shadowTransparencySlider;

    [Header("Font row")]
    public Text[] fontLabels;

    [Header("Audio")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Difficulty  (debug / dev mode only — hide this row in production)")]
    [Tooltip("Assign the difficulty row's root GameObject here. " +
             "OnEnable shows it only when GameManager.Instance.DevMode is true.")]
    public GameObject difficultyRow;
    public Text[]     difficultyLabels;

    [Header("Buttons")]
    public Button saveButton;
    public Button defaultsButton;
    public Button cancelButton;

    // =========================================================================
    // Singleton
    // =========================================================================

    public static OptionsManager Instance { get; private set; }

    // =========================================================================
    // Private state
    // =========================================================================

    // The state machine: one concrete object per navigable row
    private IOptionBehaviour _current;
    private List<IOptionBehaviour> _options;
    private int _optionIndex;

    // Input
    private PlayerBaseInput _input;
    private Vector2         _navInput;
    private bool            _suppressNav;   // prevents double-fire on same frame

    // Snapshot for Cancel: taken when the screen opens
    private string _snapshot;

    // =========================================================================
    // IOptionBehaviour interface — each row implements this
    // =========================================================================

    private interface IOptionBehaviour
    {
        void OnFocus();             // called when this row is selected
        void OnLeft();              // left arrow / left stick
        void OnRight();             // right arrow / right stick
        void OnConfirm();           // A / Enter — enter sub-mode or toggle
        void OnCancel();            // B / Escape — exit sub-mode
        void Refresh();             // re-reads UISettings and updates UI elements
    }

    // =========================================================================
    // MonoBehaviour
    // =========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // New Input System wiring
        _input = new PlayerBaseInput();
        _input.Character.Disable();
        _input.Overworld.Disable();
        // Optionally define a "UI" action map in your input asset; otherwise
        // poll directly from the actions bound below.
        // We subscribe to Navigate and Submit on the current UI action map:
        _input.Enable();
    }

    private void OnEnable()
    {
        // Ensure UISettings is loaded from PlayerPrefs before we read any values
        _ = UISettings.Instance;

        // Show difficulty row only in DevMode / DebugMode
        bool devMode = GameManager.Instance != null && GameManager.Instance.DevMode;
        if (difficultyRow != null) difficultyRow.SetActive(devMode);

        // Snapshot current settings so Cancel can restore them
        _snapshot = JsonUtility.ToJson(UISettings.Instance);

        // Build the ordered option list once, using the live UISettings/asset refs
        BuildOptionList();
        _optionIndex = 0;
        FocusOption(_optionIndex);

        // Sync all sliders and thumbnails to current UISettings
        foreach (var opt in _options) opt.Refresh();

        // Button wiring (also works via mouse click)
        saveButton?.onClick.AddListener(SaveSettings);
        defaultsButton?.onClick.AddListener(ResetToDefaults);
        cancelButton?.onClick.AddListener(CancelChanges);
    }

    private void OnDisable()
    {
        saveButton?.onClick.RemoveListener(SaveSettings);
        defaultsButton?.onClick.RemoveListener(ResetToDefaults);
        cancelButton?.onClick.RemoveListener(CancelChanges);
    }

    private void OnDestroy()
    {
        _input?.Dispose();
    }

    private void Update()
    {
        // ── Read navigate input (keyboard, controller, new input system) ─────
        // Uses Keyboard directly so we don't need a dedicated UI action map.
        // Replace with your input action callbacks if you add a Menu action map.
        Vector2 nav = Vector2.zero;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame)  nav.y = -1f;
            if (kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame)  nav.y =  1f;
            if (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame)  nav.x = -1f;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)  nav.x =  1f;
        }

        var gp = Gamepad.current;
        if (gp != null && nav == Vector2.zero)
        {
            Vector2 stick = gp.leftStick.ReadValue();
            Vector2 dpad  = gp.dpad.ReadValue();
            Vector2 raw   = (dpad.sqrMagnitude > 0.25f) ? dpad : stick;
            if (raw.y >  0.5f) nav.y =  1f;
            if (raw.y < -0.5f) nav.y = -1f;
            if (raw.x < -0.5f) nav.x = -1f;
            if (raw.x >  0.5f) nav.x =  1f;
        }

        // ── Vertical navigation = row change ─────────────────────────────────
        if (nav.y != 0f)
        {
            int next = _optionIndex - (int)nav.y;
            next = Mathf.Clamp(next, 0, _options.Count - 1);
            if (next != _optionIndex)
            {
                _optionIndex = next;
                FocusOption(_optionIndex);
            }
        }

        // ── Horizontal navigation = value change within current row ──────────
        if (nav.x < 0f) _current?.OnLeft();
        if (nav.x > 0f) _current?.OnRight();

        // ── Confirm / Cancel ─────────────────────────────────────────────────
        bool confirm = kb?.enterKey.wasPressedThisFrame  ?? false;
        confirm     |= kb?.spaceKey.wasPressedThisFrame  ?? false;
        confirm     |= gp?.buttonSouth.wasPressedThisFrame ?? false;

        bool cancel  = kb?.escapeKey.wasPressedThisFrame ?? false;
        cancel      |= gp?.buttonEast.wasPressedThisFrame ?? false;

        if (confirm) _current?.OnConfirm();
        if (cancel)  _current?.OnCancel();
    }

    // =========================================================================
    // Navigation helpers
    // =========================================================================

    private void FocusOption(int index)
    {
        _current = _options[index];
        _current.OnFocus();

        // Move the cursor graphic to the selected row's first UI element
        // (row layout handled in prefab — override here if you need exact pixel offsets)
    }

    // =========================================================================
    // Button actions (also called by keyboard shortcuts)
    // =========================================================================

    private void SaveSettings()
    {
        UISettings.Instance.Save();
        ApplySettingsToScene();
        Debug.Log("[OptionsManager] Settings saved.");
    }

    private void ResetToDefaults()
    {
        UISettings.Instance.ResetToDefaults();
        foreach (var opt in _options) opt.Refresh();
        ApplySettingsToScene();
    }

    private void CancelChanges()
    {
        // Restore snapshot taken on open
        if (!string.IsNullOrEmpty(_snapshot))
            JsonUtility.FromJsonOverwrite(_snapshot, UISettings.Instance);
        foreach (var opt in _options) opt.Refresh();
        ApplySettingsToScene();
        gameObject.SetActive(false);
    }

    // =========================================================================
    // Apply settings to every live UI element in the scene
    // Call this whenever a setting changes so the preview updates live.
    // =========================================================================

    public static void ApplySettingsToScene()
    {
        var s  = UISettings.Instance;
        var lib = UIAssetLibrary.Instance;
        if (s == null || lib == null) return;

        // Preview
        if (Instance != null)
        {
            if (Instance.previewBorder   != null) Instance.previewBorder.sprite = lib.GetBorder(s.textBorderIndex);
            if (Instance.previewBox      != null)
            {
                Instance.previewBox.sprite = lib.GetBox(s.textBoxIndex);
                Instance.previewBox.color  = new Color(s.textBoxColor.r, s.textBoxColor.g, s.textBoxColor.b, s.textBoxTransparency);
            }
            if (Instance.previewGradient != null)
            {
                Instance.previewGradient.sprite  = lib.GetGradient(s.textBorderIndex);
                Instance.previewGradient.color   = new Color(s.gradientColor.r, s.gradientColor.g, s.gradientColor.b, s.gradientEnabled ? s.gradientTransparency : 0f);
            }
            if (Instance.previewText != null)
            {
                Instance.previewText.font  = lib.GetFont(s.fontIndex);
                Instance.previewText.color = s.textColor;
            }
        }

        // Broadcast to any listener — dialogue boxes, quest UI, etc.
        // Anything that implements IUISettingsConsumer refreshes itself here.
        // Include INACTIVE objects too — dialogue boxes are usually inactive until
        // they speak, but they still need to be styled so they look right when shown.
        foreach (var consumer in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (consumer is IUISettingsConsumer c) c.OnSettingsChanged(s);
        }
    }

    // =========================================================================
    // Build the option list
    // =========================================================================

    private void BuildOptionList()
    {
        _options = new List<IOptionBehaviour>
        {
            new BorderStyleOption(this),
            new BoxStyleOption(this),
            new BorderColorOption(this),
            new BoxColorOption(this),
            new BoxTransparencyOption(this),
            new GradientOption(this),
            new TextColorOption(this),
            new TextShadowColorOption(this),
            new TextShadowTransparencyOption(this),
            new FontOption(this),
            new MusicVolumeOption(this),
            new SfxVolumeOption(this),
            new DifficultyOption(this),
        };
    }

    // =========================================================================
    // Option implementations — each handles exactly one row
    // =========================================================================

    // ── Border style ─────────────────────────────────────────────────────────

    private class BorderStyleOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public BorderStyleOption(OptionsManager m) { _m = m; }

        public void OnFocus()   { }
        public void OnConfirm() { }
        public void OnCancel()  { }

        public void OnLeft()
        {
            UISettings.Instance.textBorderIndex = Mathf.Max(0, UISettings.Instance.textBorderIndex - 1);
            Refresh();
            ApplySettingsToScene();
        }
        public void OnRight()
        {
            int max = (UIAssetLibrary.Instance?.BorderCount ?? 1) - 1;
            UISettings.Instance.textBorderIndex = Mathf.Min(max, UISettings.Instance.textBorderIndex + 1);
            Refresh();
            ApplySettingsToScene();
        }
        public void Refresh()
        {
            if (_m.borderThumbnails == null) return;
            var lib = UIAssetLibrary.Instance;
            int sel = UISettings.Instance.textBorderIndex;
            for (int i = 0; i < _m.borderThumbnails.Length; i++)
            {
                if (_m.borderThumbnails[i] == null) continue;
                _m.borderThumbnails[i].sprite = lib?.GetBorder(i);
                var c = _m.borderThumbnails[i].color;
                _m.borderThumbnails[i].color = new Color(c.r, c.g, c.b, i == sel ? 1f : 0.4f);
            }
        }
    }

    // ── Box style ────────────────────────────────────────────────────────────

    private class BoxStyleOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public BoxStyleOption(OptionsManager m) { _m = m; }

        public void OnFocus()   { }
        public void OnConfirm() { }
        public void OnCancel()  { }

        public void OnLeft()
        {
            UISettings.Instance.textBoxIndex = Mathf.Max(0, UISettings.Instance.textBoxIndex - 1);
            Refresh(); ApplySettingsToScene();
        }
        public void OnRight()
        {
            int max = (UIAssetLibrary.Instance?.BoxCount ?? 1) - 1;
            UISettings.Instance.textBoxIndex = Mathf.Min(max, UISettings.Instance.textBoxIndex + 1);
            Refresh(); ApplySettingsToScene();
        }
        public void Refresh()
        {
            if (_m.boxThumbnails == null) return;
            var lib = UIAssetLibrary.Instance;
            int sel = UISettings.Instance.textBoxIndex;
            for (int i = 0; i < _m.boxThumbnails.Length; i++)
            {
                if (_m.boxThumbnails[i] == null) continue;
                _m.boxThumbnails[i].sprite = lib?.GetBox(i);
                var c = _m.boxThumbnails[i].color;
                _m.boxThumbnails[i].color = new Color(c.r, c.g, c.b, i == sel ? 1f : 0.4f);
            }
        }
    }

    // ── Border colour ─────────────────────────────────────────────────────────

    private class BorderColorOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        private char _rgb = 'r';   // 'r' | 'g' | 'b'
        private bool _active;

        public BorderColorOption(OptionsManager m) { _m = m; }

        public void OnFocus() { _active = false; SetSlidersInteractable(false); }
        public void OnConfirm() { _active = true; SetRgb('r'); }
        public void OnCancel()  { _active = false; SetSlidersInteractable(false); }

        public void OnLeft()
        {
            if (!_active) return;
            AdjustChannel(-0.05f);
        }
        public void OnRight()
        {
            if (!_active) return;
            AdjustChannel(0.05f);
        }

        private void AdjustChannel(float delta)
        {
            var s = UISettings.Instance;
            Color c = s.textBorderColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r + delta);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g + delta);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b + delta);
            s.textBorderColor = c;
            Refresh(); ApplySettingsToScene();
        }

        private void SetRgb(char ch)
        {
            _rgb = ch;
            SetSlidersInteractable(true);
            Refresh();
        }

        private void SetSlidersInteractable(bool v)
        {
            if (_m.borderColorR) _m.borderColorR.interactable = v && _rgb == 'r';
            if (_m.borderColorG) _m.borderColorG.interactable = v && _rgb == 'g';
            if (_m.borderColorB) _m.borderColorB.interactable = v && _rgb == 'b';
        }

        public void Refresh()
        {
            var c = UISettings.Instance.textBorderColor;
            if (_m.borderColorR) _m.borderColorR.SetValueWithoutNotify(c.r);
            if (_m.borderColorG) _m.borderColorG.SetValueWithoutNotify(c.g);
            if (_m.borderColorB) _m.borderColorB.SetValueWithoutNotify(c.b);
        }
    }

    // ── Box colour ────────────────────────────────────────────────────────────

    private class BoxColorOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        private char _rgb = 'r';
        private bool _active;

        public BoxColorOption(OptionsManager m) { _m = m; }

        public void OnFocus()   { _active = false; }
        public void OnConfirm() { _active = true; }
        public void OnCancel()  { _active = false; }

        public void OnLeft()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textBoxColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r - 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g - 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b - 0.05f);
            s.textBoxColor = c; Refresh(); ApplySettingsToScene();
        }
        public void OnRight()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textBoxColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r + 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g + 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b + 0.05f);
            s.textBoxColor = c; Refresh(); ApplySettingsToScene();
        }
        public void Refresh()
        {
            var c = UISettings.Instance.textBoxColor;
            if (_m.boxColorR) _m.boxColorR.SetValueWithoutNotify(c.r);
            if (_m.boxColorG) _m.boxColorG.SetValueWithoutNotify(c.g);
            if (_m.boxColorB) _m.boxColorB.SetValueWithoutNotify(c.b);
        }
    }

    // ── Box transparency ──────────────────────────────────────────────────────

    private class BoxTransparencyOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public BoxTransparencyOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { if (_m.boxTransparencySlider) _m.boxTransparencySlider.interactable = true; }
        public void OnCancel()  { if (_m.boxTransparencySlider) _m.boxTransparencySlider.interactable = false; }
        public void OnConfirm() { }
        public void OnLeft()
        { UISettings.Instance.textBoxTransparency = Mathf.Clamp01(UISettings.Instance.textBoxTransparency - 0.05f); Refresh(); ApplySettingsToScene(); }
        public void OnRight()
        { UISettings.Instance.textBoxTransparency = Mathf.Clamp01(UISettings.Instance.textBoxTransparency + 0.05f); Refresh(); ApplySettingsToScene(); }
        public void Refresh()
        { if (_m.boxTransparencySlider) _m.boxTransparencySlider.SetValueWithoutNotify(UISettings.Instance.textBoxTransparency); }
    }

    // ── Gradient ─────────────────────────────────────────────────────────────

    private class GradientOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public GradientOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { }
        public void OnConfirm() { UISettings.Instance.gradientEnabled = !UISettings.Instance.gradientEnabled; Refresh(); ApplySettingsToScene(); }
        public void OnCancel()  { }
        public void OnLeft()    { UISettings.Instance.gradientTransparency = Mathf.Clamp01(UISettings.Instance.gradientTransparency - 0.05f); Refresh(); ApplySettingsToScene(); }
        public void OnRight()   { UISettings.Instance.gradientTransparency = Mathf.Clamp01(UISettings.Instance.gradientTransparency + 0.05f); Refresh(); ApplySettingsToScene(); }
        public void Refresh()
        {
            if (_m.gradientTransparencySlider) _m.gradientTransparencySlider.SetValueWithoutNotify(UISettings.Instance.gradientTransparency);
            if (_m.gradientToggle) _m.gradientToggle.SetIsOnWithoutNotify(UISettings.Instance.gradientEnabled);
        }
    }

    // ── Text colour ───────────────────────────────────────────────────────────

    private class TextColorOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        private char _rgb = 'r';
        private bool _active;
        public TextColorOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { _active = false; }
        public void OnConfirm() { _active = true; }
        public void OnCancel()  { _active = false; }
        public void OnLeft()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r - 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g - 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b - 0.05f);
            s.textColor = c; Refresh(); ApplySettingsToScene();
        }
        public void OnRight()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r + 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g + 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b + 0.05f);
            s.textColor = c; Refresh(); ApplySettingsToScene();
        }
        public void Refresh()
        {
            var c = UISettings.Instance.textColor;
            if (_m.textColorR) _m.textColorR.SetValueWithoutNotify(c.r);
            if (_m.textColorG) _m.textColorG.SetValueWithoutNotify(c.g);
            if (_m.textColorB) _m.textColorB.SetValueWithoutNotify(c.b);
        }
    }

    // ── Text shadow colour ────────────────────────────────────────────────────

    private class TextShadowColorOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        private char _rgb = 'r';
        private bool _active;
        public TextShadowColorOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { _active = false; }
        public void OnConfirm() { _active = true; }
        public void OnCancel()  { _active = false; }
        public void OnLeft()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textShadowColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r - 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g - 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b - 0.05f);
            s.textShadowColor = c; Refresh(); ApplySettingsToScene();
        }
        public void OnRight()
        {
            if (!_active) return;
            var s = UISettings.Instance; Color c = s.textShadowColor;
            if (_rgb == 'r') c.r = Mathf.Clamp01(c.r + 0.05f);
            if (_rgb == 'g') c.g = Mathf.Clamp01(c.g + 0.05f);
            if (_rgb == 'b') c.b = Mathf.Clamp01(c.b + 0.05f);
            s.textShadowColor = c; Refresh(); ApplySettingsToScene();
        }
        public void Refresh()
        {
            var c = UISettings.Instance.textShadowColor;
            if (_m.shadowColorR) _m.shadowColorR.SetValueWithoutNotify(c.r);
            if (_m.shadowColorG) _m.shadowColorG.SetValueWithoutNotify(c.g);
            if (_m.shadowColorB) _m.shadowColorB.SetValueWithoutNotify(c.b);
        }
    }

    // ── Text shadow transparency ──────────────────────────────────────────────

    private class TextShadowTransparencyOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public TextShadowTransparencyOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { if (_m.shadowTransparencySlider) _m.shadowTransparencySlider.interactable = true; }
        public void OnCancel()  { if (_m.shadowTransparencySlider) _m.shadowTransparencySlider.interactable = false; }
        public void OnConfirm() { }
        public void OnLeft()    { UISettings.Instance.textShadowTransparency = Mathf.Clamp01(UISettings.Instance.textShadowTransparency - 0.05f); Refresh(); ApplySettingsToScene(); }
        public void OnRight()   { UISettings.Instance.textShadowTransparency = Mathf.Clamp01(UISettings.Instance.textShadowTransparency + 0.05f); Refresh(); ApplySettingsToScene(); }
        public void Refresh()   { if (_m.shadowTransparencySlider) _m.shadowTransparencySlider.SetValueWithoutNotify(UISettings.Instance.textShadowTransparency); }
    }

    // ── Font style ────────────────────────────────────────────────────────────

    private class FontOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public FontOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { }
        public void OnConfirm() { }
        public void OnCancel()  { }
        public void OnLeft()    { UISettings.Instance.fontIndex = Mathf.Max(0, UISettings.Instance.fontIndex - 1); Refresh(); ApplySettingsToScene(); }
        public void OnRight()   { int max = (UIAssetLibrary.Instance?.FontCount ?? 1) - 1; UISettings.Instance.fontIndex = Mathf.Min(max, UISettings.Instance.fontIndex + 1); Refresh(); ApplySettingsToScene(); }
        public void Refresh()
        {
            if (_m.fontLabels == null) return;
            int sel = UISettings.Instance.fontIndex;
            for (int i = 0; i < _m.fontLabels.Length; i++)
            {
                if (_m.fontLabels[i] == null) continue;
                _m.fontLabels[i].font  = UIAssetLibrary.Instance?.GetFont(i);
                _m.fontLabels[i].color = new Color(1f, 1f, 1f, i == sel ? 1f : 0.4f);
            }
        }
    }

    // ── Music volume ──────────────────────────────────────────────────────────

    private class MusicVolumeOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public MusicVolumeOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { if (_m.musicVolumeSlider) _m.musicVolumeSlider.interactable = true; }
        public void OnCancel()  { if (_m.musicVolumeSlider) _m.musicVolumeSlider.interactable = false; }
        public void OnConfirm() { }
        public void OnLeft()    { UISettings.Instance.musicVolume = Mathf.Clamp01(UISettings.Instance.musicVolume - 0.05f); Refresh(); ApplyAudio(); }
        public void OnRight()   { UISettings.Instance.musicVolume = Mathf.Clamp01(UISettings.Instance.musicVolume + 0.05f); Refresh(); ApplyAudio(); }
        public void Refresh()   { if (_m.musicVolumeSlider) _m.musicVolumeSlider.SetValueWithoutNotify(UISettings.Instance.musicVolume); }
        private void ApplyAudio() { /* Find your AudioSource and set .volume = UISettings.Instance.musicVolume */ }
    }

    // ── SFX volume ────────────────────────────────────────────────────────────

    private class SfxVolumeOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public SfxVolumeOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { if (_m.sfxVolumeSlider) _m.sfxVolumeSlider.interactable = true; }
        public void OnCancel()  { if (_m.sfxVolumeSlider) _m.sfxVolumeSlider.interactable = false; }
        public void OnConfirm() { }
        public void OnLeft()    { UISettings.Instance.sfxVolume = Mathf.Clamp01(UISettings.Instance.sfxVolume - 0.05f); Refresh(); }
        public void OnRight()   { UISettings.Instance.sfxVolume = Mathf.Clamp01(UISettings.Instance.sfxVolume + 0.05f); Refresh(); }
        public void Refresh()   { if (_m.sfxVolumeSlider) _m.sfxVolumeSlider.SetValueWithoutNotify(UISettings.Instance.sfxVolume); }
    }

    // ── Difficulty ────────────────────────────────────────────────────────────

    private class DifficultyOption : IOptionBehaviour
    {
        private readonly OptionsManager _m;
        public DifficultyOption(OptionsManager m) { _m = m; }
        public void OnFocus()   { }
        public void OnConfirm() { }
        public void OnCancel()  { }
        public void OnLeft()    { UISettings.Instance.difficulty = Mathf.Max(1, UISettings.Instance.difficulty - 1); Refresh(); }
        public void OnRight()   { UISettings.Instance.difficulty = Mathf.Min(5, UISettings.Instance.difficulty + 1); Refresh(); }
        public void Refresh()
        {
            if (_m.difficultyLabels == null) return;
            int sel = UISettings.Instance.difficulty;
            for (int i = 0; i < _m.difficultyLabels.Length; i++)
            {
                if (_m.difficultyLabels[i] == null) continue;
                _m.difficultyLabels[i].color = new Color(1f, 1f, 1f, (i + 1) == sel ? 1f : 0.4f);
            }
        }
    }
}
