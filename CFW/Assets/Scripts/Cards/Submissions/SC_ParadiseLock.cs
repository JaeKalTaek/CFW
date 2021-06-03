using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_ParadiseLock : SC_Submission {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            cardHovered = new OnCardHovered ((c, b) => {

                if (localPlayer == Caller && !activeCard && c.Is (SC_Global.CardType.Strike)) {

                    if (b)
                        c.commonEffects.Insert (0, new CommonEffect (CommonEffectType.Break));
                    else
                        c.commonEffects.RemoveAt (0);

                }

            });

            OnCardHoveredEvent += cardHovered;

        }

        public override void ApplyModifiers () {            

            if (activeCard.Is (SC_Global.CardType.Strike)) {

                activeCard.commonEffects.Insert (0, new CommonEffect (CommonEffectType.Break));

                (activeCard as SC_OffensiveMove).effectOnOpponent.stamina += 1;                

            }            

        }

        public override void Broken () {

            base.Broken ();

            base.ApplyModifiers ();

            OnCardHoveredEvent -= cardHovered;

        }

    }

}

