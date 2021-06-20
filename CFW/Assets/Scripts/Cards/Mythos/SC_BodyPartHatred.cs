namespace Card {

    public class SC_BodyPartHatred : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Classic))
                (c as SC_OffensiveMove).effectModifiers.bodyPartDamage.damage += add ? 1 : -1;

        }

    }

}
