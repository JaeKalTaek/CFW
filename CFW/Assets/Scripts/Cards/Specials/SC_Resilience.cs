using System.Collections;
using Card;
using UnityEngine;

public class SC_Resilience : SC_BaseCard {

    protected override IEnumerator ApplyEffects () {

        modifierCards.Add (this);

        yield return StartCoroutine (base.ApplyEffects ());

    }

    public override void ApplyModifiers () {

        SC_OffensiveMove c = activeCard as SC_OffensiveMove;

        c.effectOnOpponent.bodyPartDamage.damage = Mathf.Max (1, c.effectOnOpponent.bodyPartDamage.damage - 2);
        c.effectOnOpponent.health = Mathf.Max (1, c.effectOnOpponent.health - 2);

        base.ApplyModifiers ();

    }

}
