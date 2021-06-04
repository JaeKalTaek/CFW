namespace Card {

    public class SC_RefereeSupervision : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.Caller == Receiver && activeCard.IsAlignmentCard (false)) {

                activeCard.ApplyingEffects = true;

                UICard.ToGraveyard (1, () => {                    

                    activeCard.SetCurrentEffect (new CommonEffect (CommonEffectType.DiscardRandom));

                    activeCard.DiscardRandom ();

                }, false);                 

            }

        }

    }

}
