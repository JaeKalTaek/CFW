using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using Card;

public class SC_Deck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    public TextMeshProUGUI TSize;

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    bool Local { get; set; }

    RectTransform RectT { get { return transform as RectTransform; } }

    public void Setup (bool local) {

        Local = local;

        Draw (GM.startHandSize, false, false);

        if (!local) {

            RectT.anchorMin = Vector2.up;
            RectT.anchorMax = Vector2.up;
            // RectT.anchoredPosition = new Vector2(-RectT.anchoredPosition.x, RectT.anchoredPosition.y);
            RectT.localRotation = Quaternion.Euler (0, 0, 180);
            TSize.rectTransform.localRotation = Quaternion.Euler (0, 0, -180);

        }

        TSize.gameObject.SetActive(false);

    }

    public void Draw (int nbr, bool StartTurn, bool tween = true) {

        for (int i = 0; i < nbr; i++)
            Draw (StartTurn, tween);

    }

    public static void OrganizeHand (RectTransform rT) {

        for (int i = 0; i < rT.childCount; i++)
            rT.GetChild (i).transform.localPosition = new Vector3((((rT.childCount - 1) / 2f) - i) * -(GM.cardWidth / 2), 108.I(rT == GM.otherHand), 0);

    }

    void Draw (bool startTurn, bool tween = true) {

        if ((Local ? SC_Player.localPlayer : SC_Player.otherPlayer).Hand.Count < GM.maxHandSize) {

            RectTransform rT = Local ? GM.localHand : GM.otherHand;

            SC_UI_Card c = Instantiate (Resources.Load<SC_UI_Card> ("Prefabs/Card"), Vector3.zero, Local ? Quaternion.identity : Quaternion.Euler (0, 0, 180), rT);

            c.name = cards[0].Path;

            c.Card = Instantiate (Resources.Load<SC_BaseCard> (cards[0].Path), c.transform);

            c.Card.UICard = c;

            if (Local && !tween)
                c.SetImages ();

            cards.RemoveAt (0);

            OrganizeHand (rT);

            TSize.text = Size.ToString ();

            if (tween) {

                c.Moving = true;

                Vector3 target = c.transform.localPosition;

                c.transform.position = transform.position;

                c.transform.DOLocalMove (target, GM.drawSpeed, true).OnComplete (() => { FinishDrawing (c, startTurn); });

                if (Local)
                    DOTween.Sequence ().Append (c.transform.DORotate (Vector3.up * 90, GM.drawSpeed / 2).OnComplete (() => { c.SetImages (); })).Append (c.transform.DORotate (Vector3.zero, GM.drawSpeed / 2));

            } else 
                FinishDrawing (c, startTurn);

        } else {

            FinishDrawing (null, startTurn);

        }

    }

    void FinishDrawing (SC_UI_Card c, bool startTurn) {

        if (c) {

            (Local ? SC_Player.localPlayer : SC_Player.otherPlayer).Hand.Add (c.Card);

            c.Moving = false;

        }

        if (SC_Player.localPlayer.Turn && startTurn) {

            SC_Player.localPlayer.CanPlay = true;

            UI.skipButton.SetActive (true);

        }             

    }

    public void Shuffle () {

        int[] newOrder = new int[cards.Count];

        for (int i = 0; i < newOrder.Length; i++)
            newOrder[i] = i;

        newOrder.Shuffle();

        SC_Player.localPlayer.ShuffleDeckServerRpc (newOrder);

    }

    public void Shuffle (int[] newOrder) {

        List<SC_BaseCard> oldCards = new List<SC_BaseCard> (cards);

        for (int i = 0; i < newOrder.Length; i++)
            cards[i] = oldCards[newOrder[i]];

    }

    public void OnPointerEnter (PointerEventData eventData) {

        TSize.gameObject.SetActive (true);

    }

    public void OnPointerExit (PointerEventData eventData) {

        TSize.gameObject.SetActive (false);

    }

}
