using UnityEngine;

namespace Card {

    public class SC_StrongStyle : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Strike))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 1;
            else if (GM.MatchHeat >= 15 && activeCard.Receiver == Caller && (activeCard as SC_OffensiveMove))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health = Mathf.Max (1, (activeCard as SC_OffensiveMove).effectOnOpponent.health - 1);

        }

    }

}
