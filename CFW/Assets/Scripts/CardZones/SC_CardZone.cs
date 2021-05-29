using Card;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SC_CardZone : MonoBehaviour {

    protected bool Local { get; set; }

    public bool IsDiscard { get { return this as SC_Graveyard; } }

    public abstract List<SC_BaseCard> GetCards ();

    public RectTransform RecT { get { return GetComponent<RectTransform> (); } }

    public virtual SC_UI_Card CreateCard (SC_BaseCard original, RectTransform parent = null) {

        SC_UI_Card c = SC_BaseCard.Create (original, parent ?? RecT);

        GetCards ().Remove (original);

        return c;

    }

    public virtual IEnumerator Grab (bool local, SC_BaseCard original, bool tween = true) {

        SC_GameManager GM = SC_GameManager.Instance;

        RectTransform rT = local ? GM.localHand : GM.otherHand;

        SC_UI_Card c = CreateCard (original, rT);

        c.Card.Stolen = local != Local;

        if (!local)
            c.RecT.anchorMin = c.RecT.anchorMax = c.RecT.pivot = new Vector2 (.5f, 1);

        if (IsDiscard || (local && !tween))
            c.SetImages ();

        SC_Deck.OrganizeHand (rT);

        if (tween) {

            Vector3 target = c.transform.localPosition;

            c.transform.position = transform.position;

            c.Flip (local && !IsDiscard, GM.drawSpeed);

            yield return c.transform.DOLocalMove (target, GM.drawSpeed, true).WaitForCompletion ();

        }

        (local ? SC_Player.localPlayer : SC_Player.otherPlayer).Hand.Add (c.Card);

    }

}
