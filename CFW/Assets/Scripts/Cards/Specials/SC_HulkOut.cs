using System.Collections;

namespace Card {

    public class SC_HulkOut : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            modifierCards.Add (this);

            yield return StartCoroutine (base.ApplyEffects ());

        }

        public override void ApplyModifiers () {

            (activeCard as SC_OffensiveMove).effectOnOpponent /= 2;

            base.ApplyModifiers ();

        }

    }

}
