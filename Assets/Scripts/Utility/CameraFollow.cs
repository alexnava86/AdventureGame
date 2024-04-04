using UnityEngine;
using System.Collections;
using Prime31;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 cameraOffset;
    public float smoothDampTime = 0.2f;
    public bool useFixedUpdate = false;

    private CharacterController2D playerController;
    private Vector3 smoothDampVelocity;
    private Bounds cameraBounds;
    private Vector2 minCameraPosition;
    private Vector2 maxCameraPosition;

    void Awake()
    {
        playerController = target.GetComponent<CharacterController2D>();
    }

    void Start()
    {
        CalculateCameraBounds();
    }

    void LateUpdate()
    {
        if (!useFixedUpdate)
            UpdateCameraPosition();
        this.transform.position = new Vector3(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y), Mathf.Round(this.transform.position.z));
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
            UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (MapManager.Instance == null)
            return;

        var targetPosition = GetTargetPosition();

        // Clamp camera within the bounds of the 2D map
        var cameraPosition = new Vector3(
            Mathf.Clamp(targetPosition.x, minCameraPosition.x, maxCameraPosition.x),
            Mathf.Clamp(targetPosition.y, minCameraPosition.y, maxCameraPosition.y),
            this.transform.position.z
        );

        // Smoothly move the camera
        this.transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref smoothDampVelocity, smoothDampTime);
    }

    Vector3 GetTargetPosition()
    {
        if (playerController == null)
            return target.position - cameraOffset;

        //if (_playerController.rb.velocity.x > 0)
            //return target.position - cameraOffset;

        var leftOffset = cameraOffset;
        leftOffset.x *= -1;
        return target.position - leftOffset;
    }

    void CalculateCameraBounds()
    {
        if (Camera.main != null)
        {
            cameraBounds = Camera.main.OrthographicBounds();

            // Calculate the minimum and maximum x positions for the camera to stay within the visible map area
            float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float minX = cameraHalfWidth;
            float maxX = MapManager.Instance.MaxWidth * 16 - cameraHalfWidth;

            // Calculate the minimum and maximum y positions for the camera to stay within the visible map area
            float cameraHalfHeight = Camera.main.orthographicSize;
            float minY = MapManager.Instance.MaxHeight * -16 - cameraHalfHeight;
            float maxY = 0;

            // Set the camera bounds
            minCameraPosition = new Vector2(minX, minY);
            maxCameraPosition = new Vector2(maxX, maxY);
        }
    }
}