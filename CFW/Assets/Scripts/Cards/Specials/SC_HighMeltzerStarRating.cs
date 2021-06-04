namespace Card {

    public class SC_HighMeltzerStarRating : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.IsResponse || ((activeCard as SC_AttackCard)?.finisher ?? false))
                Counters += 3;

        }

        public override void OnRingUseCounters (int counters = -1) {

            base.OnRingUseCounters (1);

        }

        protected override bool RingPlay () {

            GetComponent<SC_CardGrabber> ().maxMatchHeat = Counters;

            UICard.ToGraveyard (1, () => {

                SetCurrentEffect (new CommonEffect (CommonEffectType.Grab));

                GrabPerform ();

                StartCoroutine (ClickedEffect ());

            }, false);

            return true;

        }

    }

}
