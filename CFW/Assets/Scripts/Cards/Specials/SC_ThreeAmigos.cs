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

            OnBaseFinishedUsing += base.ApplyModifiers;

        }

        public override void ApplyModifiers () {

            if (activeCard.Has (CommonEffectType.Counter))
                base.ApplyModifiers ();
            else if (activeCard.Path == boostedCardName && activeCard.Caller == Caller) 
                activeCard.commonEffects.Add (new CommonEffect (CommonEffectType.Chain, v: 2));

        }    

    }

}
