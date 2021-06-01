namespace Card {

    public class SC_Ace : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.Caller == Caller && activeCard.IsAlignmentCard (true))
                Caller.ApplySingleEffect ("Stamina", 1);

        }

    }

}
