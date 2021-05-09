using System.Collections;

namespace Card {

    public class SC_BreakFree : SC_BaseCard {

        public override bool CanUse () {

            return SC_Player.localPlayer.Stamina > (lockingCard as SC_Submission).effect.breakCost && base.CanUse ();

        }

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            GM.AddMatchHeat (GM.count);

        }

    }

}
