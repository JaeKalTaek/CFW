using System.Collections;

namespace Card {

    public class SC_HulkOut : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            (respondedCards.Peek () as SC_OffensiveMove).effectOnOpponent /= 2;

            respondedCards.Peek ().UpdateValuesUI ();

        }

    }

}
