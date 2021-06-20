using System.Collections;

namespace Card {

    public class SC_SuperFinisher : SC_BaseCard {

        SC_AttackCard grabbed;

        protected override IEnumerator ApplyEffects () {

            int handSize = Caller.Hand.Count;

            yield return StartCoroutine (base.ApplyEffects ());

            if (Caller.Hand.Count > handSize) {

                grabbed = Caller.Hand[Caller.Hand.Count - 1] as SC_AttackCard;

                grabbed.cost *= 2;

                if (grabbed as SC_OffensiveMove)
                    (grabbed as SC_OffensiveMove).effect *= 2;
                else if (grabbed as SC_Submission)
                    (grabbed as SC_Submission).effect *= 2;

                grabbed.UpdateValuesUI ();

                SC_Player.OnNewTurn += RemoveEffect;

            }

        }

        void RemoveEffect () {

            if (grabbed) {

                grabbed.cost /= 2;

                if (grabbed as SC_OffensiveMove)
                    (grabbed as SC_OffensiveMove).effect /= 2;
                else if (grabbed != lockingCard && grabbed as SC_Submission)
                    (grabbed as SC_Submission).effect /= 2;

                grabbed.UpdateValuesUI ();

            }

            SC_Player.OnNewTurn -= RemoveEffect;

        }

    }

}
