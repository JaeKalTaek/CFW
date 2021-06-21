using Card;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_DeckBuilder : MonoBehaviour {

    public RectTransform deck, cards;

    static List<RectTransform>[] deckCards;

    // Dictionary<SC_BaseCard, GameObject> 

    static SC_DeckBuilder Instance;

    void Start () {

        Instance = this;

        deckCards = new List<RectTransform>[20];

        for (int i = 0; i < 20; i++)
            deckCards[i] = new List<RectTransform> ();

        SC_CardGrabber grabber = GetComponent<SC_CardGrabber> ();

        Vector2 size = Resources.Load<RectTransform> ("Prefabs/P_UI_Card").sizeDelta * 1.5f;

        foreach (Transform t in cards)
            Destroy (t.gameObject);

        float marginsNbr = (((int) cards.rect.width) / ((int) size.x)) - 1;

        float margin = (cards.rect.width % size.x) / marginsNbr;

        int x = 0;
        int y = 0;

        List<SC_BaseCard> list = new List<SC_BaseCard> (Resources.LoadAll<SC_BaseCard> (""));

        list.RemoveAll ((c) => { return c.Is (SC_Global.CardType.Basic); });

        foreach (SC_BaseCard c in list) {

            if (grabber.Matching (c)) {

                RectTransform r = Instantiate (Resources.Load<RectTransform> ("Prefabs/P_DeckBuilder_SearchCard"), cards);

                r.sizeDelta = size;

                r.GetComponent<SC_DeckBuilder_SearchCard> ().Card = c;

                r.GetComponent<Image> ().sprite = Resources.Load<Sprite> (c.Path);

                r.anchoredPosition = new Vector2 (x * (size.x + margin), -y * (size.y + margin));                

                x = x == ((int) cards.rect.width) / ((int) size.x) - 1 ? 0 : x + 1;

                y = x == 0 ? y + 1 : y;

            }

        }

        y += (x == 0 ? -1 : 0) + 1;

        cards.sizeDelta = new Vector2 (cards.sizeDelta.x, Mathf.Max (960, size.y * y + margin * (y - 1)));

    }

    public static void AddCard (SC_BaseCard c) {

        RectTransform r = Instantiate (Resources.Load<RectTransform> ("Prefabs/P_DeckBuilder_DeckCard"), Instance.deck);

        r.anchorMin = new Vector2 ((c.matchHeat - 1) * 0.05f, 0);

        r.anchorMax = new Vector2 ((c.matchHeat + 1) * 0.05f, 1);

        r.pivot = new Vector2 (.5f, 1);

        r.anchoredPosition = Vector2.down * 20 * deckCards[c.matchHeat - 1].Count;

        r.GetComponent<Image> ().sprite = Resources.Load<Sprite> (c.Path);

        deckCards[c.matchHeat - 1].Add (r);

        int index = 0;

        for (int i = 0; i < deckCards.Length; i++) {

            if (i == c.matchHeat - 1) {

                index += deckCards[i].Count - 1;

                break;

            }

            index += deckCards[i].Count;

        }

        r.SetSiblingIndex (index);        

    }

    public static void RemoveCard (SC_BaseCard c) {

        int? removedIndex = null;

        foreach (RectTransform r in deckCards[c.matchHeat - 1]) {

            if (c.Path.Contains (r.GetComponent<Image> ().sprite.name)) {

                removedIndex = deckCards[c.matchHeat - 1].IndexOf (r);

                Destroy (r.gameObject);

            } else if (removedIndex != null)
                r.anchoredPosition += Vector2.up * 20;            

        }

        if (removedIndex != null)
            deckCards[c.matchHeat - 1].RemoveAt ((int)removedIndex);

    }

}
