using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTest : MonoBehaviourWithStatemachine<DialogTest.State> {
    public enum State
    {
        Init,
        Test,
    }

    IEnumerator Init()
    {
        _statemachine.Next(State.Test);
        yield return null;
    }

    IEnumerator Test()
    {
        yield return ModalDialogManager.OpenInProcess(new ModalDialogElement()
        {
            Title = "TEST",
            Message = "てすと",
            Buttons = new Dictionary<string, Func<bool>>
            {
                {"OK", () => { Debug.Log("OK");return true;} },
                {"CANC", () => { Debug.Log("CAC");return true;} },
                {"TES", () => { Debug.Log("TES");return false;} }
            }
        });
        Next(State.Init);
    }
	
}
