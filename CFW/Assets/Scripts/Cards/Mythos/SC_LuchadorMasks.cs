using System.Collections;

namespace Card {

    public class SC_LuchadorMasks : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            SC_AttackCard.healthCostModifiers.Add ((c, p) => { return p == Caller && c.Is (SC_Global.CardType.Aerial) ? 1 : 0; });

        }

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Aerial))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 2;

        }

    }

}

