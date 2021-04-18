using System;

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

        Aerial, Classic, Hardcore, Strike

    }

    public enum ShiFuMi {

        None, Rock, Paper, Scissors

    }

    public static bool Win (ShiFuMi a, ShiFuMi b) {

        return ((a == ShiFuMi.Paper) && (b == ShiFuMi.Rock)) ||
            ((a == ShiFuMi.Rock) && (b == ShiFuMi.Scissors)) ||
            ((a == ShiFuMi.Scissors) && (b == ShiFuMi.Paper));

    }    

}
