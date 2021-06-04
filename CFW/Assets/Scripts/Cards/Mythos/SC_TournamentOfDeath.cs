namespace Card {

    public class SC_TournamentOfDeath : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Hardcore))
                Counters++;

        }

        public override void OnRingUseCounters (int counters = -1) {

            base.OnRingUseCounters (3);

        }

        protected override bool RingPlay () {

            Counters -= 3;

            activeCard = this;

            SetCurrentEffect (new CommonEffect (CommonEffectType.Draw));

            StartCoroutine (ApplyEffect (Draw));

            StartCoroutine (ClickedEffect ());

            return true;

        }

    }

}
