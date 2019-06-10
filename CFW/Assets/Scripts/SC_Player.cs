using Card;
using System;
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

    public bool CanPlay { get; set; }

    public bool Busy { get; set; }

    public int Health { get; set; }

    public int Stamina { get; set; }

    public int Alignment { get; set; }

    public Dictionary<BodyPart, int> BodyPartsHealth;

    #region Setup
    void Start () {      

        if ((players == null) || (players.Count >= 2))
            players = new List<SC_Player>();

        players.Add(this);

    }

    [SyncVar]
    bool setup;
        
    bool setupDeck, finishSetup;

    void Update () {

        if (isLocalPlayer) {

            if ((players.Count == 2) && !setup && GM) {

                setup = true;

                SetupPlayerValues(this);

                CmdSetup();

                localPlayer = this;

                Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + deckName), GM.background);

                foreach (SC_Player p in players)
                    if (p != this)
                        otherPlayer = p;

                SetupPlayerValues(otherPlayer);

                otherPlayer.Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + otherPlayer.deckName), GM.background);

                return;

            }

            if (setup && otherPlayer.setup && !setupDeck) {

                setupDeck = true;

                Deck.Shuffle();

                CmdSetupDeck();

                return;

            }

            if (setupDeck && otherPlayer.setupDeck && !finishSetup) {

                finishSetup = true;

                GM.connectingPanel.SetActive(false);

            }

        }

    }

    void SetupPlayerValues (SC_Player p) {

        p.Health = GM.baseHealth;

        p.Stamina = GM.baseStamina;

        p.BodyPartsHealth = new Dictionary<BodyPart, int>();

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                p.BodyPartsHealth.Add(bP, GM.baseBodyPartHealth);

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

        Deck.Draw(nbr, true);

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

                    CmdChooseStartingPlayer(true);

                } else if (ShifumiChoice == otherPlayer.ShifumiChoice) {

                    CmdResetShiFuMi();                        

                } else {

                    CmdChooseStartingPlayer(false);

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

    #region Start Game
    [Command]
    void CmdChooseStartingPlayer (bool won) {        

        RpcChooseStartingPlayer(won);

    }

    [ClientRpc]
    void RpcChooseStartingPlayer (bool won) {

        GM.shifumiPanel.SetActive(false);

        GM.ShowTurnPanel(isLocalPlayer ? won : !won);

    }    

    [Command]
    public void CmdStartGame(bool start) {

        RpcStartGame(start);

    }

    [ClientRpc]
    void RpcStartGame(bool start) {        

        (isLocalPlayer ? this : otherPlayer).Turn = start;

        (isLocalPlayer ? otherPlayer : this).Turn = !start;

        GM.StartGame();

    }
    #endregion

    #region Card usage
    delegate void CardAction (Transform t);

    void ActionOnCard (string id, CardAction a) {

        foreach (Transform t in isLocalPlayer ? GM.localHand : GM.otherHand)
            if (t.name == id)
                a(t);

    }

    #region Base usage
    [Command]
    public void CmdUseBaseCard (GameObject player, string id) {

        RpcBaseUseCard(player, id);

    }

    [ClientRpc]
    void RpcBaseUseCard (GameObject player, string id) {

        ActionOnCard(id, (t) => { t.GetComponent<SC_UI_Card>().Card.Use(player); });

        BaseCardUsage(id);

    }

    void BaseCardUsage (string id) {

        RectTransform h = isLocalPlayer ? GM.localHand : GM.otherHand;

        ActionOnCard(id, (t) => { Destroy(t.gameObject); });

        SC_Deck.OrganizeHand(h);

    }
    #endregion

    #region Offensive move with body part choice
    [Command]
    public void CmdUseOffensiveMoveCard (GameObject player, string id, bool choice) {

        RpcUseOffensiveMoveCard(player, id, choice);

    }

    [ClientRpc]
    void RpcUseOffensiveMoveCard (GameObject player, string id, bool choice) {

        ActionOnCard(id, (t) => { (t.GetComponent<SC_UI_Card>().Card as SC_OffensiveMove).Use(player, choice); });

        BaseCardUsage(id);

    }
    #endregion
    #endregion

    #region Skip turn
    [Command]
    public void CmdSkipTurn () {

        RpcSkipTurn();

    }

    [ClientRpc]
    void RpcSkipTurn () {

        if (!isLocalPlayer) {

            localPlayer.Turn = true;

            localPlayer.CmdDraw(1);

        }

    }
    #endregion

    #endregion
}
