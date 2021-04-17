using System;
using TMPro;
using UnityEngine;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

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

    public void ChoseBodyPart (bool first) {

        SC_Player.localPlayer.UseCardServerRpc (GM.UsingCardID, first);

        bodyPartDamageChoice.panel.SetActive (false);

    }

}
