using UnityEditor;
using UnityEngine;
using Card;

[CustomEditor(typeof(SC_BaseCard), true), CanEditMultipleObjects]
public class SC_CardEditor : Editor {

    public override void OnInspectorGUI () {

        DrawDefaultInspector();

        SC_BaseCard bC = target as SC_BaseCard;

        bC.matchHeat = Mathf.Clamp(bC.matchHeat, 1, 20);

        SC_AttackCard aC = bC as SC_AttackCard;

        if (aC)
            aC.matchHeatGain = aC.finisher ? 0 : Mathf.Max(aC.matchHeatGain, 0);

        SC_OffensiveMove oM = bC as SC_OffensiveMove;

        if(oM) {

            if (oM.effectOnYou.bodyPartDamage.bodyPart == SC_Global.BodyPart.None)
                oM.effectOnYou.bodyPartDamage.damage = 0;

            if (oM.effectOnOpponent.bodyPartDamage.bodyPart == SC_Global.BodyPart.None) {

                oM.effectOnOpponent.bodyPartDamage.otherBodyPart = SC_Global.BodyPart.None;

                oM.effectOnOpponent.bodyPartDamage.damage = 0;

                oM.effectOnOpponent.bodyPartDamage.both = false;

            }

            if (oM.effectOnOpponent.bodyPartDamage.otherBodyPart == SC_Global.BodyPart.None)
                oM.effectOnOpponent.bodyPartDamage.both = false;

        }

    }

}
