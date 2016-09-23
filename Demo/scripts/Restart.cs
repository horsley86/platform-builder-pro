using UnityEngine;
using System.Collections;

public class Restart : MonoBehaviour {

	void OnTriggerEnter(Collider collider)
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}