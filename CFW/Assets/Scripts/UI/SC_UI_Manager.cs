using System;
using TMPro;
using UnityEngine;
using static SC_Player;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static SC_UI_Manager Instance;

    public Transform localValues, otherValues;

    public GameObject bodyPartDamageChoicePanel, assessPanel;

    public GameObject basicsPanel, skipButton;

    public TextMeshProUGUI endText;

    void Awake () {

        Instance = this;

    }

    public void SetValue (bool local, string id, int value) {

        (local ? localValues : otherValues).Find (id).GetChild (1).GetComponent<TextMeshProUGUI> ().text = value.ToString ();

    }

    public void SkipTurn () {

        skipButton.SetActive (false);

        basicsPanel.SetActive (true);

    }

    public void ChoseBodyPart (int bodyPart) {

        bodyPartDamageChoicePanel.SetActive (false);

        localPlayer.UseCardServerRpc (GM.UsingCardID, bodyPart);        

    }

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }

}
