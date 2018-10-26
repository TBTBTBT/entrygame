using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntireSequenceManager : EntireSequenceManagerBase<EntireSequenceManager.State> {

    public enum State
    {
        Init,
        AdvertiseSequence,
        TitleSequence,
        DebugSequence
    }

    IEnumerator Init()
    {
        yield return MasterdataManager.Instance.InitMasterdataAsync();
        NextScene(State.DebugSequence);
        yield return null;
    }

    IEnumerator AdvertiseSequences()
    {
        yield return null;
    }
    IEnumerator TitleSequence()
    {
        yield return null;
    }
}
