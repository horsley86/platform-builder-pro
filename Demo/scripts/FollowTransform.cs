using UnityEngine;

public class FollowTransform : MonoBehaviour {

    public Transform target;
    public Vector3 camOffset;
    public float distance;

    private Vector3 _lastTargetPosition;


    private void Start()
    {
        _lastTargetPosition = target.position;
    }

    private void UpdateCameraPosition()
    {
        var direction = (target.position - _lastTargetPosition).normalized;

        transform.position = Vector3.Lerp(transform.position, target.position - (direction * distance) + camOffset, 0.1f);
        transform.LookAt(target);

        _lastTargetPosition = target.position;
    }

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }
}