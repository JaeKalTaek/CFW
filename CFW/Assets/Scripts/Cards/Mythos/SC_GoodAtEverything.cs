namespace Card {

    public class SC_GoodAtEverything : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Classic))
                (c as SC_OffensiveMove).effectModifiers.health += add ? 1 : -1;

        }

    }

}
