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

    [Serializable]
    public struct HandshakeUI { public GameObject panel, heelChoice, faceChoice; }

    public HandshakeUI handshakeUI;

    [Serializable]
    public struct AlignmentChoiceUI { public GameObject panel; public TextMeshProUGUI heelChoice, faceChoice; }

    public AlignmentChoiceUI alignmentChoiceUI;

    public TMP_InputField knowYourOpponentChoice;

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

        (NoLock ? hideBasicsButton : hideLockedBasicsButton).SetActive (show);

        if (show)
            for (int i = 0; i < 10; i++)
                basicsPanel.transform.GetChild (i).gameObject.SetActive (otherPlayer.Unlocked ? (localPlayer.Unlocked ? (0 <= i && i < 3) : (localPlayer.Pinned ? (4 <= i && i < 6) : (6 <= i && i < 9))) : i == 9);        

        basicsPanel.SetActive (show);        

        GM.localHand.gameObject.SetActive (!show);

    }

    public void MaintainSubmission () {

        maintainSubmissionButton.SetActive (false);

        localPlayer.MaintainSubmissionServerRpc ();

    }

    public void Pinfall (bool yes) {

        pinfallPanel.SetActive (false);

        if (yes)
            localPlayer.StartUsingBasicServerRpc (3);
        else
            localPlayer.NextTurnServerRpc ();

    }

    public void HandshakeChoice (int choice) {

        handshakeUI.panel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("Handshake", choice);

    }

    #region Alignment choice
    public void ShowAlignmentChoice (int value) {

        alignmentChoiceUI.heelChoice.text = "Alignment - " + value;

        alignmentChoiceUI.faceChoice.text = "Alignment + " + value;

        alignmentChoiceUI.panel.SetActive (true);

    }

    public void AlignmentChoice (int value) {

        alignmentChoiceUI.panel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("AlignmentChoice", SC_BaseCard.activeCard.CurrentEffect.effectValue * value);

        SC_BaseCard.activeCard.MakingChoices = false;

    }
    #endregion

    #region BodyPart choice
    public void ShowBodyPartPanel (bool damage = true) {

        ShowMessage ("BodyPart" + (damage ? "Damage" : "Heal"));

        bodyPartDamageChoicePanel.SetActive (true);

    }

    public void ChoseBodyPart (int bodyPart) {

        messagePanel.SetActive (false);

        bodyPartDamageChoicePanel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("BodyPart", bodyPart);

        SC_BaseCard.activeCard.MakingChoices = false;

    }
    #endregion

    public void KnowYourOpponentConfirmedChoice () {

        messagePanel.SetActive (false);

        knowYourOpponentChoice.transform.parent.gameObject.SetActive (false);

        localPlayer.SetStringChoiceServerRpc ("KnowYourOpponent", knowYourOpponentChoice.text);

        SC_BaseCard.activeCard.MakingChoices = false;

    }

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }

}
