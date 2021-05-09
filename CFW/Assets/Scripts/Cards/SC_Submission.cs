using System;
using UnityEngine;
using static SC_Global;
using DG.Tweening;

namespace Card {

    public class SC_Submission : SC_AttackCard {

        [Header ("Submission Variables")]
        [Tooltip ("Effects of this submission on your opponent")]
        public Effect effect;

        [Serializable]
        public struct Effect {

            public int stamina;

            public OffensiveBodyPartDamage bodyPartDamage;

            public int breakCost, earlyBreakDamage;            

        }

        public override void ApplyEffect () {            

            base.ApplyEffect ();

            Other.ApplySingleEffect ("Stamina", -effect.stamina);

            ApplyBodyPartDamage ();

            Lock ();

        }

        public void Maintain () {

            UICard.RecT.SetAsLastSibling ();

            UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * GM.enlargeCardFactor, .5f).OnComplete (() => {

                ApplyBodyPartDamage ();                

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.enlargeCardFactor, .5f).OnComplete (() => {

                    UICard.RecT.SetAsFirstSibling ();

                    NextTurn ();

                });

            });               

        }

    }

}
