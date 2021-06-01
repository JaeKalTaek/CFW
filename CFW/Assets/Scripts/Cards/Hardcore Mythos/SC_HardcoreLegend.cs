using UnityEngine;

namespace Card {

    public class SC_HardcoreLegend : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Receiver == Caller && (activeCard as SC_OffensiveMove) && activeCard.Is (SC_Global.CardType.Hardcore))
                (activeCard as SC_OffensiveMove).effectOnOpponent.health = Mathf.Max (1, (activeCard as SC_OffensiveMove).effectOnOpponent.health - 1);            

        }

    }

}
