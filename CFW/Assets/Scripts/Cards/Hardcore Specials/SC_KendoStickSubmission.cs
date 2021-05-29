namespace Card {

    public class SC_KendoStickSubmission : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            if (boosting)
                return base.CanUse (user, ignorePriority, ignoreLocks);
            else {

                commonEffects.Clear ();

                bool b = base.CanUse (user, ignorePriority, true) && SC_Player.otherPlayer.Submitted;

                commonEffects.Add (new CommonEffect (CommonEffectType.Boost));

                return b;

            }

        }

        public override void Boost () {

            base.Boost ();

            ((respondedCards.Count > 0 ? respondedCards.Peek () : lockingCard) as SC_Submission).effect.bodyPartDamage.damage += 2;

        }

    }

}
