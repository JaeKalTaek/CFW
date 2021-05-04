using Card;
using System;
using TMPro;
using UnityEngine;
using static SC_Player;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static SC_UI_Manager Instance;

    public Transform localValues, otherValues;

    public GameObject bodyPartDamageChoicePanel, messagePanel;

    public GameObject pinfallPanel;

    public TextMeshProUGUI endText;

    public GameObject basicsPanel, showBasicsButton, showLockedBasicsButton, hideBasicsButton, hideLockedBasicsButton, maintainSubmissionButton;           

    void Awake () {

        Instance = this;

    }

    public void SetValue (bool local, string id, int value) {

        (local ? localValues : otherValues).Find (id).GetChild (1).GetComponent<TextMeshProUGUI> ().text = value.ToString ();

    }

    public void ShowMessage (string id) {

        SC_Global.Messages.TryGetValue (id, out string m);

        messagePanel.GetComponentInChildren<TextMeshProUGUI> ().text = m;

        messagePanel.SetActive (true);

    }

    public void ShowBasics (bool show) {

        (NoLock ? showBasicsButton : showLockedBasicsButton).SetActive (!show);

        if (show)
            for (int i = 0; i < 10; i++)
                basicsPanel.transform.GetChild (i).gameObject.SetActive (otherPlayer.Unlocked ? (localPlayer.Unlocked ? (0 <= i && i < 3) : (localPlayer.Pinned ? (4 <= i && i < 6) : (6 <= i && i < 9))) : i == 9);        

        basicsPanel.SetActive (show);

        (NoLock ? hideBasicsButton : hideLockedBasicsButton).SetActive (show);

        GM.localHand.gameObject.SetActive (!show);

    }

    public void MaintainSubmission () {

        maintainSubmissionButton.SetActive (false);

        localPlayer.MaintainSubmissionServerRpc ();

    }

    public void Pinfall (bool yes) {

        pinfallPanel.SetActive (false);

        if (yes)
            localPlayer.UseBasicCardServerRpc (3);
        else
            localPlayer.NextTurn ();

    }

    public void ShowBodyPartPanel (bool damage = true) {

        ShowMessage ("BodyPart" + (damage ? "Damage" : "Heal"));

        bodyPartDamageChoicePanel.SetActive (true);

    }

    public void ChoseBodyPart (int bodyPart) {

        messagePanel.SetActive (false);

        bodyPartDamageChoicePanel.SetActive (false);

        localPlayer.UseCardServerRpc (SC_BaseCard.activeCard.UICard.name, bodyPart);        

    }

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }

}
