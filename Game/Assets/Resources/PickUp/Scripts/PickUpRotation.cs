using UnityEngine;
using System.Collections;

public class PickUpRotation : MonoBehaviour {

	Vector3 rotationSpeed;
	void Start () {
		rotationSpeed.x = Random.Range(80, 120);
		rotationSpeed.y = Random.Range(120, 160);
		rotationSpeed.z = Random.Range(60, 80);
	}

	void Update () {
		transform.Rotate(rotationSpeed * Time.deltaTime);
	}
}
