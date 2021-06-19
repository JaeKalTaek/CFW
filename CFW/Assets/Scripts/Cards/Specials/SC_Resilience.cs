using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Resilience : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            SC_OffensiveMove c = respondedCards.Peek () as SC_OffensiveMove;

            c.effectOnOpponent.bodyPartDamage.damage = Mathf.Max (1, c.effectOnOpponent.bodyPartDamage.damage - 2);
            c.effectOnOpponent.health = Mathf.Max (1, c.effectOnOpponent.health - 2);

            c.UpdateValuesUI ();

        }

    }

}
