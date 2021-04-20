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

            if (oM.cost.bodyPartDamage.bodyPart == SC_Global.BodyPart.None)
                oM.cost.bodyPartDamage.damage = 0;

            SC_Global.OffensiveBodyPartDamage oBPD = oM.effectOnOpponent.bodyPartDamage;

            if (oBPD.bodyPart == SC_Global.BodyPart.None) {

                oBPD.otherBodyPart = SC_Global.BodyPart.None;

                oBPD.damage = 0;

                oBPD.both = false;

            }

            if (oBPD.otherBodyPart == SC_Global.BodyPart.None)
                oBPD.both = false;

        }

    }

}
