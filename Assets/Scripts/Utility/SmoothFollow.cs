using UnityEngine;
using System.Collections;
using Prime31;


public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public float smoothDampTime = 0.2f;
    //[HideInInspector]
    //public new Transform transform;
    //private Transform t;
    public Vector3 cameraOffset;
    public bool useFixedUpdate = false;

    private CharacterController2dOld _playerController;
    private Vector3 _smoothDampVelocity;

    void Awake()
    {
        //t = gameObject.transform;
        _playerController = target.GetComponent<CharacterController2dOld>();
        //Debug.Log(Camera.main.View);
        //Camera.main.OrthographicBounds();
        //Debug.Log(Camera.main.OrthographicBounds().center.x - Camera.main.OrthographicBounds().extents.x);
    }

    void LateUpdate()
    {
        if (!useFixedUpdate)
            updateCameraPosition();
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
            updateCameraPosition();
    }


    void updateCameraPosition()
    {
        if (MapManager.Instance != null)
        {
            if (Camera.main.OrthographicBounds().min.x >= 16)
            {
                //MapManager.ma
                if (_playerController == null)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, target.position - cameraOffset, ref _smoothDampVelocity, smoothDampTime);
                    return;
                }

                if (_playerController.velocity.x > 0)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, target.position - cameraOffset, ref _smoothDampVelocity, smoothDampTime);
                }
                else
                {
                    var leftOffset = cameraOffset;
                    leftOffset.x *= -1;
                    //this.transform.position = Vector3.SmoothDamp(transform.position, target.position - leftOffset, ref _smoothDampVelocity, smoothDampTime);
                    this.transform.position = Vector3.SmoothDamp(transform.position, target.position - leftOffset, ref _smoothDampVelocity, smoothDampTime);
                }
            }
            this.transform.position = new Vector2(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y));
        }
    }
}
