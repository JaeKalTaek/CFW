namespace Card {

    public class SC_OsakaJoHall : SC_BaseCard {

        protected override void OnNewTurnTriggered () {

            if (GM.MatchHeat >= 15)
                (Caller.Turn ? Caller : Receiver).ApplySingleEffect ("Stamina", 1);

        }

    }

}
