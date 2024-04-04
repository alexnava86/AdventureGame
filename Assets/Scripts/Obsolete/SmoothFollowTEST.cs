using UnityEngine;
using System.Collections;
using Prime31;

public class SmoothFollowTEST : MonoBehaviour
{
    public Transform target;
    public float smoothDampTime = 0.2f;
    public Vector3 cameraOffset;
    public bool useFixedUpdate = false;

    private CharacterController2D _playerController;
    private Vector3 _smoothDampVelocity;
    private Bounds _cameraBounds;
    private Vector2 _minCameraPosition;
    private Vector2 _maxCameraPosition;

    void Awake()
    {
        _playerController = target.GetComponent<CharacterController2D>();
    }

    void Start()
    {
        CalculateCameraBounds();
    }

    void LateUpdate()
    {
        if (!useFixedUpdate)
            UpdateCameraPosition();
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
            Mathf.Clamp(targetPosition.x, _minCameraPosition.x, _maxCameraPosition.x),
            Mathf.Clamp(targetPosition.y, _minCameraPosition.y, _maxCameraPosition.y),
            this.transform.position.z
        );

        // Smoothly move the camera
        this.transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref _smoothDampVelocity, smoothDampTime);
        Debug.Log("Camera Position Y=" + cameraPosition.y + " - Min Position Y=" + _minCameraPosition.y);
    }

    Vector3 GetTargetPosition()
    {
        if (_playerController == null)
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
            _cameraBounds = Camera.main.OrthographicBounds();

            // Calculate the minimum and maximum x positions for the camera to stay within the visible map area
            float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float minX = cameraHalfWidth;
            float maxX = MapManager.Instance.MaxWidth * 16 - cameraHalfWidth;

            // Calculate the minimum and maximum y positions for the camera to stay within the visible map area
            float cameraHalfHeight = Camera.main.orthographicSize;
            float minY = MapManager.Instance.MaxHeight * -16 - cameraHalfHeight;
            float maxY = 0;
            //Debug.Log(minY);

            // Set the camera bounds
            _minCameraPosition = new Vector2(minX, minY);
            _maxCameraPosition = new Vector2(maxX, maxY);
            //Debug.Log(_minCameraPosition.y);
        }
    }
}