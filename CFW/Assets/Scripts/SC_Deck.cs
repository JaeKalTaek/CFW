using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Deck : MonoBehaviour {

	public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public void Setup(bool local) {

        Draw(GM.startHandSize, local);

    }

    public void Draw(int nbr, bool local) {

        for (int i = 0; i < nbr; i++)
            Draw(local);

    }

    void Draw (bool local) {

        RectTransform rT = local ? GM.localHand : GM.otherHand;

        Image c = Instantiate(Resources.Load<GameObject>("UI/Card"), rT).GetComponent<Image>();

        if (!local) {

            Quaternion upsideDown = Quaternion.identity;
            upsideDown.eulerAngles = new Vector3(0, 0, 180);
            c.transform.localRotation = upsideDown;

        }

        c.name = cards[0].Path;

        c.sprite = Resources.Load<Sprite>(local ? cards[0].Path : "Sprites/CardBack");

        for (int i = 0; i < rT.childCount; i++)
            rT.GetChild(i).transform.localPosition = new Vector3((((rT.childCount - 1) / 2f) - i) * (rT.childCount % 2 == 0 ? 1 : -1) * (GM.cardWidth / 2), 130 * (local ? -1 : 1), 0);              

        cards.RemoveAt(0);

        (local ? GM.localDeckSize : GM.otherDeckSize).text = Size.ToString();

    }

    public void Shuffle() {

        int[] newOrder = new int[cards.Count];

        for (int i = 0; i < newOrder.Length; i++)
            newOrder[i] = i;

        newOrder.Shuffle();

        SC_Player.localPlayer.CmdShuffleDeck(newOrder);

    }

    public void Shuffle(int[] newOrder) {

        List<SC_BaseCard> oldCards = new List<SC_BaseCard>(cards);

        for (int i = 0; i < newOrder.Length; i++)
            cards[i] = oldCards[newOrder[i]];

    }

}
