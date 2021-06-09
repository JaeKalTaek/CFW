using Card;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (SC_CardSearcher), true)]
public class SC_CardSearcherEditor : Editor {

    public override void OnInspectorGUI () {

        base.OnInspectorGUI ();

        SC_CardGrabber cardGrabber = (target as SC_CardSearcher).GetComponent<SC_CardGrabber> ();

        if (GUILayout.Button ("Search")) {

            foreach (SC_BaseCard c in Resources.LoadAll<SC_BaseCard> ("")) {

                if (cardGrabber.Matching (c))
                    Debug.Log (c.name);

            }

        }

    }

}
