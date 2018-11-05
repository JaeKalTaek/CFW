using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

    [HideInInspector]
    [SyncVar]
    public string deckName;

	public SC_Deck Deck { get; set; }

    SC_GameManager GM;

    static List<SC_Player> players;

    static SC_Player localPlayer, otherPlayer;

    void Awake () {

        if ((players == null) || (players.Count >= 2))
            players = new List<SC_Player>();

        players.Add(this);

    }

    bool setup;

    void Update () {
        
        if((players.Count == 2) && !setup && isLocalPlayer) {

            setup = true;

            localPlayer = this;

            Deck = Resources.Load<SC_Deck>("Decks/" + deckName);

            Deck.Shuffle();

            GM = SC_GameManager.Instance;

            GM.localDeckSize.text = Deck.Size.ToString();

            foreach (SC_Player p in players)
                if (p != this)
                    otherPlayer = p;

            otherPlayer.Deck = Resources.Load<SC_Deck>("Decks/" + otherPlayer.deckName);

            GM.otherDeckSize.text = otherPlayer.Deck.Size.ToString();

        }

    }

}
