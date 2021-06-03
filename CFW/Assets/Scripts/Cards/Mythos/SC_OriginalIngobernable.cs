namespace Card {

    public class SC_OriginalIngobernable : SC_BaseCard {

        protected override void OnNoAttackTurnTriggered () {

            if (Caller.Turn)
                Rest ();

        }

    }

}
