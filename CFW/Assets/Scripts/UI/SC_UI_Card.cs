using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;
using DG.Tweening;
using System;
using static Card.SC_BaseCard;
using static SC_Global;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler , IEndDragHandler {

    public SC_BaseCard Card;

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public RectTransform RecT { get { return transform as RectTransform; } }

    public RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public Image image, bigImage;

    public GameObject bigCard;

    public GameObject highlight, bigHighlight;

    [Serializable]
    public struct Counters {

        public GameObject root;

        public TextMeshProUGUI nbr;

    }

    public Counters counters, bigCounters;

    bool IsBasic { get { return Card.Is (CardType.Basic); } }

    public bool FaceUp { get { return image.sprite.name != "CardBack"; } }

    [Serializable]
    public struct OffensiveMoveValues {

        public GameObject parent;
        public Transform[] lines;
        public TextMeshProUGUI staminaCost, healthCost, bodyPartsCost;
        public TextMeshProUGUI staminaDamage, healthDamage, bodyPartsDamage;

    }

    [Serializable]
    public struct SubmissionValues {

        public GameObject parent;
        public Transform[] lines;
        public TextMeshProUGUI staminaCost, bodyPartsCost;
        public TextMeshProUGUI staminaReduction, bodyPartsDamage;
        public TextMeshProUGUI breakCost;

    }

    [Header ("Attack cards texts")]
    public OffensiveMoveValues offensiveMoveValues;
    public OffensiveMoveValues offensiveMoveBigValues;

    public SubmissionValues submissionValues, submissionBigValues;


    void Awake () {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        if (Card) {

            Card = Instantiate (Card, transform);

            SetImages (true);

        } else
            BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages (bool faceUp = true) {

        image.sprite = bigImage.sprite = Resources.Load<Sprite> (faceUp ? Card.Path : "Sprites/CardBack");

        ShowAttackValues (faceUp);

    }

    #region Attack values
    public void SetAttackValue (string id, int value, bool health) {

        SetAttackValue (id, value.ToString (), health);

    }

    public void SetAttackValue (string id, string text, bool health) {

        SetAttackValue (Card as SC_OffensiveMove ? offensiveMoveValues : (object) submissionValues, id, text, health);

        SetAttackValue (Card as SC_OffensiveMove ? offensiveMoveBigValues : (object) submissionBigValues, id, text, health);

    }

    void SetAttackValue (object o, string id, string text, bool health) {

        TextMeshProUGUI t = o.GetType ().GetField (id).GetValue (o) as TextMeshProUGUI;

        t.text = text == "0" ? "---" : text.ToString ();

        t.color = text == "0" ? Color.black : (health ? UI.healthColor : UI.staminaColor);

    }

    public void HideBodyPartsTexts () {        

        if (Card as SC_OffensiveMove) {

            HideBodyPartsTexts (offensiveMoveValues);
            HideBodyPartsTexts (offensiveMoveBigValues);

        } else if (Card as SC_Submission) {

            HideBodyPartsTexts (submissionValues);
            HideBodyPartsTexts (submissionBigValues);

        }

    }

    void HideBodyPartsTexts (object o) {

        (o.GetType ().GetField ("bodyPartsCost").GetValue (o) as TextMeshProUGUI).gameObject.SetActive (false);
        (o.GetType ().GetField ("bodyPartsDamage").GetValue (o) as TextMeshProUGUI).gameObject.SetActive (false);

    }

    void ShowAttackValues (bool show) {

        if (Card as SC_OffensiveMove) {

            offensiveMoveValues.parent.SetActive (show);
            offensiveMoveBigValues.parent.SetActive (show);

        } else if (Card as SC_Submission) {

            submissionValues.parent.SetActive (show);
            submissionBigValues.parent.SetActive (show);

        }

    }
    #endregion

    #region Highlight
    public void SetHighlight (bool on) {

        highlight.SetActive (on);

        bigHighlight.SetActive (on);

    }
    #endregion

    #region Hover
    public bool OverrideActiveHover { get; set; }

    public void OnPointerEnter (PointerEventData eventData) {

        if (!dragging && (Card != activeCard || OverrideActiveHover) && (IsBasic || Card.OnTheRing || localPlayer.Hand.Contains (Card) || lockingCard == Card)) {

            ShowBigCard (true);

            Card.CardHovered (true);

            SetupKeywordRemindersPanel ();

            StartCoroutine (Hovered ());

        }

    }

    void SetupKeywordRemindersPanel () {

        string reminders = "";

        List<string> keywords = new List<string> ();

        foreach (CommonEffect ce in Card.commonEffects)
            if (!keywords.Contains (ce.effectType.ToString ()))
                keywords.Add (ce.effectType.ToString ());

        keywords.AddRange (Card.additionalKeywords);

        if ((Card as SC_AttackCard)?.finisher ?? false)
            keywords.Add ("Finisher");

        if (Card.unblockable)
            keywords.Add ("Unblockable");

        for (int i = 0; i < keywords.Count; i++) {

            if (KeywordReminders.TryGetValue (keywords[i], out string r)) {

                reminders += r;

                if (i < keywords.Count - 1)
                    reminders += "\n\n";

            }

        }

        if (reminders != "") {

            UI.keywordsReminder.panel.anchoredPosition = BigRec.anchoredPosition + Vector2.left * (BigRec.sizeDelta.x / 2);

            if (BigRec.anchoredPosition.x - (BigRec.sizeDelta.x / 2) - UI.keywordsReminder.panel.sizeDelta.x < 0)
                UI.keywordsReminder.panel.anchoredPosition += Vector2.right * (BigRec.sizeDelta.x + UI.keywordsReminder.panel.sizeDelta.x);

            float y = Card.OnTheRing ? BigRec.anchoredPosition.y - (UI.keywordsReminder.panel.sizeDelta.y / 2) : (BigRec.sizeDelta.y - UI.keywordsReminder.panel.sizeDelta.y) / 2;

            UI.keywordsReminder.panel.anchoredPosition = new Vector2 (UI.keywordsReminder.panel.anchoredPosition.x, y);

            UI.keywordsReminder.text.text = reminders;

            UI.keywordsReminder.panel.gameObject.SetActive (true);

            UI.keywordsReminder.view.verticalNormalizedPosition = 1;

        }

    }

    IEnumerator Hovered () {

        while (Card != activeCard && (IsBasic || Card.OnTheRing || localPlayer.Hand.Contains (Card) || lockingCard == Card)) {

            if (UI.keywordsReminder.panel.gameObject.activeSelf)
                UI.keywordsReminder.view.verticalNormalizedPosition = Mathf.Clamp01 (UI.keywordsReminder.view.verticalNormalizedPosition + Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * UI.keywordsReminder.view.scrollSensitivity);

            yield return new WaitForEndOfFrame ();

        }

        OnPointerExit (new PointerEventData (EventSystem.current));

    }

    public void OnPointerExit (PointerEventData eventData) {
            
        if (bigCard.activeSelf && !dragging) {            

            StopCoroutine (Hovered ());

            ShowBigCard (false);

            Card.CardHovered (false);

            UI.keywordsReminder.panel.gameObject.SetActive (false);

        }

    }

    public void ShowBigCard (bool show) {

        bigCard.transform.SetParent (show ? UI.hoveredParent : transform);

        bigCard.SetActive (show);

    }
    #endregion

    #region Click
    public bool CantPlay { get; set; }

    public void OnPointerClick (PointerEventData eventData) {

        if (activeCard != Card && !CantPlay) {

            if (localPlayer.ChoosingCard != ChoosingCard.None && !Card.OnTheRing) {

                switch (localPlayer.ChoosingCard) {

                    case ChoosingCard.Discard:
                        localPlayer.DiscardServerRpc (transform.GetSiblingIndex ());
                        StopChoosing ();
                        break;

                    case ChoosingCard.Assess:
                        localPlayer.SetIntChoiceServerRpc ("Assess", transform.GetSiblingIndex ());
                        StopChoosing ();
                        break;

                    case ChoosingCard.DoubleDiscard:
                        if (localPlayer.GetIntChoice ("DoubleDiscard") == -1) {

                            UI.ShowMessage ("DoubleDiscard2");

                            localPlayer.SetIntChoiceServerRpc ("DoubleDiscard", transform.GetSiblingIndex ());

                        } else if (transform.GetSiblingIndex () != localPlayer.GetIntChoice ("DoubleDiscard")) {

                            localPlayer.SetIntChoiceServerRpc ("DoubleDiscard2", transform.GetSiblingIndex ());

                            localPlayer.DoubleDiscardServerRpc ();

                            StopChoosing ();

                        }
                        break;

                    case ChoosingCard.Play:
                        if ((Card as SC_OffensiveMove) && Card.CanUse (localPlayer, true)) {

                            Card.StartCoroutine (Card.StartPlaying ());

                            StopChoosing ();

                        }
                        break;

                }

            } else
                TryPlay ();

        }

    }

    void TryPlay () {

        if (bigCard.activeSelf) {

            if (Card.CanUse (localPlayer)) {

                UI.basicsPanel.SetActive (false);

                if (IsBasic) {

                    GM.localHand.gameObject.SetActive (true);

                    localPlayer.StartUsingBasicServerRpc (transform.GetSiblingIndex ());

                } else
                    Card.StartCoroutine (Card.StartPlaying ());

                OnPointerExit (new PointerEventData (EventSystem.current));

            } else if (localPlayer.Turn && Card.OnTheRing)
                Card.OnRingClicked ();

        }        

    }   

    void StopChoosing () {

        OnPointerExit (new PointerEventData (EventSystem.current));

        UI.messagePanel.SetActive (false);

        localPlayer.ChoosingCard = ChoosingCard.None;

    }
    #endregion

    #region Drag
    static bool dragging;

    Vector2 offset, basePos;

    void IBeginDragHandler.OnBeginDrag (PointerEventData eventData) {

        if (!Card.OnTheRing && bigCard.activeSelf && Card != lockingCard) {

            UI.keywordsReminder.panel.gameObject.SetActive (false);

            dragging = true;

            basePos = BigRec.anchoredPosition;

            offset = basePos - UI.MousePosUI ();

        }

    }

    void IDragHandler.OnDrag (PointerEventData eventData) {

        if (dragging)
            BigRec.anchoredPosition = UI.MousePosUI () + offset;

    }

    void IEndDragHandler.OnEndDrag (PointerEventData eventData) {

        if (dragging) {

            dragging = false;

            bool inDistance = Vector2.Distance (BigRec.anchoredPosition, basePos) <= GM.maxDragDistanceToPlay;

            BigRec.anchoredPosition = basePos;

            if (activeCard != Card && !CantPlay && inDistance && Card.CanUse (localPlayer))
                TryPlay ();
            else
                OnPointerExit (eventData);            

        }

    }
    #endregion

    #region Movement
    public void Flip (bool flip, float speed, bool faceUp = true) {

        if (flip)
            DOTween.Sequence ().Append (transform.DORotate (Vector3.up * 90, speed / 2)
                .OnComplete (() => { SetImages (faceUp); }))
                .Append (transform.DORotate (Vector3.zero, speed / 2));

    }

    public void ToGraveyard (float speed, Action action, bool? flip = null) {

        SC_Graveyard target = Card.Caller.IsLocalPlayer == Card.Stolen ? GM.otherGraveyard : GM.localGraveyard;

        transform.SetParent (target.transform);

        SC_Deck.OrganizeHand (Card.Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

        RecT.anchorMin = RecT.anchorMax = RecT.pivot = Vector2.one * .5f;

        Flip (flip ?? !Card.Caller.IsLocalPlayer, speed);

        if (Card.OnTheRing)
            Card.DiscardedFromRing ();

        RecT.DOAnchorPos (Vector2.zero, speed).OnComplete (() => {

            action ();

            if (Card.Ephemeral)
                Destroy (gameObject);
            else {

                ShowAttackValues (false);

                target.Cards.Add (Card);

            }
            
        });

    }

    public void ToRingSlot () {

        for (int i = 0; i < 4; i++) {        

            if (!(Card.Caller.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[i].occupied) {

                transform.SetParent ((Card.Caller.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[i].slot);

                (Card.Caller.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[i].occupied = true;

                Card.RingSlot = i;

                break;

            }

        }

        RecT.anchorMin = RecT.anchorMax = Vector2.one * .5f;

        RecT.DOAnchorPos (Vector2.zero, 1).OnComplete (() => {

            transform.SetParent (transform.parent.parent);

            BigRec.anchorMin = BigRec.anchorMax = BigRec.pivot = Vector2.one * .5f;

            BigRec.anchoredPosition = Vector2.zero;           

            Card.FinishedUsing ();

        });

    }

    public IEnumerator DiscardToDeck (SC_Deck target) {

        target.cards.Add (Resources.Load<SC_BaseCard> (Card.Path));

        transform.SetParent (target.RecT);        

        Flip (true, 1, false);        

        (target.Local ? localPlayer.Graveyard : otherPlayer.Graveyard).Cards.Remove (Card);

        yield return RecT.DOAnchorPos (Vector2.zero, 1).WaitForCompletion ();

        Destroy (gameObject);

    }    
    #endregion

}
