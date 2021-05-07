using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_KnowYourOpponent : SC_BaseCard {

        public override IEnumerator StartUsing () {

            MakingChoices = true;

            StartCoroutine (base.StartUsing ());

            UI.ShowMessage ("KnowYourOpponent");

            UI.knowYourOpponentChoice.text = "";

            UI.knowYourOpponentChoice.transform.parent.gameObject.SetActive (true);

            yield return null;

        }

        public override void ApplyEffect () {

            base.ApplyEffect ();

            string choice = Caller.GetStringChoice ("KnowYourOpponent").Replace (" ", "").ToLower ();

            foreach (SC_BaseCard c in Other.Hand) {

                if (c.name.Replace ("(Clone)", "").ToLower () == choice) {

                    ApplyingEffects = true;

                    c.Discard (Other);

                    return;

                }

            }

            foreach (SC_BaseCard c in Other.Deck.cards) {

                if (c.name.Replace ("(Clone)", "").Replace (" ", "").ToLower () == choice) {                    

                    ApplyingEffects = true;

                    SC_UI_Card card = Other.Deck.CreateCard (Other.Deck.transform, c);

                    card.RecT.anchoredPosition = Vector2.zero;

                    card.Card.Caller = Other;

                    card.ToGraveyard (GM.drawSpeed, () => { activeCard.ApplyingEffects = false; }, true);

                    return;

                }

            }

        }

    }

}
