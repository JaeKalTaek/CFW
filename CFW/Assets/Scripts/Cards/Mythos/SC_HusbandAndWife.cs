namespace Card {

    public class SC_HusbandAndWife : SC_BaseCard {

        protected override void OnNewTurnTriggered () {

            if (Caller.Turn && Caller.Face)
                Caller.ApplySingleEffect ("Health", 1);

        }

    }

}
