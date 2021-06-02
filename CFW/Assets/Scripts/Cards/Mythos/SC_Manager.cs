namespace Card {

    public class SC_Manager : SC_BaseCard {

        protected override void OnNewTurnTriggered () {

            if (Caller.Turn && !Caller.Neutral)
                GM.AddMatchHeat (1);

        }

    }

}
