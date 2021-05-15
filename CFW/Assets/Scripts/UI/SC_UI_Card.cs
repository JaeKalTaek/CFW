using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;
using DG.Tweening;
using System;
using static Card.SC_BaseCard;
using static SC_Global;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public RectTransform RecT { get { return transform as RectTransform; } }

    public RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains ("Local"); } }

    bool IsBasic { get { return Card.Is (CardType.Basic); } }

    void Awake () {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        Card = GetComponentInChildren<SC_BaseCard> ();

        if (!Card)
            BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages (bool faceUp = true) {

        GetComponent<Image> ().sprite = bigCard.GetComponent<Image> ().sprite = Resources.Load<Sprite> (faceUp ? Card.Path : "Sprites/CardBack");

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if ((IsBasic || localPlayer.Hand.Contains (Card) || lockingCard == Card) && (!localPlayer.Busy || localPlayer.ChoosingCard != ChoosingCard.None)) {

            bigCard.transform.SetParent (transform.parent);

            bigCard.transform.SetAsLastSibling ();

            bigCard.SetActive (true);

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (bigCard.activeSelf) {

            bigCard.transform.SetParent (transform);

            bigCard.SetActive (false);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (activeCard != Card) {

            if (localPlayer.ChoosingCard == ChoosingCard.DoubleDiscard) {

                if (localPlayer.GetStringChoice ("DoubleDiscard") == "") {

                    UI.ShowMessage ("DoubleDiscard2");

                    localPlayer.SetStringChoiceServerRpc ("DoubleDiscard", name);

                } else if (name != localPlayer.GetStringChoice ("DoubleDiscard")) {

                    localPlayer.Busy = true;

                    localPlayer.SetStringChoiceServerRpc ("DoubleDiscard2", name);

                    localPlayer.DoubleDiscardServerRpc ();

                    StopChoosing ();

                }

            } else if (localPlayer.ChoosingCard != ChoosingCard.None) {

                if (localPlayer.ChoosingCard == ChoosingCard.Discarding)
                    localPlayer.DiscardServerRpc (name);
                else if (localPlayer.ChoosingCard == ChoosingCard.Assessing) {

                    localPlayer.SetStringChoiceServerRpc ("Assess", name);

                    activeCard.MakingChoices = false;

                }

                StopChoosing ();                

            } else if (bigCard.activeSelf && Card.CanUse (localPlayer)) {

                OnPointerExit (new PointerEventData (EventSystem.current));

                foreach (GameObject g in new GameObject[] { UI.basicsPanel, UI.showBasicsButton, UI.showLockedBasicsButton, UI.hideBasicsButton, UI.hideLockedBasicsButton })
                    g.SetActive (false);

                localPlayer.Busy = true;

                if (IsBasic) {

                    GM.localHand.gameObject.SetActive (true);

                    localPlayer.StartUsingBasicServerRpc (transform.GetSiblingIndex ());

                } else
                    StartCoroutine (Card.StartPlaying ());

            }

        }

    }

    void StopChoosing () {

        OnPointerExit (new PointerEventData (EventSystem.current));

        UI.messagePanel.SetActive (false);

        localPlayer.ChoosingCard = ChoosingCard.None;

    }

    public void Flip (bool flip, float speed, bool faceUp = true) {

        if (flip)
            DOTween.Sequence ().Append (transform.DORotate (Vector3.up * 90, speed / 2)
                .OnComplete (() => { SetImages (faceUp); }))
                .Append (transform.DORotate (Vector3.zero, speed / 2));

    }

    public void ToGraveyard (float speed, Action action, bool? flip = null) {

        transform.SetParent ((Card.Caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).transform);

        RecT.anchorMin = RecT.anchorMax = RecT.pivot = Vector2.one * .5f;

        Flip (flip ?? !Card.Caller.IsLocalPlayer, speed);

        RecT.DOAnchorPos (Vector2.zero, speed).OnComplete (() => {

            action ();

            if (Card.IsEphemeral ())
                Destroy (gameObject);
            else
                (Card.Caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).Cards.Add (Card);            
            
        });

    }

}
