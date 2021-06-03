using System.Collections;
using static Card.SC_AttackCard;

namespace Card {

    public class SC_LuchadorMasks : SC_BaseCard {

        HealthCostModifier m;

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            m = (c, p) => { return p == Caller && c.Is (SC_Global.CardType.Aerial) ? 1 : 0; };

            healthCostModifiers.Add (m);

        }

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Aerial))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 2;

        }

        public override void DiscardedFromRing () {

            base.DiscardedFromRing ();

            healthCostModifiers.Remove (m);

        }

    }

}

