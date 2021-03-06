﻿using System;


[Serializable]
public class MsgRoot<T>
{
    public string type;
    public T data;
}
[Serializable]
public class MsgConnect
{
    public string id;
}
[Serializable]
public class MsgReady
{
    [Serializable]
    public class PlayerData
    {
        public string id;
        public int pid;
    }
    [Serializable]
    public class RuleData
    {
        public int time = 0;
        public int add = 0;
    }
    public PlayerData[] member;
    public RuleData rule;
}
[Serializable]
public class MsgMatching
{
    public string address;
    public string room;
}
[Serializable]
public class MsgEntry
{
    public string id;
}
[Serializable]
public class MsgInput
{
    public int pid;
    public string type;
    public int strong;
    public int angle;
    public int frame;
    public int number;
}
[Serializable]
public class SendEntry
{
    public string name;
}
[Serializable]
public class SendConnectGame
{
    public string name;
    public string room;
}
[Serializable]
public class SendInput
{
    public string id;
    public string type;
    public int strong;
    public int angle;
    public int frame;
}