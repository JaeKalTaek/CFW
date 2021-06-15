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

        Aerial, Classic, Hardcore, Strike, Basic, Submission, Special, Mytho

    }

    public enum Locked {

        Unlocked, Pinned, Submission

    }

    public enum ShiFuMi {

        None, Rock, Paper, Scissors

    }

    public enum ChoosingCard {

        None, Discard, Assess, DoubleDiscard, Play

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

        { "DoubleDiscard2", "Choose a 2nd card to discard" },

        { "ResponseCan", "You may respond to this card for now!" },

        { "ResponseCant", "You have no response to this card right now" },

        { "Responding", "Your opponent is responding to your play" },

        { "Boost", "You can boost this card" },

        { "Play", "Choose an offensive move to play" },

        { "NoGrabbing", "There was nothing to grab" },

        { "DestroyBackstory", "Choose a mythos card on the ring to discard" }

    };

    public static Dictionary<string, string> KeywordReminders = new Dictionary<string, string> () {

        { "Assess", "Assess: Discard a card of your choice to draw one from your deck." },

        { "Tire", "Tire: Lower the target player’s stamina to 2." },

        { "Break", "Break: Stop the count if it’s ongoing. Discard the locking card if there is one. Both players are not locked anymore." },

        { "Rest", "Rest: Gain 1 health and 1 stamina." },

        { "DoubleTap", "Double Tap: After this card has been played, you may play a copy of it by discarding 2 cards." },

        { "Exchange", "Exchange: After this card has been played, your opponent can play a copy of it as part of your turn." },

        { "Chain", "Chain: After this card has been played, it can be copied and played X times in a row." },

        { "Response", "Response: A card that can be played in response to an opponent playing a card fulfilling the following condition(s). Attack cards with this can also be played normally whereas special cards with this can only be played as a response." },

        { "Counter", "Counter: A card that can be played in response to an opponent playing a card fulfilling the following condition(s). The responded card has all of its effects (but not costs) negated. Attack cards with this can also be played normally whereas special cards with this can only be played as a response." },

        { "Boost", "Boost: You can only play this card as you play your first attack fulfilling the following condition(s) during your turn. It can bypass the limit of 1 special card played per turn." },

        { "Grab", "Grab: Search your deck (and/or other zones if specified) for X cards fulfilling the following condition(s) and put it/them in your hand." },

        { "Face", "Face: You are face if you have an alignment of 1 or more." },

        { "Neutral", "Neutral: You are neutral if you have an alignment of 0." },

        { "Heel", "Heel: You are heel if you have an alignment of -1 or less." },

        { "HeelCard", "Heel card: It is a card that requires to be heel (but not neutral) to be played." },

        { "FaceCard", "Face card: It is a card that requires to be face (but not neutral) to be played." },

        { "Finisher", "Finisher: Set the match heat to 20." },

        { "Unblockable", "Unblockable: A card that can not be responded to." },

        { "Count", "Count: When the count reaches 3, the player who started it wins the game." },

        { "Locked", "Locked: If there is an ongoing count, because of an active pinfall or submission, both players are locked." },

        { "StartPin", "Play Pinfall: Create and play the basic card \"Pinfall\"." },

        { "StartPinfall", "Start Pinfall: Start the count, this card becomes a “locking card”, and both players are now locked." }

    };

    public static void DebugWithTime (object s) {

        Debug.Log (s + ": " + DateTime.Now + ":" + DateTime.Now.Millisecond);

    }

}
