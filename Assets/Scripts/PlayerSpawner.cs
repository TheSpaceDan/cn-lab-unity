using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    
    void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Photon.Pun.PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    
    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        
        player = Photon.Pun.PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }
}
