namespace Card {

    public class SC_LegendaryRivalry : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard as SC_AttackCard)
                Counters++;

        }

        public override void OnRingUseCounters (int counters = -1) {

            base.OnRingUseCounters (10);

        }

        protected override bool RingPlay () {

            UICard.ToGraveyard (1, () => {

                SetCurrentEffect (new CommonEffect (CommonEffectType.Grab));

                GrabPerform ();

                StartCoroutine (ClickedEffect ());

            }, false);

            return true;

        }

    }

}
