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

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

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

    void Awake () {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        Card = GetComponentInChildren<SC_BaseCard> ();

        if (!Card)
            BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages (bool faceUp = true) {

        image.sprite = bigImage.sprite = Resources.Load<Sprite> (faceUp ? Card.Path : "Sprites/CardBack");

    }

    public void ShowBigCard (bool show) {

        bigCard.transform.SetParent (show ? UI.hoveredParent : transform);

        bigCard.SetActive (show);

    }

    #region Hover
    public bool OverrideActiveHover { get; set; }

    public void OnPointerEnter (PointerEventData eventData) {

        if ((Card != activeCard || OverrideActiveHover) && (IsBasic || Card.OnTheRing || localPlayer.Hand.Contains (Card) || lockingCard == Card)) {

            counters.root.SetActive (false);

            ShowBigCard (true);

            Card.CardHovered (true);

            StartCoroutine (Hovered ());

        }

    }

    IEnumerator Hovered () {

        while (Card != activeCard && (IsBasic || Card.OnTheRing || localPlayer.Hand.Contains (Card) || lockingCard == Card))
            yield return new WaitForEndOfFrame ();

        OnPointerExit (new PointerEventData (EventSystem.current));

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (bigCard.activeSelf) {

            counters.root.SetActive (Card.Counters > 0);

            StopCoroutine (Hovered ());

            ShowBigCard (false);

            Card.CardHovered (false);

        }

    }
    #endregion

    #region Click
    public bool BlockClick { get; set; }

    public void OnPointerClick (PointerEventData eventData) {

        if (activeCard != Card && !BlockClick) {

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

            } else if (bigCard.activeSelf) {

                if (Card.CanUse (localPlayer)) {

                    foreach (GameObject g in new GameObject[] { UI.basicsPanel, UI.showBasicsButton, UI.hideBasicsButton })
                        g.SetActive (false);

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

    }   

    void StopChoosing () {

        OnPointerExit (new PointerEventData (EventSystem.current));

        UI.messagePanel.SetActive (false);

        localPlayer.ChoosingCard = ChoosingCard.None;

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
            else
                target.Cards.Add (Card);            
            
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
