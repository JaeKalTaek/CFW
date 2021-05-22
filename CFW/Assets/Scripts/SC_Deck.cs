using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using Card;
using static SC_Player;
using System.Collections;
using UnityEngine.UI;
using static SC_Global;

public class SC_Deck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool ordered;

    public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    public TextMeshProUGUI TSize;

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    bool Local { get; set; }

    SC_Player owner;

    RectTransform RectT { get { return transform as RectTransform; } }

    public void Setup (bool local) {

        Local = local;

        owner = Local ? localPlayer : otherPlayer;

        StartCoroutine (Draw (GM.startHandSize, false, false));

        if (!local)
            RectT.anchorMin = RectT.anchorMax = RectT.pivot = Vector2.up;        

    }

    public SC_UI_Card CreateCard (Transform parent, SC_BaseCard original) {

        SC_UI_Card c = SC_BaseCard.Create (original, parent);

        cards.Remove (original);

        if (cards.Count <= 0) {

            GetComponent<Image> ().enabled = false;

            TSize.gameObject.SetActive (false);

        } else
            TSize.text = Size.ToString ();

        return c;

    }

    public IEnumerator Draw (int nbr, bool StartTurn, bool tween = true) {

        for (int i = 0; i < nbr; i++)
            yield return StartCoroutine (Draw (StartTurn && i == nbr - 1, tween));

    }

    public static void OrganizeHand (RectTransform rT) {

        for (int i = 0; i < rT.childCount; i++)
            (rT.GetChild (i).transform as RectTransform).anchoredPosition = new Vector2((((rT.childCount - 1) / 2f) - i) * -(GM.cardWidth / 2), GM.yOffset.F(rT == GM.otherHand));

    }

    public IEnumerator Draw (bool startTurn, bool tween = true) {

        if (owner.Hand.Count < GM.maxHandSize && (cards.Count > 0 || owner.Graveyard.Cards.Count > 0)) {

            if (cards.Count <= 0)
                yield return StartCoroutine (Refill ());

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

                c.Flip (Local, GM.drawSpeed);

                yield return c.transform.DOLocalMove (target, GM.drawSpeed, true).WaitForCompletion ();

            }

            FinishDrawing (c, startTurn);

        } else
            FinishDrawing (null, startTurn);

    }

    void FinishDrawing (SC_UI_Card c, bool startTurn) {

        if (c)
            owner.Hand.Add (c.Card);

        if (startTurn) {

            owner.ApplySingleEffect ("Stamina", GM.staminaPerTurn);

            owner.StartTurn ();

        }

    }

    bool shuffled;

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

        shuffled = true;

    }

    public IEnumerator Refill () {

        shuffled = false;

        foreach (SC_BaseCard c in owner.Graveyard.Cards) {

            cards.Add (Resources.Load<SC_BaseCard> (c.Path));

            c.UICard.transform.SetParent (transform);

            c.UICard.RecT.DOAnchorPos (Vector2.zero, 1);

            c.UICard.Flip (true, 1, false);

        }

        yield return new WaitForSeconds (1);

        GetComponent<Image> ().enabled = true;

        TSize.text = Size.ToString ();

        foreach (SC_BaseCard c in owner.Graveyard.Cards)
            Destroy (c.UICard.gameObject);

        owner.Graveyard.Cards.Clear ();        

        if (Local)
            Shuffle ();

        while (!shuffled)
            yield return new WaitForEndOfFrame ();

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if (cards.Count > 0)
            TSize.gameObject.SetActive (true);

    }

    public void OnPointerExit (PointerEventData eventData) {

        TSize.gameObject.SetActive (false);

    }

}
