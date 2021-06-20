namespace Card {

    public class SC_OnTheApron : SC_BaseCard {

        public override void Boost () {            

            (respondedCards.Peek () as SC_OffensiveMove).cost.health += 3;

            (respondedCards.Peek () as SC_OffensiveMove).effect.health += 3;

            base.Boost ();

        }

    }

}
