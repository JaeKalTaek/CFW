﻿using System;
using TMPro;
using UnityEngine;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static SC_UI_Manager Instance;

    public BodyPartDamageChoice bodyPartDamageChoice;

    public GameObject skipButton;

    public TextMeshProUGUI endText;

    [Serializable]
	public struct BodyPartDamageChoice {

        public GameObject panel;

        public TextMeshProUGUI firstChoice, secondChoice;

    }

    void Awake () {

        Instance = this;

    }    

    public void ChoseBodyPart (bool first) {

        SC_Player.localPlayer.UseCardServerRpc (GM.UsingCardID, first);

        bodyPartDamageChoice.panel.SetActive (false);

    }

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }

}
