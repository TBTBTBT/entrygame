using System;


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
public class MsgMatching
{
    public string address;
    public string room;
}
[Serializable]
public class SendEntry
{
    public string name;
}