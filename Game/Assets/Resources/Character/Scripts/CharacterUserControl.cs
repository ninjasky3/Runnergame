using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Character))]
public class CharacterUserControl : MonoBehaviour {

	public bool walkByDefault = false; 

	private Character character;   
	private Vector3 move;          
	
	void Start () {
		character = GetComponent<Character>();
	}
	
	void FixedUpdate () {
		bool crouch = Input.GetKey(KeyCode.C);
		bool jump = Input.GetButton("Jump");
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		
		move = v * Vector3.forward + h * Vector3.right;
		
		if (move.magnitude > 1) move.Normalize();
		
		bool walkToggle = Input.GetKey(KeyCode.LeftShift);
		float walkMultiplier = (walkByDefault ? walkToggle ? 1 : 0.5f : walkToggle ? 0.5f : 1);
		move *= walkMultiplier;

		character.Move( move, crouch, jump );
	}
}
