using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_StrikesCombo : SC_BaseCard {

        int handSize;

        protected override IEnumerator ApplyEffects () {

            handSize = Caller.Hand.Count;

            interceptNext = true;

            yield return StartCoroutine (base.ApplyEffects ());

        }

        public override void FinishedUsing (bool countered = false) {

            if (!countered && Caller.IsLocalPlayer)
                StartCoroutine (PlayCombo ());

        }

        protected override void AddRemoveModifier (bool add) { }

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Strike))
                c.unblockable = true;

        }

        bool comboNext;

        IEnumerator PlayCombo () {

            int nbr = Caller.Hand.Count - handSize;

            for (int i = 0; i < nbr; i++) {

                Caller.Hand[handSize].StartCoroutine (Caller.Hand[handSize].StartPlaying ());

                while (!comboNext)
                    yield return new WaitForEndOfFrame ();

                comboNext = false;

            }

            Caller.FinishStrikesComboServerRpc (nbr);

        }

        public IEnumerator FinishCombo (int nbr) {

            yield return StartCoroutine (SC_Player.WaitForPlayers ());

            modifierCards.Remove (this);

            for (int i = 0; i < nbr; i++)
                StartCoroutine (Caller.Graveyard.Cards[Caller.Graveyard.Cards.Count - 1].UICard.DiscardToDeck (Caller.Deck));

            if (nbr > 0)
                yield return new WaitForSeconds (1);

            if (Caller.IsLocalPlayer) {

                if (Receiver.Stamina < 3)
                    StartPinfallChoice ();
                else
                    NextTurn ();

            }

        }

        protected override void InterceptFinish () {

            interceptNext = true;

            comboNext = true;

        }

    }

}
