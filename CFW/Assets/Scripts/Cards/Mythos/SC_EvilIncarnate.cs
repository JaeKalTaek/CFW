namespace Card {

    public class SC_EvilIncarnate : SC_BaseCard {

        protected override void OnPlayTriggered () {

            print (activeCard.Caller == Caller);

            if (activeCard.Caller == Caller && activeCard.IsAlignmentCard (false))
                Receiver.ApplySingleEffect ("Stamina", -1);

        }

    }

}
