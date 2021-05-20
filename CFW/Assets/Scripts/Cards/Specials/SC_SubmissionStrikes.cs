using System.Collections;

namespace Card {

    public class SC_SubmissionStrikes : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, true) && SC_Player.otherPlayer.Submitted;

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            Receiver.ApplySingleBodyEffect ((lockingCard as SC_Submission).effect.bodyPartDamage.bodyPart, 5);

        }

    }

}
