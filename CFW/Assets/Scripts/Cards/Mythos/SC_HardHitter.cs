namespace Card {

    public class SC_HardHitter : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Strike))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 1;

        }

    }

}
