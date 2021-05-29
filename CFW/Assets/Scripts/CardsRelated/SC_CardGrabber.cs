using UnityEngine;
using static SC_Global;
using static Card.SC_BaseCard;
using System;
using Card;

public class SC_CardGrabber : SC_CardMatcher {

    [Header("WHERE TO GRAB FROM")]
    public bool deck;

    public bool discard, otherDiscard;

    [Header ("ADDITIONAL QUERIES")]
    public bool finisher;

    public bool heel, face;

    public int maxMatchHeat;

    public BodyPart bodyPartTarget;

    public int moveOfDoom;

    public override bool Matching (SC_BaseCard card) {

        if (maxMatchHeat > 0 && card.matchHeat > maxMatchHeat)
            return false;

        if (finisher && (!(card as SC_AttackCard) || !(card as SC_AttackCard).finisher))
                return false;

        if (moveOfDoom > 0 && (!(card as SC_OffensiveMove) || (card as SC_OffensiveMove).moveOfDoom != moveOfDoom))
            return false;

        if (heel || face) {

            bool has = false;

            foreach (CommonRequirement c in card.commonRequirements) {

                if (c.valueType == ValueName.Alignment && !c.opponent && ((heel && c.requirementType == RequirementType.Maximum && c.requirementValue <= 0) || (face && c.requirementType == RequirementType.Minimum && c.requirementValue >= 0)))
                    has = true;

            }

            if (!has)
                return false;

        }

        if (bodyPartTarget != BodyPart.None) {

            if (card as SC_AttackCard) {

                OffensiveBodyPartDamage obpd = (card as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (card as SC_Submission).effect.bodyPartDamage;

                if (obpd.bodyPart != bodyPartTarget && obpd.otherBodyPart != bodyPartTarget)
                    return false;

            } else
                return false;

        }

        return base.Matching (card);

    }

}
