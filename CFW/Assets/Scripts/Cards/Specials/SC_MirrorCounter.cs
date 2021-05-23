using static SC_Player;

namespace Card {

    public class SC_MirrorCounter : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && (activeCard as SC_AttackCard).CanPayCost (localPlayer);

        }

        bool intercept;

        public override void CounterFinished () {

            if (!intercept) {

                intercept = true;

                if (Caller.IsLocalPlayer)
                    Caller.MirrorCounterServerRpc ();

            } else
                base.CounterFinished ();

        }

    }

}
