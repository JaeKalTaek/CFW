using System.Collections;

namespace Card {

    public class SC_StuckInATrashCan : SC_BaseCard {

        public OnCardHovered cardHovered;

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            cardHovered = new OnCardHovered ((c, b) => {

                /*if (c.Is (SC_Global.CardType.Strike))
                    c.commonEffects[0] = new CommonEffect (CommonEffectType.Break);*/

            });

            OnCardHoveredEvent += cardHovered;

        }

        public override void Broken () {

            base.Broken ();

            OnCardHoveredEvent -= cardHovered;

        }

    }

}

