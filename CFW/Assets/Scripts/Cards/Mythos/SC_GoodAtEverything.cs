namespace Card {

    public class SC_GoodAtEverything : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Classic))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 1;

        }

    }

}
