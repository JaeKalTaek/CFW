using System;
using TMPro;
using UnityEngine;
using static SC_Player;
using static Card.SC_BaseCard;
using static SC_Global;
using UnityEngine.UI;
using Card;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static SC_UI_Manager Instance;

    public Transform localValues, otherValues;

    public TextMeshProUGUI count;

    public GameObject bodyPartDamageChoicePanel, messagePanel;

    public TextMeshProUGUI endText;

    public GameObject basicsPanel, showBasicsButton, hideBasicsButton;

    public RingSlot[] ringSlots;

    [Serializable]
    public struct RingSlot {

        public Transform slot;

        public SC_BaseCard card;

    }

    [Serializable]
    public struct ThreeChoicesUI { public GameObject panel, leftChoice, rightChoice; }

    public ThreeChoicesUI threeChoicesUI;

    [Serializable]
    public struct AlignmentChoiceUI { public GameObject panel; public TextMeshProUGUI heelChoice, faceChoice; }

    public AlignmentChoiceUI alignmentChoiceUI;

    public TMP_InputField knowYourOpponentChoice;

    public GameObject booleanChoiceUI;

    public delegate void BooleanChoice (bool b);

    [Serializable]
    public struct NumberChoiceUI {

        public GameObject panel;
        public TextMeshProUGUI text;
        public GameObject upButton, downButton;

        public int Max { get; set; }
        public int Number { get; set; }

    }

    public NumberChoiceUI numberChoiceUI;

    [Serializable]
    public struct GrabUI {

        public GameObject panel, showButton;

        public TextMeshProUGUI text;

        public Button confirmButton;

        public RectTransform container;

    }

    public GrabUI grabUI;

    void Awake () {

        Instance = this;

        knowYourOpponentChoice.onSubmit.AddListener (KnowYourOpponentConfirmedChoice);

    }

    public void SetValue (bool local, string id, int value) {

        (local ? localValues : otherValues).Find (id).GetChild (1).GetComponent<TextMeshProUGUI> ().text = value.ToString ();

    }

    public void ShowMessage (string id) {

        Messages.TryGetValue (id, out string m);

        messagePanel.GetComponentInChildren<TextMeshProUGUI> ().text = m;

        messagePanel.SetActive (true);

    }

    public void ShowBasics (bool show) {

        showBasicsButton.SetActive (!show);

        hideBasicsButton.SetActive (show);

        if (show)
            for (int i = 0; i < 11; i++)
                basicsPanel.transform.GetChild (i).gameObject.SetActive (NoLock ? (0 <= i && i <= 2) : (localPlayer.Unlocked ? (otherPlayer.Pinned ? i == 9 : i == 10) : (localPlayer.Pinned ? (4 == i || i == 5) : (6 <= i && i <= 8))));      

        basicsPanel.SetActive (show);        

        GM.localHand.gameObject.SetActive (!show);

    }

    public void ShowBooleanChoiceUI (string a, string b, BooleanChoice c) {

        booleanChoiceUI.transform.GetChild (0).GetComponent<Button> ().SetupChoiceButton (true, a, c);
        booleanChoiceUI.transform.GetChild (1).GetComponent<Button> ().SetupChoiceButton (false, b, c);
        booleanChoiceUI.SetActive (true);

    }

    public void ShowThreeChoices (string[] texts) {

        for (int i = 0; i < 3; i++) {

            threeChoicesUI.panel.transform.GetChild (i).GetComponentInChildren<TextMeshProUGUI> ().text = texts[i];

            threeChoicesUI.panel.transform.GetChild (i).gameObject.SetActive (true);

        }

        threeChoicesUI.panel.SetActive (true);

    }

    public void ThreeChoicesSelect (int choice) {

        threeChoicesUI.panel.SetActive (false);

        activeCard.MakingChoices = false;

        localPlayer.SetIntChoiceServerRpc ("ThreeChoices", choice);

    }

    #region Alignment choice
    public void ShowAlignmentChoice (int value) {

        alignmentChoiceUI.heelChoice.text = "Alignment - " + value;

        alignmentChoiceUI.faceChoice.text = "Alignment + " + value;

        alignmentChoiceUI.panel.SetActive (true);

    }

    public void AlignmentChoice (int value) {

        alignmentChoiceUI.panel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("AlignmentChoice", activeCard.CurrentEffect.effectValue * value);

        activeCard.MakingChoices = false;

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

        activeCard.MakingChoices = false;

    }
    #endregion

    #region Know Your Opponent choice
    void KnowYourOpponentConfirmedChoice (string s) {

        messagePanel.SetActive (false);

        knowYourOpponentChoice.transform.parent.gameObject.SetActive (false);

        localPlayer.SetStringChoiceServerRpc ("KnowYourOpponent", s);

        activeCard.MakingChoices = false;
    }

    public void KnowYourOpponentConfirmedChoice () {

        KnowYourOpponentConfirmedChoice (knowYourOpponentChoice.text);

    }
    #endregion

    #region Number Choice
    public void ShowNumberChoiceUI (int max) {

        numberChoiceUI.Number = numberChoiceUI.Max = max;

        UpdateNumberChoice ();

        numberChoiceUI.panel.SetActive (true);

    }

    public void ChangeNumberChoice (bool add) {

        numberChoiceUI.Number += add ? 1 : -1;

        UpdateNumberChoice ();

    }

    void UpdateNumberChoice () {

        numberChoiceUI.text.text = numberChoiceUI.Number.ToString ();

        numberChoiceUI.upButton.SetActive (numberChoiceUI.Number < numberChoiceUI.Max);

        numberChoiceUI.downButton.SetActive (numberChoiceUI.Number > 1);

    }

    public void ConfirmNumberChoice (bool yes) {

        numberChoiceUI.panel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("NumberChoice", yes ? numberChoiceUI.Number : 0);

        activeCard.MakingChoices = false;

    }
    #endregion

    #region Grab
    public void ShowGrabPanel () {

        grabUI.panel.transform.SetAsLastSibling ();

        grabUI.showButton.SetActive (true);

        grabUI.text.text = "Select " + activeCard.GrabsRemaining;

        grabUI.panel.SetActive (true);

    }
    
    public void ShowGrabPanel (bool show) {

        grabUI.panel.SetActive (show);

    }

    public void GrabSelected () {

        grabUI.confirmButton.interactable = activeCard.GrabsRemaining == 0;

        grabUI.text.text = activeCard.GrabsRemaining == 0 ? "Confirm or change selection" : "Select " + activeCard.GrabsRemaining;

    }

    public void ConfirmGrab () {

        grabUI.showButton.SetActive (false);

        grabUI.panel.SetActive (false);

        localPlayer.SetIntChoiceServerRpc ("Grab", 0);

    }
    #endregion

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }   

}
