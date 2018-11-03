using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Change_Value : MonoBehaviour {

	public void ChangeMatchHeat(int amount) {

		Text[] associatedTexts = transform.parent.GetComponentsInChildren<Text> ();

		int currentAmount;

		int.TryParse (associatedTexts[0].text, out currentAmount);

		int newAmount = currentAmount + (amount * (name.Equals ("Plus") ? 1 : -1));

		newAmount = (newAmount > 20) ? 11 : Mathf.Max (newAmount, 1);

		foreach (Text t in associatedTexts)
			t.text = newAmount.ToString ();

	}

	public void ChangeStat(int amount) {

		ChangeStat (transform.parent, amount, name == "Plus");

	}

	public void Rest(bool player1) {

		foreach (Transform t in (player1 ? FindObjectOfType<SC_UI>().player1 : FindObjectOfType<SC_UI>().player2))
			if((t.name != "Stamina") && (t.name != "Rest_Button") && (t.name != "Alignment"))
				ChangeStat (t, (t.name.Equals ("Health") ? 2 : 1), true);

	}

	void ChangeStat(Transform stat, int amount, bool plus) {

		Text associatedText = stat.GetComponentInChildren<Text> ();

		int currentAmount;

		int.TryParse (associatedText.text, out currentAmount);

		int newAmount = currentAmount + (amount * (plus ? 1 : -1));

		bool align = stat.name == "Alignment";

		bool stamina = stat.name == "Stamina";

		bool health = stat.name == "Health";

		newAmount = Mathf.Clamp (newAmount, align ? -5 : 0, align ? 5 : (stamina ? 10 : (health ? SC_Values.health : SC_Values.bodyPartsH)));

		associatedText.text = newAmount.ToString();

	}

}
