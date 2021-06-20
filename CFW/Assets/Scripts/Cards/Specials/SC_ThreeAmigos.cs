namespace Card {

    public class SC_ThreeAmigos : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && activeCard.Path.Contains ("Suplex");

        }

        string boostedCardName;

        public override void Boost () {

            base.Boost ();

            modifierCards.Add (this);

            boostedCardName = respondedCards.Peek ().Path;

            SC_Player.OnNewTurn += () => modifierCards.Remove (this);

        }

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Path == boostedCardName)
                c.commonEffects.Add (new CommonEffect (CommonEffectType.Chain, v: 2));

        }

    }

}
