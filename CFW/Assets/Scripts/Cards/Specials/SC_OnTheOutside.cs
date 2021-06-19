namespace Card {

    public class SC_OnTheOutside : SC_BaseCard {

        public override void Boost () {

            (respondedCards.Peek () as SC_OffensiveMove).cost.health += 4;

            (respondedCards.Peek () as SC_OffensiveMove).effectOnOpponent.health += 4;

            base.Boost ();

        }

    }

}
