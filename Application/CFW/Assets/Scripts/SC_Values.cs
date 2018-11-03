using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Values : MonoBehaviour {

	public static int health, bodyPartsH, matchHeat;

	void Awake() {

		if (PlayerPrefs.HasKey ("Starting_Health"))
			health = PlayerPrefs.GetInt ("Starting_Health");
		else {
			PlayerPrefs.SetInt ("Starting_Health", 30);
			health = 30;
		}

		if (PlayerPrefs.HasKey ("Starting_Body_Parts_Health"))
			bodyPartsH = PlayerPrefs.GetInt ("Starting_Body_Parts_Health");
		else {
			PlayerPrefs.SetInt ("Starting_Body_Parts_Health", 10);
			bodyPartsH = 10;
		}

		if (PlayerPrefs.HasKey ("Starting_Match_Heat"))
			matchHeat = PlayerPrefs.GetInt ("Starting_Match_Heat");
		else {
			PlayerPrefs.SetInt ("Starting_Match_Heat", 3);
			matchHeat = 3;
		}

	}

}
