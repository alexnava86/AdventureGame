using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ParallaxLayer : MonoBehaviour
{
    private Vector3 startPos;
    private float length;
    public float layerXOffset;
    public float layerYOffset;
    public float parallaxXFactor;
    public float parallaxYFactor;
    //public float PixelsPerUnit = 1;
    public GameObject cam;

    void Start()
    {
        startPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);// + layerXOffset, this.transform.position.y + layerYOffset, this.transform.position.z);
        length = GetComponent<TilemapRenderer>().bounds.size.x;
        //length = GetComponent<SpriteRenderer>().bounds.size.x;
        //Debug.Log(length);
    }

    void Update()
    {
        ///*
        //float tempX = cam.transform.position.x * (1 - parallaxXFactor);
        float distanceX = cam.transform.position.x * (1 - parallaxXFactor) + layerXOffset; //- (Camera.main.pixelWidth / 2) * parallaxXFactor;
        float distanceY = cam.transform.position.y * (1 - parallaxYFactor) + -layerYOffset;

        Vector3 newPosition = new Vector3(startPos.x + distanceX, startPos.y + distanceY, this.transform.position.z);

        transform.position = PixelPerfectClamp(newPosition, 1); //PixelsPerUnit);

        //if (tempX > startPos.x + (length / 2)) startPos.x += length;
        //else if (tempX < startPos.x - (length / 2)) startPos.x -= length;
        //*/

        //if(parallaxXFactor > 1f)
        {
            //Instantiate an object to the right of the current object
            //Vector3 thePosition = transform.TransformPoint(2, 0, 0);
        }
    }

    private Vector3 PixelPerfectClamp(Vector3 locationVector, float pixelsPerUnit)
    {
        Vector3 vectorInPixels = new Vector3(Mathf.CeilToInt(locationVector.x * pixelsPerUnit), Mathf.CeilToInt(locationVector.y * pixelsPerUnit), Mathf.CeilToInt(locationVector.z * pixelsPerUnit));
        return vectorInPixels / pixelsPerUnit;
    }
}