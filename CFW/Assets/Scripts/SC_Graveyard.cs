using Card;
using System.Collections.Generic;
using UnityEngine;

public class SC_Graveyard : MonoBehaviour {

    public List<SC_BaseCard> Cards { get; set; }

    public RectTransform RecT { get { return GetComponent<RectTransform>(); } }

}
