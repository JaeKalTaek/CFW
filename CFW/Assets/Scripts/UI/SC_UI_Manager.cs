using System;
using TMPro;
using UnityEngine;
using static SC_Player;
using static Card.SC_BaseCard;
using static SC_Global;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SC_UI_Manager : MonoBehaviour {

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public RectTransform canva;

    public static SC_UI_Manager Instance;

    public Transform localValues, otherValues;

    public TextMeshProUGUI count;

    public GameObject bodyPartDamageChoicePanel, messagePanel;

    public TextMeshProUGUI endText;

    public GameObject basicsPanel;

    public Transform hoveredParent;

    public RectTransform turnIndicator;
    public float turnIndicatorRotation;

    public Color staminaColor, healthColor;
    public Color darkGrey, lightGrey;

    public RingSlot[] localRingSlots, otherRingSlots;

    [Serializable]
    public struct RingSlot {

        public Transform slot;

        public bool occupied;

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

    [Serializable]
    public struct KeywordsReminder {

        public RectTransform panel;

        public ScrollRect view;

        public TextMeshProUGUI text;

    }

    public KeywordsReminder keywordsReminder;

    [Serializable]
    public struct History {

        public GameObject panel;

        public ScrollRect view;

        public RectTransform content;

        public RectTransform linePrefab, cardPrefab;

    }

    public History history;

    void Awake () {

        Instance = this;

        knowYourOpponentChoice.onSubmit.AddListener (KnowYourOpponentConfirmedChoice);

    }

    void Update () {

        if (Input.GetKeyDown (KeyCode.H))
            ShowHistory ();

    }

    public void SetValue (bool local, string id, int value, bool anim = false) {

        TextMeshProUGUI t = (local ? localValues : otherValues).Find (id).GetChild (1).GetComponent<TextMeshProUGUI> ();

        t.text = value.ToString ();

        if (anim)
            t.transform.DOScale (t.transform.localScale * 2f, .5f).OnComplete (() => { t.transform.DOScale (t.transform.localScale / 2f, .5f); });

    }

    public void ShowMessage (string id) {

        Messages.TryGetValue (id, out string m);

        messagePanel.GetComponentInChildren<TextMeshProUGUI> ().text = m;

        messagePanel.SetActive (true);

    }

    public void ShowBasics () {

        for (int i = 0; i < 11; i++)
            basicsPanel.transform.GetChild (i).gameObject.SetActive (NoLock ? (0 <= i && i <= 2) : (localPlayer.Unlocked ? (otherPlayer.Pinned ? i == 9 : i == 10) : (localPlayer.Pinned ? (4 == i || i == 5) : (6 <= i && i <= 8))));

        basicsPanel.SetActive (true);

    }

    public IEnumerator UpdateTurn (bool active) {

        yield return turnIndicator.DORotate (new Vector3 (0, 0, turnIndicatorRotation.F (active)), .5f).WaitForCompletion ();

    }

    public void ShowBooleanChoiceUI (string a, string b, BooleanChoice c) {

        booleanChoiceUI.transform.GetChild (0).GetComponent<Button> ().SetupChoiceButton (true, a, c);
        booleanChoiceUI.transform.GetChild (1).GetComponent<Button> ().SetupChoiceButton (false, b, c);
        booleanChoiceUI.SetActive (true);

    }

    #region Three choices
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
    #endregion

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

    #region History
    void ShowHistory () {

        if (history.content.childCount > 0) {

            history.panel.SetActive (!history.panel.activeSelf);

            history.view.verticalNormalizedPosition = 0;

        }

    }

    public void AddCardToHistory (Image m, bool you) {

        if (history.content.childCount < GM.Turn) {

            RectTransform l = Instantiate (history.linePrefab, history.content);

            l.anchoredPosition = new Vector2 (l.anchoredPosition.x, -(l.sizeDelta.y + 50) * (GM.Turn - 1));

            l.GetComponentInChildren<TextMeshProUGUI> ().text = "Turn " + GM.Turn;

        }

        RectTransform c = Instantiate (history.cardPrefab, history.content.GetChild (GM.Turn - 1));

        c.anchoredPosition = new Vector2 (c.anchoredPosition.x + (history.content.GetChild (GM.Turn - 1).childCount - 2) * c.sizeDelta.x, c.anchoredPosition.y);

        c.GetComponent<Image> ().sprite = m.sprite;

        c.GetComponentInChildren<TextMeshProUGUI> ().text = you ? "YOU" : "OPPONENT";

        history.content.sizeDelta = new Vector2 (0, Mathf.Max (1080, history.content.childCount * (history.linePrefab.sizeDelta.y + 50)));

        history.view.verticalNormalizedPosition = 0;

    }
    #endregion

    public void ShowEndPanel (bool won) {

        endText.text = "YOU " + (won ? "WON" : "LOST");

        endText.transform.parent.gameObject.SetActive (true);

    }   

    public Vector2 MousePosUI () {

        return Vector2.Scale (Camera.main.ScreenToViewportPoint (Input.mousePosition), canva.sizeDelta);

    }

}
