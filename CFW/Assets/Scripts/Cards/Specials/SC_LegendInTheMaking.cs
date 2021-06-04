namespace Card {

    public class SC_LegendInTheMaking : SC_BaseCard {

        protected override void OnNewTurnTriggered () {

            if (Caller.Turn) {

                Counters++;

                if (Counters >= 1) {

                    activeCard = this;

                    ApplyingEffects = true;

                    UICard.ToGraveyard (1, () => {

                        SetCurrentEffect (new CommonEffect (CommonEffectType.Grab, true));

                        GrabPerform ();                        

                    }, false);

                }

            }

        }

    }

}

