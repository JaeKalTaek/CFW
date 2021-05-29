namespace Card {

    public class SC_IrishWhip : SC_BaseCard {

        public override void Boost () {

            base.Boost ();

            (respondedCards.Peek () as SC_OffensiveMove).effectOnOpponent.health += 2;

        }

    }

}
