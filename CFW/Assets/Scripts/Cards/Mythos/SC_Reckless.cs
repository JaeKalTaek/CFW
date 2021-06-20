namespace Card {

    public class SC_Reckless : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Aerial))
                (c as SC_OffensiveMove).effectModifiers.health += (c as SC_OffensiveMove).cost.health * (add ? 1 : -1);

        }

    }

}
