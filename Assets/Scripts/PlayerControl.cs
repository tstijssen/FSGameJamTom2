using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    public float pSpeed;
    public float growScale;
    public float moveHorizontal;
    public float moveVertical;

    public float blobCooldown = 2.0f;
    public float cooldownTimer = 0.0f;

    public GameObject blobPrefab;
    public uint activeBlobID = 0;

    [SyncVar(hook="onNameChange")]
    public string playerName;
    [SyncVar(hook="onTypeChange")]
    public int playerType;

    public Sprite[] typeSprites;
    public bool alive = true;
    public static int playersAlive = 0;

    private AudioSource[] lava = new AudioSource[2];

    // Start is called before the first frame update
    void Start()
    {
        lava = GetComponents<AudioSource>();
        lava[0].Pause();
        playersAlive += 1;
        if (isLocalPlayer)
        {
            CmdChangeName(PlayerPrefs.GetString("playerName"));
            CmdChangeType(PlayerPrefs.GetInt("playerType"));
        }
    }

    [Command]
    void CmdChangeName(string newName)
    {
        playerName = newName;
    }

    [Command]
    void CmdChangeType(int newType)
    {
        playerType = newType;
    }

    void onNameChange(string oldString, string newString)
    {
        name = newString;
        playerName = newString;
        EndOfGameHandler.Instance.AddPlayer(name);
        Debug.Log(playerName + " setup!");
    }

    void onTypeChange(int oldType, int newType)
    {
        GetComponent<SpriteRenderer>().sprite = typeSprites[newType];
        playerType = newType;
        Debug.Log(name + " type = " + playerType);
    }

    // Update is called once per frame
    void Update()
    {
        // movement for local active player
        if (!isLocalPlayer || !alive) return;
        
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        transform.Translate(movement * pSpeed * Time.deltaTime);

        if(cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            cooldownTimer = 0.0f;
        }

        // place blob with mouseDown
        if (Input.GetMouseButtonDown(0) && cooldownTimer == 0.0f)
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            SpawnBlob(Camera.main.ScreenToWorldPoint(mousePos), GetComponent<NetworkIdentity>().netId);
            lava[0].Play();
        }
        // release mouse to deactivate blob
        if (Input.GetMouseButtonUp(0) && activeBlobID != 0)
        {
            cooldownTimer = blobCooldown;
            DeactivateBlob(activeBlobID);
            activeBlobID = 0;
            lava[0].Stop();
            lava[1].Play();
        }
    }

    // this is called on the server
    [Command]
    private void SpawnBlob(Vector3 mousePos, uint ownerPlayerID)
    {
        GameObject newBlob = Instantiate(blobPrefab, mousePos, Quaternion.identity);
        NetworkServer.Spawn(newBlob);
        OnBlobSpawned(newBlob.GetComponent<NetworkIdentity>().netId, ownerPlayerID);
    }

    [ClientRpc]
    private void OnBlobSpawned(uint newBlobID, uint ownerPlayerID)
    {
        NetworkIdentity targetNetID;
        bool success = NetworkIdentity.spawned.TryGetValue(ownerPlayerID, out targetNetID);
        if(success)
            targetNetID.GetComponent<PlayerControl>().activeBlobID = newBlobID;

        NetworkIdentity blobNetID;
        success = NetworkIdentity.spawned.TryGetValue(newBlobID, out blobNetID);
        if (success)
            blobNetID.GetComponent<BlobHaviour>().ownerNetworkID = ownerPlayerID;
    }

    [Command]
    public void DeactivateBlob(uint targetBlobID)
    {
        
        RpcOnBlobDeactivate(targetBlobID);
        
    }

    [ClientRpc]
    void RpcOnBlobDeactivate(uint targetBlobID)
    {
        
        NetworkIdentity targetNetID;
        bool success = NetworkIdentity.spawned.TryGetValue(targetBlobID, out targetNetID);

        BlobHaviour targetBlob = targetNetID.GetComponent<BlobHaviour>();
        targetBlob.DeActivate();
    }

    public void Kill(uint killer)
    {
        string killerName = "";
        uint ourID = GetComponent<NetworkIdentity>().netId;

        // who killed this player?
        if (killer == ourID)
        {
            killerName = name;
            Debug.Log(name + " killed themselves!");
        }
        else
        {
            NetworkIdentity targetNetID;
            bool success = NetworkIdentity.spawned.TryGetValue(killer, out targetNetID);
            if (success)
                killerName = targetNetID.gameObject.name;

            Debug.Log(name + " killed by " + killerName + "!");
        }

        // commence End of Game
        if (activeBlobID != 0)
        {
            cooldownTimer = blobCooldown;
            DeactivateBlob(activeBlobID);
            activeBlobID = 0;

        }

        alive = false;
        PlayerKilled(name, killerName);
    }

    [Command]
    public void PlayerKilled(string name, string killerName)
    {
        RpcPlayerKilled(name, killerName);
    }

    [ClientRpc]
    private void RpcPlayerKilled(string name, string killerName)
    {
        EndOfGameHandler.Instance.PlayerKilled(name, killerName);
    }
}

