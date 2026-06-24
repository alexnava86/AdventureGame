// =============================================================================
// PauseMenu.cs   —   Assets/Scripts/UI/
//
// A full-stop pause menu opened with the Pause action (Character map) and
// closed with the Unpause action (UI map) — both already defined in your
// PlayerBaseInput asset. Freezes gameplay via Time.timeScale = 0.
//
// Menu options: Options (opens the OptionsManager screen) and Quit.
// No Save option — saving happens only at in-world save points.
//
// SETUP:
//   • Put this on a pause-menu Canvas (start it INACTIVE in the hierarchy, or
//     it will hide itself on Awake).
//   • Assign the Options panel (your OptionsManager canvas) and the buttons.
//   • The menu switches the active input map to "UI" while open so gameplay
//     input (movement/jump/attack) is ignored, then back to "Character" on close.
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Root object of the pause menu UI (the buttons panel).")]
    public GameObject pauseRoot;
    [Tooltip("The OptionsManager canvas/panel. Opened by the Options button.")]
    public GameObject optionsPanel;

    [Header("Buttons")]
    public Button optionsButton;
    public Button quitButton;

    [Header("Quit Target")]
    [Tooltip("Scene to load on Quit (e.g. your start/title screen). " +
             "Leave empty to quit the application instead.")]
    public string quitToSceneName = "";

    // ── State ─────────────────────────────────────────────────────────────────
    public static bool IsPaused { get; private set; }

    private PlayerBaseInput _input;

    private void Awake()
    {
        _input = new PlayerBaseInput();

        // Listen for Pause (gameplay) and Unpause (menu) regardless of which map
        // is active, by subscribing to the actions directly.
        _input.Character.Pause.performed   += _ => TogglePause();
        _input.UI.Unpause.performed        += _ => TogglePause();

        _input.Enable();

        if (pauseRoot     != null) pauseRoot.SetActive(false);
        if (optionsPanel  != null) optionsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        optionsButton?.onClick.AddListener(OpenOptions);
        quitButton?.onClick.AddListener(Quit);
    }

    private void OnDisable()
    {
        optionsButton?.onClick.RemoveListener(OpenOptions);
        quitButton?.onClick.RemoveListener(Quit);
    }

    private void OnDestroy()
    {
        _input?.Dispose();
    }

    // =========================================================================
    // Pause toggle
    // =========================================================================

    public void TogglePause()
    {
        // If the options panel is open, the first "back" press closes IT,
        // returning to the pause menu rather than unpausing entirely.
        if (IsPaused && optionsPanel != null && optionsPanel.activeSelf)
        {
            CloseOptions();
            return;
        }

        if (IsPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;                       // freeze all gameplay
        if (pauseRoot != null) pauseRoot.SetActive(true);

        // Switch input to the UI map so movement/jump/attack are ignored.
        _input.Character.Disable();
        _input.UI.Enable();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;                       // unfreeze
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pauseRoot    != null) pauseRoot.SetActive(false);

        _input.UI.Disable();
        _input.Character.Enable();
    }

    // =========================================================================
    // Options
    // =========================================================================

    private void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (pauseRoot    != null) pauseRoot.SetActive(false);
    }

    private void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pauseRoot    != null) pauseRoot.SetActive(true);

        // Persist any UI settings the player changed while in options.
        UISettings.Instance.Save();
        OptionsManager.ApplySettingsToScene();
    }

    // =========================================================================
    // Quit
    // =========================================================================

    private void Quit()
    {
        // Always restore time scale before leaving so the next scene runs normally.
        Time.timeScale = 1f;
        IsPaused = false;

        if (!string.IsNullOrEmpty(quitToSceneName))
            SceneManager.LoadScene(quitToSceneName);
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
