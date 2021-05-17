using System.Collections;

namespace Card {

    public class SC_MaintainPressure : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            (lockingCard as SC_Submission).ApplyBodyPartDamage ();

        }

    }

}

