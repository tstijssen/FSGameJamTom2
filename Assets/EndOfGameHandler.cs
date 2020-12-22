using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class EndOfGameHandler : MonoBehaviour
{
    public GameObject endGameStateUI;

    [Scene]
    public string offlineScene;

    public Text playerVictoryText;
    public Text playerVictoryText2;

    public Text deadPlayersText;

    public List<string> alivePlayerNames = new List<string>();
    public List<string> deadPlayerNames = new List<string>();

    private static EndOfGameHandler _instance;

    public static EndOfGameHandler Instance { get { return _instance; } }

    // Start is called before the first frame update
    void Start()
    {
        deadPlayersText.text = "";
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private string findName = "";

    private bool isName(string name)
    {

        return (name == findName);
    }

    public void AddPlayer(string name)
    {
        findName = name;
        if (!alivePlayerNames.Exists(isName))
        {
            alivePlayerNames.Add(name);
        }
    }

    public void PlayerKilled(string name, string killerName)
    {
        deadPlayerNames.Add(name);

        deadPlayersText.text += (name + " - killed by: " + killerName) + "\n";

        alivePlayerNames.Remove(name);

        //check if all players killed
        if(alivePlayerNames.Count <= 1)
        {
            endGameStateUI.SetActive(true);

            if(alivePlayerNames.Count == 1)
            {
                playerVictoryText.text = (alivePlayerNames[0] + " WON!");
                playerVictoryText2.text = (alivePlayerNames[0] + " WON!");
            }
            else
            {
                playerVictoryText.text = "DRAW everyone died!";
                playerVictoryText2.text = "DRAW everyone died!";
            }
        }
    }

    public void ExitMatch()
    {
        NetworkClient.Shutdown();
        SceneManager.LoadScene(offlineScene);
    }
}
