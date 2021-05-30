using System.Collections;
using UnityEngine;
using static SC_Global;

namespace Card {

    public class SC_StrikesCombo : SC_BaseCard {

        int handSize;

        protected override IEnumerator ApplyEffects () {

            handSize = Caller.Hand.Count;

            interceptFinishCard = this;

            modifierCards.Add (this);

            yield return StartCoroutine (base.ApplyEffects ());

        }

        protected override void FinishedUsing (bool countered = false) {

            if (Caller.IsLocalPlayer)
                StartCoroutine (PlayCombo ());

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

            print ("CALL FINISH");

            Caller.FinishStrikesComboServerRpc (nbr);

            /*for (int i = 0; i < nbr; i++)
                StartCoroutine (Caller.Graveyard.Cards[Caller.Graveyard.Cards.Count - 1].UICard.DiscardToDeck (Caller.Deck));

            if (nbr > 0)
                yield return new WaitForSeconds (1);*/

            //modifierCards.Remove (this);

            // base.FinishedUsing ();

        }

        public IEnumerator FinishCombo (int nbr) {

            modifierCards.Remove (this);

            interceptFinishCard = null;

            foreach (SC_BaseCard c in Caller.Graveyard.Cards)
                print (c.Path);

            for (int i = 0; i < nbr; i++)
                StartCoroutine (Caller.Graveyard.Cards[Caller.Graveyard.Cards.Count - 1].UICard.DiscardToDeck (Caller.Deck));

            if (nbr > 0)
                yield return new WaitForSeconds (1);

            if (Caller.IsLocalPlayer) {

                if (Receiver.Stamina < 3)
                    UI.pinfallPanel.SetActive (true);
                else
                    NextTurn ();

            }

        }

        public override void ApplyModifiers () {

            activeCard.unblockable = true;

        }

        protected override void InterceptFinish () {

            comboNext = true;

        }

    }

}
