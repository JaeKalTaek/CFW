namespace Card {

    public class SC_JAndJSecurity : SC_BaseCard {

        protected override void OnNewTurnTriggered () {

            if (Caller.Turn && Caller.Heel)
                Receiver.ApplySingleEffect ("Health", -1);

        }

    }

}
