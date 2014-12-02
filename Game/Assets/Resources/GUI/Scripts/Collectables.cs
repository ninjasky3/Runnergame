using UnityEngine;
using System.Collections;

public class Collectables : MonoBehaviour {

	GameObject[] pickUps;																	// array of all pickups in scene
	private static int totalAmountOfPickUps = 0;											// variable for total amount of pickups
	private static int currentAmountOfPickUps = 0;											// variable for current amount of taken pickups

	void Start () {
		pickUps = GameObject.FindGameObjectsWithTag("Item");								
		totalAmountOfPickUps = pickUps.Length ;
		currentAmountOfPickUps = totalAmountOfPickUps - pickUps.Length;
	}

	public void AddOneToCounter(){
		pickUps = GameObject.FindGameObjectsWithTag("Item");
		currentAmountOfPickUps = totalAmountOfPickUps - (pickUps.Length - 1);
	}
	void Update(){
		
		gameObject.guiText.text = totalAmountOfPickUps + " / " + currentAmountOfPickUps;
	}
}
