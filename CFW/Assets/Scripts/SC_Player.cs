﻿using System;
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

                Health = GM.baseHealth;

                Stamina = GM.baseStamina;

                BodyPartsHealth = new Dictionary<BodyPart, int>();

                foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
                    if (bP != BodyPart.None)
                        BodyPartsHealth.Add(bP, GM.baseBodyPartHealth);

                CmdSetup();

                localPlayer = this;

                Deck = Instantiate(Resources.Load<SC_Deck>("Decks/" + deckName), GM.background);

                foreach (SC_Player p in players)
                    if (p != this)
                        otherPlayer = p;

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

    #region Start Turn
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

    #endregion
}
