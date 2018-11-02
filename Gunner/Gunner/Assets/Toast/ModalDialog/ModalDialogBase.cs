using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModalDialogElement
{
    public string Title { get; set; }
    public string Message { get; set; }
    public Dictionary<string, Func<bool>> Buttons { get; set; }//ボタンコールバック　trueで閉じる
    public bool EnableClose { get; set; } = true;
}
public abstract class ModalDialogBase : MonoBehaviourWithStatemachine<ModalDialogBase.State> {
    public enum State
    {
        Init,
        Open,
        Display,
        Close,
        End
    }

    public void SetupElement(ModalDialogElement element)
    {
        Setup(element);
    }

    public IEnumerator WaitForEndState()
    {
        while (Current != State.End)
        {
            yield return null;
        }

    }

    public abstract void Setup(ModalDialogElement element);
}
