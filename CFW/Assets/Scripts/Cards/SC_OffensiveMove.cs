using System;
using UnityEngine;
using static SC_Global;

public class SC_OffensiveMove : SC_AttackCard {

    [Header("Offensive Move Variables")]
    [Tooltip("Effects of this offensive move on you")]
    public SelfEffect effectOnYou;

    [Tooltip("Effects of this offensive move on your opponent")]
    public Effect effectOnOpponent;

    [Serializable]
    public struct SelfEffect {

        public int stamina;

        public int health;

        public SelfBodyPartDamage bodyPartDamage;

    }

    [Serializable]
    public struct Effect {

        public int stamina;

        public int health;

        public BodyPartDamage bodyPartDamage;

    }

}
