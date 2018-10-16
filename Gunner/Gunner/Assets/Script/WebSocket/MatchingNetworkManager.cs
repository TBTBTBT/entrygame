using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class MatchingNetworkManager : MonoBehaviour
{
    [SerializeField] private Text _inputName;
    [SerializeField] private WebSocketManager _wsModule;
    [SerializeField] private GameNetworkManager _gameNetwork;
    private string _matchingId = "";
    public string GameServerAddress { get; set; } = "";
    public string RoomId { get; set; } = "";
    public enum State
    {
        Init,
        Connect,
        Entry,
        Wait,
        Cancel,
        Close,
    }

    private Statemachine<State> _statemachine;
    void Awake()
    {
        _statemachine = new Statemachine<State>();
        _statemachine.Init(this);
        _statemachine.Next(State.Init);
    }

    void Update()
    {
        _statemachine.Update();
    }
    IEnumerator Init()
    {
        _matchingId = "";
        GameServerAddress = "";
        RoomId = "";
        _wsModule.Setup("wss://socketmmo.herokuapp.com/",
            null,
            OnMessage,
            (o,e)=>Debug.Log($"WebSocket Close Code: {e.Code} Reason: {e.Reason}"),
            null);
        while (!_wsModule.IsConnect())
        {
            //Debug.Log("[ws]Connecting...");
           // yield return new WaitForSeconds(1f);
            yield return null;
        }
        _statemachine.Next(State.Connect);
        yield return null;
    }
    IEnumerator Connect()
    {
        
        yield return null;
    }

    IEnumerator Close()
    {
        _wsModule.Close();
        yield return null;
    }
    void OnMessage(object sender,MessageEventArgs e)
    {
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(e.Data);
        switch (obj.type)
        {
            case "connect":
                ResConnect(e.Data);
                break;
            case "match":
                ResMatching(e.Data);
                break;
        }
    }

    void ResConnect(string msg)
    {
        MsgRoot<MsgConnect> obj = JsonUtility.FromJson<MsgRoot<MsgConnect>>(msg);
        _matchingId = obj.data.id;
        _statemachine.Next(State.Entry);
        Debug.Log("[matching] id :" + _matchingId);
    }

    void ResMatching(string msg)
    {
        MsgRoot<MsgMatching> obj = JsonUtility.FromJson<MsgRoot<MsgMatching>>(msg);
        GameServerAddress = obj.data.address;
        Debug.Log("[matching] room :" + obj.data.room + ", address : " + obj.data.address);
        _statemachine.Next(State.Close);
    }
    //request
    void ReqEntry()
    {
        if (_statemachine.GetCurrentState() != State.Entry)
        {
            return;
        }
        MsgRoot<SendEntry> obj =
            new MsgRoot<SendEntry>()
            {
                type = "connect",
                data = new SendEntry()
                {
                    name = _inputName.text
                }
            };
        _wsModule.Send(JsonUtility.ToJson(obj));
        _statemachine.Next(State.Wait);
    }
    //---
    //input
    public void OnPushSendName()
    {
        ReqEntry();
    }
}
