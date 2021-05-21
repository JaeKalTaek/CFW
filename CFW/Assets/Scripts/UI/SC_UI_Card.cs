﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;
using DG.Tweening;
using System;
using static Card.SC_BaseCard;
using static SC_Global;
using System.Collections;

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

        if ((IsBasic && Card != activeCard) || localPlayer.Hand.Contains (Card) || lockingCard == Card) {

            Card.CardHovered (true);

            bigCard.transform.SetParent (transform.parent.parent);

            bigCard.transform.SetAsLastSibling ();

            bigCard.SetActive (true);

            StartCoroutine (Hovered ());

        }

    }

    IEnumerator Hovered () {

        while ((IsBasic && Card != activeCard) || localPlayer.Hand.Contains (Card) || lockingCard == Card)
            yield return new WaitForEndOfFrame ();

        OnPointerExit (new PointerEventData (EventSystem.current));

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (bigCard.activeSelf) {

            StopCoroutine (Hovered ());

            Card.CardHovered (false);

            bigCard.transform.SetParent (transform);

            bigCard.SetActive (false);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (activeCard != Card) {

            if (localPlayer.ChoosingCard == ChoosingCard.DoubleDiscard) {

                if (localPlayer.GetIntChoice ("DoubleDiscard") == -1) {

                    UI.ShowMessage ("DoubleDiscard2");

                    localPlayer.SetIntChoiceServerRpc ("DoubleDiscard", transform.GetSiblingIndex ());

                } else if (transform.GetSiblingIndex () != localPlayer.GetIntChoice ("DoubleDiscard")) {

                    localPlayer.SetIntChoiceServerRpc ("DoubleDiscard2", transform.GetSiblingIndex ());

                    localPlayer.DoubleDiscardServerRpc ();

                    StopChoosing ();

                }

            } else if (localPlayer.ChoosingCard != ChoosingCard.None) {

                if (localPlayer.ChoosingCard == ChoosingCard.Discarding)
                    localPlayer.DiscardServerRpc (transform.GetSiblingIndex ());
                else if (localPlayer.ChoosingCard == ChoosingCard.Assessing)
                    localPlayer.SetIntChoiceServerRpc ("Assess", transform.GetSiblingIndex ());

                StopChoosing ();                

            } else if (bigCard.activeSelf && Card.CanUse (localPlayer)) {                

                foreach (GameObject g in new GameObject[] { UI.basicsPanel, UI.showBasicsButton, UI.showLockedBasicsButton, UI.hideBasicsButton, UI.hideLockedBasicsButton })
                    g.SetActive (false);

                if (IsBasic) {

                    GM.localHand.gameObject.SetActive (true);

                    localPlayer.StartUsingBasicServerRpc (transform.GetSiblingIndex ());

                } else
                    StartCoroutine (Card.StartPlaying ());

                OnPointerExit (new PointerEventData (EventSystem.current));

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

            if (Card.Ephemeral)
                Destroy (gameObject);
            else
                (Card.Caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).Cards.Add (Card);            
            
        });

    }

}
