using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour {
    public enum State
    {
        Init,
        Connect,
        Entry,
        Wait,
        Cancel,
        Close,
    }
    private string _gameId = "";
    public string GameServerAddress { get; set; } = "";
    public string RoomId { get; set; } = "";
    // Use this for initialization
    void Awake () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
