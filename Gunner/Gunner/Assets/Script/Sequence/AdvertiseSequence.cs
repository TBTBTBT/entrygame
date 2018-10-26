using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AdvertiseSequence : SingletonMonoBehaviourWithStatemachine<AdvertiseSequence, AdvertiseSequence.State> { 
    public enum State
    {
        Init,
    }

    IEnumerator Init()
    {
        Debug.Log("Work");

        yield return null;
    }
}
