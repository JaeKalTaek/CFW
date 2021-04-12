using System;
using TMPro;
using UnityEngine;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_Player Player { get { return SC_Player.localPlayer; } }

    public static SC_UI_Manager Instance;

    public BodyPartDamageChoice bodyPartDamageChoice;

    public GameObject skipButton;

    [Serializable]
	public struct BodyPartDamageChoice {

        public GameObject panel;

        public TextMeshProUGUI firstChoice, secondChoice;

    }

    void Start () {

        Instance = this;

    }

    public void SkipTurn () {

        Player.Turn = false;

        Player.CanPlay = false;

        skipButton.SetActive(false);

        Player.SkipTurnServerRpc ();

    }

    public void ChoseBodyPart (bool first) {

        Player.UseOffensiveMoveCardServerRpc (GM.UsingCardID, first);

    }

}
