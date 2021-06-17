using System.Collections;
using static SC_Global;

namespace Card {

    public class SC_MaintainPressure : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            (lockingCard as SC_Submission).ApplyBodyPartDamage ();

            if ((lockingCard as SC_Submission).cost.bodyPartDamage.bodyPart != BodyPart.None)
                Caller.ApplySingleBodyEffect ((lockingCard as SC_Submission).cost.bodyPartDamage.bodyPart, (lockingCard as SC_Submission).cost.bodyPartDamage.damage);

        }

    }

}

