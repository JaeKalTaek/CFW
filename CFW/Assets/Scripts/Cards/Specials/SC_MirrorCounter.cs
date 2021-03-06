using static SC_Player;

namespace Card {

    public class SC_MirrorCounter : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && (activeCard as SC_AttackCard).CanPayCost (localPlayer);

        }

        public override void CounterFinished () {

            interceptNext = true;

            Caller.CopyAndStartUsing (respondedCards.Peek () == null ? originalCard : respondedCards.Peek ());

        }

        protected override void InterceptFinish () {

            base.CounterFinished ();

        }

    }

}
