using System.Collections;

namespace Card {

    public class SC_SubmissionTransition : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, true) && SC_Player.otherPlayer.Submitted;

        }

        SC_BaseCard grabbed;

        SC_AttackCard.Cost savedCost;

        protected override IEnumerator ApplyEffects () {

            int handSize = Caller.Hand.Count;

            yield return StartCoroutine (base.ApplyEffects ());

            if (Caller.Hand.Count > handSize) {

                grabbed = Caller.Hand[Caller.Hand.Count - 1];

                grabbed.commonEffects.Add (new CommonEffect (CommonEffectType.Break));

                savedCost = (grabbed as SC_Submission).cost;

                (grabbed as SC_Submission).cost = new SC_AttackCard.Cost ();

                grabbed.UpdateValuesUI ();

                SC_Player.OnNewTurn += RemoveEffect;

            }

        }

        void RemoveEffect () {

            grabbed.commonEffects.RemoveAt (grabbed.commonEffects.Count - 1);

            (grabbed as SC_Submission).cost = savedCost;

            grabbed.UpdateValuesUI ();

            SC_Player.OnNewTurn -= RemoveEffect;

        }

    }

}
