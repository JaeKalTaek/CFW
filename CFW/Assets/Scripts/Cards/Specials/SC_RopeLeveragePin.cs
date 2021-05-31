namespace Card {

    public class SC_RopeLeveragePin : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            if (boosting)
                return base.CanUse (user, ignorePriority, ignoreLocks);
            else {

                commonEffects.RemoveAt (0);

                bool b = base.CanUse (user, ignorePriority, true) && SC_Player.otherPlayer.Pinned;

                commonEffects.Insert (0, new CommonEffect (CommonEffectType.Boost));

                return b;

            }

        }

    }

}
