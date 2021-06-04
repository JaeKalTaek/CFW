using System.Collections;

namespace Card {

    public class SC_FinishingSequence : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            GM.blockMatchHeatReset++;

            return base.ApplyEffects ();

        }

        public override void FinishedUsing (bool countered = false) {

            if (!countered)
                Counters = 5;

            base.FinishedUsing (countered);

        }

        protected override void OnFinishedPlayingTriggered () {

            if (activeCard as SC_AttackCard) {

                Counters--;

                if (Counters <= 0)
                    UICard.ToGraveyard (1, () => { activeCard.BaseFinishedUsing (); }, false);
                else
                    activeCard.BaseFinishedUsing ();

            } else
                activeCard.BaseFinishedUsing ();

        }

        public override void DiscardedFromRing () {

            GM.blockMatchHeatReset--;

            base.DiscardedFromRing ();

        }

    }

}
