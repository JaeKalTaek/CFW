using Card;
using System.Collections.Generic;
using UnityEngine;

public class SC_Graveyard : SC_CardZone {

    public List<SC_BaseCard> Cards { get; set; }    

    public override List<SC_BaseCard> GetCards () {

        return Cards;

    }

    void Awake () {

        Cards = new List<SC_BaseCard> ();

    }

    public override SC_UI_Card CreateCard (SC_BaseCard original, RectTransform parent = null) {

        SC_UI_Card c = CreateCard (original, parent);

        Destroy (original.UICard.gameObject);

        return c;

    }

}
