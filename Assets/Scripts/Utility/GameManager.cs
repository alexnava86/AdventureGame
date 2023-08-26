using UnityEngine;
using UnityEngine.SceneManagement;
//using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Variables
    private static GameObject dialogue;
    private static bool dialogueSkip;
    private static bool dialogueOverflow;
    private static bool dialogueComplete;
    #endregion

    #region Properties
    public bool Paused { get; set; }
    public bool DevMode { get; set; }
    public bool DebugMode { get; set; }
    public string Destination { get; set; } //Temporary until data permanence or other solution implemented...
    public int PortalID { get; set; } //Temporary until data permanence or other solution implemented...
    public string Direction { get; set; } //Temporary until data permanence or other solution implemented...
    public static GameManager Instance { get; private set; }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void OnEnable()
    {
        //OverworldPortal.OnPortalEnter += 
    }
    private void Start()
    {
        
    }
    private void Update()
    {

    }
    #endregion

    #region Methods
    public void StartNewGame()
    {

    }
    public void ContinueGame()
    {

    }
    public void SaveGame(int saveslot)
    {

    }
    public void LoadGame(int saveslot)
    {

    }
    public void PauseGame()
    {
        //Time.timeScale = 0;
    }
    public void UnpauseGame()
    {
        //Time.timeScale = 1;
    }
    public void OpenMenu()
    {

    }
    public void OpenDebugMenu()
    {

    }
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Coroutines
    #endregion
}