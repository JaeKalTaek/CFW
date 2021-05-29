namespace Card {

    public class SC_OnTheApron : SC_BaseCard {

        public override void Boost () {

            base.Boost ();

            (respondedCards.Peek () as SC_OffensiveMove).cost.health += 3;

            (respondedCards.Peek () as SC_OffensiveMove).effectOnOpponent.health += 3;

        }

    }

}
