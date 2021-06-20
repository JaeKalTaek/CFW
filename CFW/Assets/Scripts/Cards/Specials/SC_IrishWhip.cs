namespace Card {

    public class SC_IrishWhip : SC_BaseCard {

        public override void Boost () {

            (respondedCards.Peek () as SC_OffensiveMove).effect.health += 2;

            base.Boost ();

        }

    }

}
