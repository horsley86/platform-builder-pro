using UnityEngine;

public class Ball : MonoBehaviour {

    public float force;
    public Transform cameraTransform;

    private Rigidbody _rigidbody;
    private bool _isGrounded;

	// Use this for initialization
	void Start () {
        _rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        //if (!_isGrounded) return;

        var direction = (transform.position - cameraTransform.position).normalized;
        var directionRight = Vector3.Cross(Vector3.up, direction).normalized;

        var vectorForce = Input.GetAxis("Horizontal") * directionRight;

        if (_isGrounded)
            vectorForce += Input.GetAxis("Vertical") * direction;

        _rigidbody.AddForce(vectorForce * force);
	}

    void OnTriggerEnter (Collider other)
    {
        _isGrounded = true;
    }

    void OnTriggerExit (Collider other)
    {
        _isGrounded = false;
    }
}