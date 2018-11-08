using System;

public class SC_Global {

    [Serializable]
    public struct SelfBodyPartDamage {

        public BodyPart bodyPart;

        public int damage;

    }

    [Serializable]
    public struct BodyPartDamage {

        public BodyPart bodyPart;

        public BodyPart otherBodyPart;

        public bool both;

        public int damage;

    }

    public enum BodyPart {

        None, Legs, Ribs, Back, Arms, Neck

    }
	
    public enum CardType {

        Aerial, Classic, Hardcore, Mytho, Special, Strike, Submission

    }

}
