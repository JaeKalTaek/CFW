using UnityEngine;

namespace Card {

    public class SC_AttackCard : SC_BaseCard {

        [Header("Attack Card Variables")]
        [Tooltip("Match Heat gained when using this card (irrelevent if Finisher)")]
        public int matchHeatGain;

        [Tooltip("Is this card a Finisher ?")]
        public bool finisher;

        public override void Use (SC_Player caller) {

            base.Use (caller);

            GM.AddMatchHeat (matchHeatGain);

        }
    }

}
