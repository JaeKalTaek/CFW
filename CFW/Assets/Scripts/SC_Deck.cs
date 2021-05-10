using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using Card;
using static SC_Player;
using System.Collections;
using System;
using UnityEngine.UI;

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

        if (!local)
            RectT.anchorMin = RectT.anchorMax = RectT.pivot = Vector2.up;        

    }

    public SC_UI_Card CreateCard (Transform parent, SC_BaseCard original) {

        SC_UI_Card c = Instantiate (Resources.Load<SC_UI_Card> ("Prefabs/P_UI_Card"), Vector3.zero, Quaternion.identity, parent);

        c.name = original.Path;

        c.Card = Instantiate (original, c.transform);

        cards.Remove (original);

        if (cards.Count <= 0) {

            GetComponent<Image> ().enabled = false;

            TSize.gameObject.SetActive (false);

        } else
            TSize.text = Size.ToString ();

        return c;

    }

    public void Draw (int nbr, bool StartTurn, bool tween = true) {

        for (int i = 0; i < nbr; i++)
            Draw (StartTurn, tween);

    }

    public static void OrganizeHand (RectTransform rT) {

        for (int i = 0; i < rT.childCount; i++)
            (rT.GetChild (i).transform as RectTransform).anchoredPosition = new Vector2((((rT.childCount - 1) / 2f) - i) * -(GM.cardWidth / 2), GM.yOffset.F(rT == GM.otherHand));

    }

    public void Draw (bool startTurn, bool tween = true) {

        if ((Local ? localPlayer : otherPlayer).Hand.Count < GM.maxHandSize) {

            if (cards.Count <= 0) {

                StartCoroutine (Refill (() => { Draw (startTurn, tween); }));

                return;

            }

            RectTransform rT = Local ? GM.localHand : GM.otherHand;

            SC_UI_Card c = CreateCard (rT, cards[0]);

            if (!Local)
                c.RecT.anchorMin = c.RecT.anchorMax = c.RecT.pivot = new Vector2 (.5f, 1);            

            if (Local && !tween)
                c.SetImages ();

            OrganizeHand (rT);            

            if (tween) {

                Vector3 target = c.transform.localPosition;

                c.transform.position = transform.position;

                c.transform.DOLocalMove (target, GM.drawSpeed, true).OnComplete (() => { FinishDrawing (c, startTurn); });

                c.Flip (Local, GM.drawSpeed);

            } else 
                FinishDrawing (c, startTurn);

        } else
            FinishDrawing (null, startTurn);

    }

    void FinishDrawing (SC_UI_Card c, bool startTurn) {

        if (c)
            (Local ? localPlayer : otherPlayer).Hand.Add (c.Card);

        if (localPlayer.Turn && startTurn)
            (NoLock ? UI.showBasicsButton : (otherPlayer.Submitted ? UI.maintainSubmissionButton : UI.showLockedBasicsButton)).SetActive (true);         

    }

    public void Shuffle () {

        int[] newOrder = new int[cards.Count];

        for (int i = 0; i < newOrder.Length; i++)
            newOrder[i] = i;

        newOrder.Shuffle();

        localPlayer.ShuffleDeckServerRpc (newOrder);

    }

    public void Shuffle (int[] newOrder) {

        List<SC_BaseCard> oldCards = new List<SC_BaseCard> (cards);

        for (int i = 0; i < newOrder.Length; i++)
            cards[i] = oldCards[newOrder[i]];

    }

    IEnumerator Refill (Action a = null) {

        foreach (SC_BaseCard c in (Local ? GM.localGraveyard : GM.otherGraveyard).Cards) {

            cards.Add (Resources.Load<SC_BaseCard> (c.Path));

            c.UICard.transform.SetParent (transform);

            c.UICard.RecT.DOAnchorPos (Vector2.zero, 1);

            c.UICard.Flip (true, 1, false);

        }

        yield return new WaitForSeconds (1);

        GetComponent<Image> ().enabled = true;

        TSize.text = Size.ToString ();

        foreach (SC_BaseCard c in (Local ? GM.localGraveyard : GM.otherGraveyard).Cards)
            Destroy (c.UICard.gameObject);

        (Local ? GM.localGraveyard : GM.otherGraveyard).Cards.Clear ();

        a?.Invoke ();

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if (cards.Count > 0)
            TSize.gameObject.SetActive (true);

    }

    public void OnPointerExit (PointerEventData eventData) {

        TSize.gameObject.SetActive (false);

    }

}
