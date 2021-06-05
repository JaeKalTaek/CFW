using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_SignatureMoves : SC_BaseCard {

        int handSize;

        protected override IEnumerator ApplyEffects () {

            handSize = Caller.Hand.Count;

            SC_GrabCard.canGrab = (c) => {

                foreach (SC_BaseCard ca in SC_GrabCard.selectedCards)
                    if (ca.matchHeat == c.matchHeat)
                        return false;

                return true;

            };

            return base.ApplyEffects ();

        }

        public override void GrabFinished () {

            StartCoroutine (GrabbedToRing ());            

        }

        IEnumerator GrabbedToRing () {

            for (int i = Caller.Hand.Count - handSize; i > 0; i--) {

                ApplyingEffects = true;

                SC_UI_Card target = null;

                for (int j = handSize; j < Caller.Hand.Count; j++)
                    if (!target || Caller.Hand[j].matchHeat > target.Card.matchHeat)
                        target = Caller.Hand[j].UICard;

                target.transform.SetParent (transform);

                Caller.Hand.Remove (target.Card);

                SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

                target.RecT.anchorMin = target.RecT.anchorMax = target.RecT.pivot = Vector2.one * .5f;

                target.Flip (!Caller.IsLocalPlayer, 1);

                yield return target.RecT.DOAnchorPos (Vector2.zero, 1).WaitForCompletion ();

                target.transform.SetAsLastSibling ();

                target.BigRec.anchorMin = target.BigRec.anchorMax = target.BigRec.pivot = Vector2.one * .5f;

                target.BigRec.anchoredPosition = Vector2.zero;

            }

            cardHovered = new OnCardHovered ((c, b) => {

                if (c == this) {

                    c.UICard.ShowBigCard (false);

                    transform.GetChild (transform.childCount - 1).GetComponent<SC_UI_Card> ().BlockClick = b;

                    transform.GetChild (transform.childCount - 1).GetComponent<SC_UI_Card> ().ShowBigCard (b);

                }

            });

            OnCardHoveredEvent += cardHovered;

            FinishedUsing ();

        }

        protected override void OnNewTurnTriggered () {            

            if (Caller.Turn) {

                activeCard = this;

                ApplyingEffects = true;

                Caller.IntChoices["SignatureMoveDraw"] = -1;

                StartCoroutine (DrawSignatureMove ());

                if (Caller.IsLocalPlayer)
                    UI.ShowBooleanChoiceUI ("Draw a signature move", "Regular draw from deck", (b) => {

                        if (b)
                            Caller.SetIntChoiceServerRpc ("SignatureMoveDraw", 0);
                        else
                            Caller.FinishedApplyingEffectsServerRpc ();

                    });

            }

        }

        IEnumerator DrawSignatureMove () {

            while (ApplyingEffects && Caller.IntChoices["SignatureMoveDraw"] == -1)
                yield return new WaitForEndOfFrame ();

            if (ApplyingEffects) {

                Caller.SkipDraw = true;

                RectTransform rT = Caller.IsLocalPlayer ? GM.localHand : GM.otherHand;

                SC_UI_Card c = transform.GetChild (transform.childCount - 1).GetComponent<SC_UI_Card> ();

                c.RecT.anchorMin = c.RecT.anchorMax = c.RecT.pivot = c.BigRec.anchorMin = c.BigRec.anchorMax = c.BigRec.pivot = new Vector2 (.5f, Caller.IsLocalPlayer ? 0 : 1);

                c.BigRec.anchoredPosition = Vector2.up * GM.yOffset;

                c.transform.SetParent (rT);

                SC_Deck.OrganizeHand (rT);

                Vector3 target = c.transform.localPosition;

                c.transform.position = transform.position;

                yield return c.transform.DOLocalMove (target, GM.drawSpeed, true).WaitForCompletion ();

                (Caller.IsLocalPlayer ? SC_Player.localPlayer : SC_Player.otherPlayer).Hand.Add (c.Card);

                c.BlockClick = false;

                if (GetComponentsInChildren<SC_BaseCard> ().Length == 1) {

                    OnCardHoveredEvent -= cardHovered;

                    UICard.ToGraveyard (1, AppliedEffects, false);

                } else
                    ApplyingEffects = false;

            }

        }

    }

}

