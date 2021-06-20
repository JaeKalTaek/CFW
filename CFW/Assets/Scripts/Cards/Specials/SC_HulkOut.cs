using System.Collections;

namespace Card {

    public class SC_HulkOut : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            (respondedCards.Peek () as SC_OffensiveMove).effect = (respondedCards.Peek () as SC_OffensiveMove).GetEffect / 2;

            respondedCards.Peek ().UpdateValuesUI ();

        }

    }

}
