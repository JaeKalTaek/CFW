using UnityEngine;
using static SC_Global;

public class SC_BaseCard : MonoBehaviour {

    [Header("Base Card Variables")]
    [Tooltip("The minimum Match Heat required to play this card")]
    public int matchHeat;

    [Tooltip("Types of this card")]
    public CardType[] types;

    public string Path {

        get {

            string s = TypeToString(0);

            for (int i = 1; i < types.Length; i++)
                s += " " + TypeToString(i);

            return s + "/" + name;

        }
    }

    string TypeToString(int i) {

        return types[i].ToString() + (types.Length == i + 1 ? "s" : "");

    }

}
