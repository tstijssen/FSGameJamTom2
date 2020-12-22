using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    [Header("Player Info")]
    [SyncVar]
    public string playerName;
    [SyncVar]
    public int playerType;
    
    public GameObject playerUI;
    public Text playerNameText;
    public Text playerReadyText;
    public Image playerSprite;

    public Sprite[] typeSprites;

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerType = index;
        playerName = "Player " + (index + 1);

        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.SetInt("playerType", playerType);
    }

    public override void OnGUI()
    {
        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room || !NetworkManager.IsSceneActive(room.RoomScene))
        {
            if(!playerUI.activeInHierarchy)
            {
                GameObject lobby = GameObject.FindWithTag("Lobby");

                if(lobby)
                {
                    if(lobby.TryGetComponent(out GridLayoutGroup grid))
                    {
                        this.transform.SetParent(lobby.transform);
                    }
                }
                playerType = index;
                playerName = "Player " + (index + 1);

                PlayerPrefs.SetString("playerName", playerName);
                PlayerPrefs.SetInt("playerType", playerType);

                playerNameText.text = playerName;
                playerUI.SetActive(true);
                playerSprite.sprite = typeSprites[index];
            }
        }
        else if (playerUI.activeInHierarchy)
        {
            playerUI.SetActive(false);
            this.transform.SetParent(null);
        }
    }

    public void SetPlayerReady()
    {
        if (NetworkClient.active && isLocalPlayer && !readyToBegin)
        {
            CmdChangeReadyState(true);
            playerReadyText.text = "Ready!";
        }
    }

    public void UnSetPlayerReady()
    {
        if (NetworkClient.active && isLocalPlayer && readyToBegin)
        {
            CmdChangeReadyState(false);
            playerReadyText.text = "Not ready!";
        }
    }

    public void RemovePlayer()
    {

    }
}
