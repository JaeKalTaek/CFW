namespace Card {

    public class SC_ParadiseLock : SC_Submission {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Strike)) {

                c.commonEffects.Insert (0, new CommonEffect (CommonEffectType.Break));

                (c as SC_OffensiveMove).effectModifiers.stamina += add ? 1 : -1;

            }

        }

        public override void Broken () {

            base.Broken ();

            AddRemoveModifier (false);

        }

    }

}

