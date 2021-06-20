using System.Collections;
using static SC_GameManager;

namespace Card {

    public class SC_StrongStyle : SC_BaseCard {

        OnMatchHeatChanged onMatchHeatChanged;

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            OnMatchHeatChangedEvent += onMatchHeatChanged = o => {

                if (GM.MatchHeat >= 15 && o < 15)
                    ApplyOtherModifiersToHand (true);
                else if (GM.MatchHeat < 15 && o >= 15)
                    ApplyOtherModifiersToHand (false);

            };

        }

        protected override void AddRemoveModifier (bool add) {

            base.AddRemoveModifier (add);

            if (GM.MatchHeat >= 15)
                ApplyOtherModifiersToHand (add);

        }

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Strike))
                (c as SC_OffensiveMove).effectModifiers.health += add ? 1 : -1;

        }

        void ApplyOtherModifiersToHand (bool add) {

            foreach (SC_BaseCard c in GetCardsToModify ((Caller.IsLocalPlayer ? SC_Player.otherPlayer : SC_Player.localPlayer).Hand)) {

                if (c as SC_OffensiveMove)
                    (c as SC_OffensiveMove).effectModifiers.health += add ? -1 : 1;

                c.UpdateValuesUI ();

            }

        }

        public override void DiscardedFromRing () {

            OnMatchHeatChangedEvent -= onMatchHeatChanged;

            base.DiscardedFromRing ();            

        }

    }

}
