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
                    (grabbed as SC_OffensiveMove).effectOnOpponent *= 2;
                else if (grabbed as SC_Submission)
                    (grabbed as SC_Submission).effect *= 2;

                SC_Player.OnNewTurn += RemoveEffect;

            }

        }

        void RemoveEffect () {

            grabbed.cost /= 2;

            if (grabbed as SC_OffensiveMove)
                (grabbed as SC_OffensiveMove).effectOnOpponent /= 2;
            else if (grabbed != lockingCard && grabbed as SC_Submission)
                (grabbed as SC_Submission).effect /= 2;

            SC_Player.OnNewTurn -= RemoveEffect;

        }

    }

}
