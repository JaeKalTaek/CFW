using UnityEngine;
using static SC_Global;
using static Card.SC_BaseCard;
using System;
using Card;

public class SC_CardGrabber : SC_CardMatcher {

    /*//Super type
    public enum Supertype { Any, Attack, OffensiveMove }

    public Supertype superTypeQuery;

    //Type
    [Serializable]
    public struct TypeQuery {

        public bool isOfType;

        public CardType type;

    }

    public TypeQuery[] typeQueries;    

    //Common effects
    public CommonEffectType[] commonEffectQueries;

    public bool Matching (SC_BaseCard card) {

        if (superTypeQuery == Supertype.Attack && !(card as SC_AttackCard))
            return false;
        else if (superTypeQuery == Supertype.OffensiveMove && !(card as SC_OffensiveMove))
            return false;

        foreach (TypeQuery tq in typeQueries)
            if (card.Is (tq.type) != tq.isOfType)
                return false;               

        foreach (CommonEffectType cet in commonEffectQueries)
            if (!card.Has (cet))
                return false;

        return true;

    } */

}
