using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_ParadiseLock : SC_Submission {

        public OnCardHovered cardHovered;

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            boostingCards.Add (this);

            cardHovered = new OnCardHovered ((c, b) => {

                if (localPlayer.Turn && localPlayer.Unlocked && c.Is (SC_Global.CardType.Strike)) {

                    if (b)
                        c.commonEffects.Add (new CommonEffect (CommonEffectType.Break));
                    else
                        c.commonEffects.RemoveAt (c.commonEffects.Count - 1);

                }

            });

            OnCardHoveredEvent += cardHovered;

        }

        public override void ApplyBoosts () {            

            if (activeCard.Is (SC_Global.CardType.Strike)) {

                activeCard.commonEffects.Add (new CommonEffect (CommonEffectType.Break));

                (activeCard as SC_OffensiveMove).effectOnOpponent.stamina += 1;

                base.ApplyBoosts ();

            }

        }

        public override void Broken () {

            base.Broken ();

            boostingCards.Remove (this);

            OnCardHoveredEvent -= cardHovered;

        }

    }

}

