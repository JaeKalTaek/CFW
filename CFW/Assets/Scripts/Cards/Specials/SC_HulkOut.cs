using System.Collections;

namespace Card {

    public class SC_HulkOut : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            modifierCards.Add (this);

            yield return StartCoroutine (base.ApplyEffects ());

        }

        public override void ApplyModifiers () {

            SC_OffensiveMove c = activeCard as SC_OffensiveMove;

            c.effectOnOpponent.bodyPartDamage.damage /= 2;
            c.effectOnOpponent.health /= 2;
            c.effectOnOpponent.stamina /= 2;

            base.ApplyModifiers ();

        }

    }

}
