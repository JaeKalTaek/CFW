using System;
using System.Collections.Generic;
using UnityEngine;

public class SC_Global {

    [Serializable]
    public struct BodyPartDamage {

        public BodyPart bodyPart;

        public int damage;

    }

    [Serializable]
    public struct OffensiveBodyPartDamage {

        public BodyPart bodyPart, otherBodyPart;

        public bool both;

        public int damage;

    }

    public enum BodyPart {

        None, Legs, Ribs, Back, Arms, Neck

    }

    public enum CardType {

        Aerial, Classic, Hardcore, Strike, Basic, Submission, Special

    }

    public enum Locked {

        Unlocked, Pinned, Submission

    }

    public enum ShiFuMi {

        None, Rock, Paper, Scissors

    }

    public enum ChoosingCard {

        None, Discarding, Assessing, DoubleDiscard

    }

    public static bool Win (ShiFuMi a, ShiFuMi b) {

        return ((a == ShiFuMi.Paper) && (b == ShiFuMi.Rock)) ||
            ((a == ShiFuMi.Rock) && (b == ShiFuMi.Scissors)) ||
            ((a == ShiFuMi.Scissors) && (b == ShiFuMi.Paper));

    }

    public static Dictionary<string, string> Messages = new Dictionary<string, string> () {

        { "Assess", "Choose which card to discard to assess" },
        
        { "Discard", "Choose which card to discard" },

        { "BodyPartDamage", "Which body part do you want to damage?" },

        { "BodyPartHeal", "Which body part do you want to heal?" },

        { "KnowYourOpponent", "Type a card name and press confirm" },

        { "DoubleDiscard", "Choose a 1st card to discard" },

        { "DoubleDiscard2", "Choose a 2nd card to discard" }

    };

    public static void DebugWithTime (string s) {

        Debug.Log (s + ": " + DateTime.Now + ":" + DateTime.Now.Millisecond);

    }

}
