namespace Card {

    public class SC_Janice : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if ((c as SC_OffensiveMove) && c.Is (SC_Global.CardType.Hardcore)) {

                (c as SC_OffensiveMove).effectModifiers.health += add ? 1 : -1;

                (c as SC_OffensiveMove).effectModifiers.bodyPartDamage.damage += add ? 1 : -1;

            }

        }

    }

}
