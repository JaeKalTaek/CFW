using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Resilience : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            SC_OffensiveMove c = respondedCards.Peek () as SC_OffensiveMove;

            c.effectModifiers.bodyPartDamage.damage -= 2;
            c.effectModifiers.health -= 2;

            c.UpdateValuesUI ();

        }

    }

}
