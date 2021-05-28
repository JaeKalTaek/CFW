using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_KnowYourOpponent : SC_BaseCard {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            yield return StartCoroutine (MakeChoice (() => {

                UI.ShowMessage ("KnowYourOpponent");

                UI.knowYourOpponentChoice.text = "";

                UI.knowYourOpponentChoice.transform.parent.gameObject.SetActive (true);

                UI.knowYourOpponentChoice.Select ();

            }));

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            string choice = Caller.GetStringChoice ("KnowYourOpponent").Replace (" ", "").ToLower ();

            foreach (SC_BaseCard c in Receiver.Hand) {

                if (c.name.Replace ("(Clone)", "").Replace (" ", "").ToLower () == choice) {

                    yield return StartCoroutine (ApplyEffect (() => {

                        ApplyingEffects = true;

                        c.Discard (Receiver);

                    }));                    

                    break;

                }

            }

            foreach (SC_BaseCard c in Receiver.Deck.cards) {

                if (c.name.Replace ("(Clone)", "").Replace (" ", "").ToLower () == choice) {

                    yield return StartCoroutine (ApplyEffect (() => {

                        ApplyingEffects = true;

                        SC_UI_Card card = Receiver.Deck.CreateCard (c);

                        card.RecT.anchoredPosition = Vector2.zero;

                        card.Card.Caller = Receiver;

                        card.ToGraveyard (GM.drawSpeed, AppliedEffects, true);

                    }));

                    break;

                }

            }

        }

    }

}
