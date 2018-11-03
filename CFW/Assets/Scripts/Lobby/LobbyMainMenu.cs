using UnityEngine;
using UnityEngine.UI;

namespace Prototype.NetworkLobby {

    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour {

        LobbyManager lobbyManager;

        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;

        public InputField ipInput;
        public InputField matchNameInput;

        public void OnEnable() {

            lobbyManager = GetComponentInParent<LobbyManager>();

            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(OnEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(OnEndEditGameName);

        }

        public void OnClickHost() {

            lobbyManager.StartHost();

        }

        public void OnClickJoin() {

            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.networkAddress = ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);

        }

        public void OnClickDedicated() {

            lobbyManager.ChangeTo(null);
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);

        }

        public void OnClickCreateMatchmakingGame() {

            lobbyManager.CreateMatchmakingGame(matchNameInput.text);

        }        

        public void OnQuickMatchmaking() {

            lobbyManager.StartMatchMaker();
            lobbyManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, lobbyManager.OnMatchList);            

        }

        public void OnClickOpenServerList() {

            lobbyManager.StartMatchMaker();
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.ChangeTo(lobbyServerList);

        }

        void OnEndEditIP(string text) {

            if (Input.GetKeyDown(KeyCode.Return))
                OnClickJoin();            

        }

        void OnEndEditGameName(string text) {

            if (Input.GetKeyDown(KeyCode.Return))
                OnClickCreateMatchmakingGame();
            
        }

    }
}
