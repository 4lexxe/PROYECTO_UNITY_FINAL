using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.2f;
    public float lookAheadDistance = 1.0f;
    public Vector2 offset = Vector2.zero;

    private float _lastTargetX;
    private float _lookAheadX;
    private Vector3 _velocity;

    void Start()
    {
        if (target != null) _lastTargetX = target.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x;
        float deltaX = targetX - _lastTargetX;
        _lastTargetX = targetX;

        if (Mathf.Abs(deltaX) > 0.001f)
        {
            _lookAheadX = Mathf.Sign(deltaX) * lookAheadDistance;
        }
        else
        {
            _lookAheadX = Mathf.Lerp(_lookAheadX, 0f, Time.deltaTime * 3f);
        }

        Vector3 desired = new Vector3(targetX + _lookAheadX + offset.x, transform.position.y + offset.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
    }
}