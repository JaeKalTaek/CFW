using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Player : NetworkBehaviour {

    [HideInInspector]
    [SyncVar]
    public string deckName;

	public SC_Deck Deck { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static List<SC_Player> players;

    public static SC_Player localPlayer, otherPlayer;

    public ShiFuMi ShifumiChoice { get; set; }

    public bool Turn { get; set; }

    #region Setup
    void Awake () {

        if ((players == null) || (players.Count >= 2))
            players = new List<SC_Player>();

        players.Add(this);

    }

    [SyncVar]
    bool setup;
        
    bool setupDeck, finishSetup;

    void Update () {

        if (isLocalPlayer) {

            if ((players.Count == 2) && !setup) {

                setup = true;

                CmdSetup();

                localPlayer = this;

                Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + deckName), transform);

                GM.localDeckSize.text = Deck.Size.ToString();

                foreach (SC_Player p in players)
                    if (p != this)
                        otherPlayer = p;

                otherPlayer.Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + otherPlayer.deckName), otherPlayer.transform);

                GM.otherDeckSize.text = otherPlayer.Deck.Size.ToString();

            }

            if (setup && otherPlayer.setup && !setupDeck) {

                setupDeck = true;

                Deck.Shuffle();

                CmdSetupDeck();

            }

            if (setupDeck && otherPlayer.setupDeck && !finishSetup) {

                finishSetup = true;

                GM.connectingPanel.SetActive(false);

            }

        }

    }
    #endregion

    #region Commands

    #region Setup
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

        setupDeck = true;

        Deck.Setup(isLocalPlayer);

    }
    #endregion

    #region Deck
    [Command]
    public void CmdShuffleDeck(int[] newOrder) {

        RpcShuffleDeck(newOrder);

    }

    [ClientRpc]
    void RpcShuffleDeck(int[] newOrder) {

        Deck.Shuffle(newOrder);

    }

    [Command]
    public void CmdDraw(int nbr) {

        RpcDraw(nbr);

    }

    [ClientRpc]
    void RpcDraw(int nbr) {

        Deck.Draw(nbr, isLocalPlayer);

    }
    #endregion

    #region ShiFuMi
    [Command]
    public void CmdShiFuMiChoice(int s) {

        RpcShiFuMiChoice(s);

    }

    [ClientRpc]
    void RpcShiFuMiChoice(int s) {

        ShifumiChoice = (ShiFuMi)s;

        if (isLocalPlayer) {

            if (otherPlayer.ShifumiChoice != ShiFuMi.None) {

                if (Win(ShifumiChoice, otherPlayer.ShifumiChoice)) {

                    CmdStartTurn(true);

                } else if (ShifumiChoice == otherPlayer.ShifumiChoice) {

                    CmdResetShiFuMi();                        

                } else {

                    CmdStartTurn(false);

                }

            } else {

                GM.shifumiText.text = "Waiting for other player to choose...";

            }

        }

    }

    [Command]
    void CmdResetShiFuMi () {

        RpcResetShiFuMi();

    }

    [ClientRpc]
    void RpcResetShiFuMi () {

        ShifumiChoice = ShiFuMi.None;

        otherPlayer.ShifumiChoice = ShiFuMi.None;

        SC_ShiFuMiChoice.Draw();

    }

    /*void ShiFuMiResult(bool won) {

        GM.shifumiPanel.SetActive(false);

        GM.ShowTurnPanel(won);

        CmdStartTurn(!won);

    }*/
    #endregion

    #region Start Turn
    [Command]
    void CmdStartTurn (bool won) {        

        RpcStartTurn(won);

    }

    [ClientRpc]
    void RpcStartTurn (bool won) {

        GM.shifumiPanel.SetActive(false);

        GM.ShowTurnPanel(isLocalPlayer ? won : !won);

    }    

    [Command]
    public void CmdSetStartTurn(bool start) {

        RpcSetStartTurn(start);

    }

    [ClientRpc]
    void RpcSetStartTurn(bool start) {

        GM.StartGame();

        (isLocalPlayer ? this : otherPlayer).Turn = start;

        (isLocalPlayer ? otherPlayer : this).Turn = !start;

    }
    #endregion

    #endregion
}
