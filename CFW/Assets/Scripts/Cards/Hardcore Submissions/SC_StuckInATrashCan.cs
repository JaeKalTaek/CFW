using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_StuckInATrashCan : SC_Submission {

        public OnCardHovered cardHovered;

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            boostingCard = this;

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

                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 2;

                base.ApplyBoosts ();

            }

        }

        public override void Broken () {

            base.Broken ();

            boostingCard = boostingCard == this ? null : boostingCard;

            OnCardHoveredEvent -= cardHovered;

        }

    }

}

