using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

    [HideInInspector]
    [SyncVar]
    public string deckName;

	public SC_Deck Deck { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static List<SC_Player> players;

    public static SC_Player localPlayer, otherPlayer;

    #region Setup
    void Awake () {

        if ((players == null) || (players.Count >= 2))
            players = new List<SC_Player>();

        players.Add(this);

    }

    [SyncVar]
    bool setup;
        
    bool setupDeck;

    void Update () {
        
        if((players.Count == 2) && !setup && isLocalPlayer) {

            CmdSetup();

            localPlayer = this;

            Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + deckName), transform);

            GM.localDeckSize.text = Deck.Size.ToString();

            foreach (SC_Player p in players)
                if (p != this)
                    otherPlayer = p;

            otherPlayer.Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + otherPlayer.deckName), transform);

            GM.otherDeckSize.text = otherPlayer.Deck.Size.ToString();

        }

        if(isLocalPlayer && setup && otherPlayer.setup && !setupDeck) {

            setupDeck = true;

            Deck.Shuffle();

            CmdSetupDeck();

        }

    }
    #endregion

    #region Commands
    [Command]
    void CmdSetup() {

        setup = true;

    }

    [Command]
    void CmdSetupDeck() {

        RpcSetupDeck();

    }

    [ClientRpc]
    void RpcSetupDeck() {

        Deck.Setup(isLocalPlayer);

    }

    [Command]
    public void CmdShuffleDeck(int[] newOrder) {

        RpcShuffleDeck(newOrder);

    }

    [ClientRpc]
    void RpcShuffleDeck(int[] newOrder) {

        Deck.Shuffle(newOrder);

    }
    #endregion

}
