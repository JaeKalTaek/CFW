using Card;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (SC_CardSearcher), true)]
public class SC_CardSearcherEditor : Editor {

    public override void OnInspectorGUI () {

        base.OnInspectorGUI ();

        SC_MatchingCard cardMatcher = (target as SC_CardSearcher).GetComponent<SC_MatchingCard> ();

        if (GUILayout.Button ("Search")) {

            foreach (SC_BaseCard c in Resources.LoadAll<SC_BaseCard> ("")) {

                if (cardMatcher.Matching (c))
                    Debug.Log (c.name);

            }

        }

    }

}
