using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {
	
	float secondsInGame = 0;																// amount of seconds under 60
	float realTime = 0;																		// the real time
	int minutesInGame = 0;																	// amount of minutes

	void Start () {
	}

	void Update () {

		realTime  = Time.time;											
		secondsInGame = realTime - minutesInGame * 60;							

		if(secondsInGame >= 60)
			AddMinutes();												

		secondsInGame = Mathf.Round(secondsInGame * 100) / 100;						// making sure i get 1 digit after whole seconds
		gameObject.guiText.text = "Time: " + minutesInGame + ":" + secondsInGame;	// setting text on screen
	}

	void AddMinutes(){		// Set seconds to 0 and add a minute
		minutesInGame = Mathf.RoundToInt(realTime / 60);						// setting minutes
	}
}
