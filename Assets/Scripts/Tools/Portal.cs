using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string Destination;
    public int PortalID;
    public string Direction;
    public delegate void PortalAction<T>(T portalIDNo);
    public static event PortalAction<int> OnPortalEnter;
    public static event PortalAction<Vector3> OnPlayerSummon;

    void Awake()
    {
        //DontDestroyOnLoad(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        try
        {
            if (OnPortalEnter != null)
            {
                //OnPortalEnter(PortalID);
            }
            //SceneManager.LoadScene(Destination);
            //Destroy(this);
        }
        catch
        {

        }
    }

    private void Start()
    {
        //Debug.Log(GameManager.Instance.Destination);
        //Temporary until data permanence or other solution implemented...
        if (GameManager.Instance.Destination != "")
        {
            //Debug.Log("TEST_0");
            if (GameManager.Instance.PortalID == this.PortalID)
            {
                //Debug.Log(GameManager.Instance.Destination);
                //Debug.Log("TEST_1");
                //OnPlayerSummon(this.transform.position);
            }

            if (OnPlayerSummon != null && GameManager.Instance.PortalID == this.PortalID)
            {
                //Debug.Log(GameManager.Instance.Destination);
                //Debug.Log("TEST_2");
                OnPlayerSummon(this.transform.position);
            }
        }
    }

    void OnEnable()
    {
        //OverworldPortal.OnPortalEnter += WarpPlayerToPortal;
    }

    private void OnDisable()
    {
        //OverworldPortal.OnPortalEnter -= WarpPlayerToPortal;
    }

    private void WarpPlayerToPortal(int portalID)
    {
        Debug.Log("Portal ID: " + portalID);
        /*
        if (collision.CompareTag("Player"))
        {
            // Find the source portal by name in the source scene
            GameObject sourcePortal = GameObject.Find(sourcePortalName);

            if (sourcePortal != null)
            {
                // Calculate the offset between the player's current position and the source portal's position
                Vector3 offset = collision.transform.position - sourcePortal.transform.position;

                // Move the player to the destination portal's position with the calculated offset
                collision.transform.position = transform.position + offset;

                // Unload the destination scene (optional if desired)
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(sourcePortalName));
            }
        }
        */
    }
}