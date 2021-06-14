using System.Collections;

namespace Card {

    public class SC_BreakFree : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return user.Stamina >= (lockingCard as SC_Submission).effect.breakCost && base.CanUse (user, ignorePriority, ignoreLocks);

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            GM.AddMatchHeat (GM.Count);

        }

    }

}
