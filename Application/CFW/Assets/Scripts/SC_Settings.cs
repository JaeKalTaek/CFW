using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Settings : MonoBehaviour {

	public InputField healthI, bodyPartsI, matchHeatI;

	void OnEnable() {

		for (int i = 0; i < 3; i++)
			ResetField (i);

	}

	public void CheckValue(int i) {		

		if (GetInputField (i).text != "") {

			int value;

			if (!int.TryParse (GetInputField (i).text, out value) || (value <= 0) || ((i == 2) && (value > 20)))
				GetInputField (i).text = GetValue (i).ToString ();

		}

	}

	public void SetValue(int i) {

		if (GetInputField (i).text != "") {

			int v;

			int.TryParse (GetInputField (i).text, out v);

			if (i == 0) {

				PlayerPrefs.SetInt ("Starting_Health", v);
				SC_Values.health = v;

			} else if (i == 1) {

				PlayerPrefs.SetInt ("Starting_Body_Parts_Health", v);
				SC_Values.bodyPartsH = v;

			} else {

				PlayerPrefs.SetInt ("Starting_Match_Heat", v);
				SC_Values.matchHeat = v;

			}

			ResetField (i);

		}

	}

	void ResetField(int i) {

		GetInputField (i).text = "";

		GetInputField (i).placeholder.GetComponent<Text> ().text = GetValue (i).ToString ();

	}

	InputField GetInputField(int i) {

		return (i == 0) ? healthI : ((i == 1) ? bodyPartsI : matchHeatI);

	}

	int GetValue(int i) {

		return (i == 0) ? SC_Values.health : ((i == 1) ? SC_Values.bodyPartsH : SC_Values.matchHeat);

	}
		
}
