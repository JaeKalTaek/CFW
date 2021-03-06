using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Card;
using static SC_Player;
using System.Collections;
using UnityEngine.UI;

public class SC_Deck : SC_CardZone, IPointerEnterHandler, IPointerExitHandler {

    public bool ordered;

    public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    public TextMeshProUGUI TSize;

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }    

    SC_Player owner;

    void OnValidate () {

        foreach (SC_BaseCard c in cards)
            if (!c)
                print ("CARD ERROR IN:" + name);

    }

    public void Setup (bool local) {

        Local = local;

        owner = Local ? localPlayer : otherPlayer;

        StartCoroutine (Draw (GM.startHandSize, false));

        if (!local)
            RecT.anchorMin = RecT.anchorMax = RecT.pivot = Vector2.up;        

    }

    public override SC_UI_Card CreateCard (SC_BaseCard original, RectTransform parent = null) {

        SC_UI_Card c = base.CreateCard (original, parent);

        if (cards.Count <= 0) {

            GetComponent<Image> ().enabled = false;

            TSize.gameObject.SetActive (false);

        } else
            TSize.text = Size.ToString ();

        return c;

    }

    public IEnumerator Draw (int nbr, bool tween = true) {

        for (int i = 0; i < nbr; i++)
            yield return StartCoroutine (Draw (tween));

    }

    public static void OrganizeHand (RectTransform rT) {

        for (int i = 0; i < rT.childCount; i++)
            (rT.GetChild (i).transform as RectTransform).anchoredPosition = new Vector2((((rT.childCount - 1) / 2f) - i) * -(GM.cardWidth / 2), GM.yOffset.F(rT == GM.otherHand) / (rT == GM.localHand ? 1 : .67f));

    }

    public IEnumerator Draw (bool tween = true) {

        if (owner.Hand.Count < GM.maxHandSize && (cards.Count > 0 || owner.Graveyard.Cards.Count > 0)) {

            if (cards.Count <= 0)
                yield return StartCoroutine (Refill ());

            yield return StartCoroutine (Grab (Local, cards[0], tween));

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

        foreach (SC_BaseCard c in new List<SC_BaseCard> (owner.Graveyard.Cards))
            StartCoroutine (c.UICard.DiscardToDeck (this));       

        yield return new WaitForSeconds (1);

        GetComponent<Image> ().enabled = true;

        TSize.text = Size.ToString ();

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

    public override List<SC_BaseCard> GetCards () {

        return cards;

    }

}
