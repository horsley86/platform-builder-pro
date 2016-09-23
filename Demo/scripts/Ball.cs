using UnityEngine;

public class Ball : MonoBehaviour {

    public float force;

    private Rigidbody _rigidbody;
    private bool _isGrounded;

	// Use this for initialization
	void Start () {
        _rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!_isGrounded) return;
        var vectorForce = Input.GetAxis("Vertical") * Vector3.forward;
        vectorForce += Input.GetAxis("Horizontal") * Vector3.right;
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