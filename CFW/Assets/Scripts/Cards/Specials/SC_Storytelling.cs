using static SC_Player;

namespace Card {

    public class SC_Storytelling : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return (localPlayer.Graveyard.Cards.Count + otherPlayer.Graveyard.Cards.Count) >= 15 && base.CanUse (user, ignorePriority, ignoreLocks);

        }

    }

}
