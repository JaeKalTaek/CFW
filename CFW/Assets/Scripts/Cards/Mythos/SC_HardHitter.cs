namespace Card {

    public class SC_HardHitter : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Strike))
                (c as SC_OffensiveMove).effectModifiers.health += add ? 1 : -1;

        }

    }

}
