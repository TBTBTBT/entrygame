﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using WebSocketSharp.Net;



public class WebSocketManager : MonoBehaviour
{


    private WebSocket ws;

    public void Setup(string address,
        Action<object, EventArgs> onopen,
        Action<object, MessageEventArgs> onmessage, 
        Action<object, CloseEventArgs> onclose, 
        Action<object, ErrorEventArgs> onerror)
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }

        Debug.Log(address);
        ws = new WebSocket(address);
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("[ws] Open");
            onopen?.Invoke(sender, e);
        };
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("[ws] " + e.Data);
            onmessage?.Invoke(sender, e);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("[ws] Error Message: " + e.Message);
            onerror?.Invoke(sender, e);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("[ws] Close");
            onclose?.Invoke(sender, e);
        };
        //ws.Log.Level = LogLevel.Debug;
        //ws.Log.File = Application.dataPath +"/log.txt";
        ws.SetProxy("http://157.109.25.6:3128", "", "");
        // ws.Log.Level = LogLevel.Trace;
        ws.ConnectAsync();
        
    }

    public bool IsConnect()
    {
        return ws != null && ws.ReadyState == WebSocketState.Open;
    }
    public void Send(string msg)
    {
        ws?.Send(msg);
    }
    private void OnApplicationQuit()
    {
        if (IsConnect())
        {
            ws.Close();
        }
    }

    public void Close()
    {
        ws.Close();
    }


}
