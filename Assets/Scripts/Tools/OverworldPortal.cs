using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldPortal : MonoBehaviour
{
    public string Destination;
    public int PortalID;
    public string Direction;
    public delegate void PortalAction<T>(T portalIDNo);
    public static event PortalAction<int> OnPortalEnter;

    void Awake()
    {
        //DontDestroyOnLoad(this);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterOverworldController>() != null)
        {
            GameManager.Instance.Destination = this.Destination; //Temporary until data permanence or other solution implemented...
            GameManager.Instance.PortalID = this.PortalID; //Temporary until data permanence or other solution implemented...
            GameManager.Instance.Direction = this.Direction; //Temporary until data permanence or other solution implemented...
            SceneManager.LoadScene(Destination.Trim('"')); 
        }
        /*
        try
        {
            
            if (OnPortalEnter != null)
            {
                OnPortalEnter(PortalID);
            }
            //SceneManager.LoadScene(Destination.Trim('"')); 
            //Destroy(this);
        }
        catch
        {

        }
        */
    }
    /*
    void OnEnable()
    {
        Portal.OnPortalEnter += WarpPlayerToPortal;
    }
    private void OnDisable()
    {
        Portal.OnPortalEnter -= WarpPlayerToPortal;
    }
    private void WarpPlayerToPortal(int portalID)
    {
        Debug.Log("Portal ID: " + portalID);
    }
    */
}