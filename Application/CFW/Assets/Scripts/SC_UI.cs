using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_UI : MonoBehaviour {

	public GameObject startMenu, settingsMenu, gameMenu;

	public Transform player1, player2;
    
	public void ShowMenu(int menu) {

        ResetGame();

        startMenu.SetActive(menu == 0);

        settingsMenu.SetActive(menu == 1);
            
        gameMenu.SetActive(menu == 2);

    }

	public void ResetGame() {

		ResetPlayer (player1);
		ResetPlayer (player2);

		foreach (Text t in gameMenu.transform.Find("Match_Heat").GetComponentsInChildren<Text>())
			t.text = SC_Values.matchHeat.ToString();

	}

	void ResetPlayer(Transform p) {

		foreach (Transform t in p) {

			string n = t.name;

			if(n != "Rest_Button")
				t.GetComponentInChildren<Text> ().text = (n == "Health" ? SC_Values.health.ToString() : (n == "Stamina" ? "10" : (n == "Alignment" ? "0" : SC_Values.bodyPartsH.ToString())));

		}

	}

}
