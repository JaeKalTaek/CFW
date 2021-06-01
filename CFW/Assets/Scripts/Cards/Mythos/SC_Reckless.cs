namespace Card {

    public class SC_Reckless : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Aerial))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += (activeCard as SC_OffensiveMove).cost.health;

        }

    }

}
