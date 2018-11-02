using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonDialog : ModalDialogBase
{
    [Header("Prefab")]
    [SerializeField]
    private CommonDialogButton _buttonPrefab;
    [Header("Set")]
    [SerializeField] private Text _title;
    [SerializeField] private Text _message;
    [SerializeField] private LayoutGroup _buttonGrid;

    public override void Setup(ModalDialogElement element)
    {
        _title.text = element.Title;
        _message.text = element.Message;
        SetButtons(element);
    }

    void SetButtons(ModalDialogElement element)
    {
        if (element.Buttons == null)
        {
            return;
        }

        foreach (var buttonData in element.Buttons)
        {
            var button = Instantiate(_buttonPrefab, _buttonGrid.transform);
            button.Button.onClick.AddListener(()=>OnPushButton(buttonData.Value));
            button.Text.text = buttonData.Key;
        }

    }
    void OnPushButton(Func<bool> cb)
    {
        if (Current == State.Display)
        {
            if (cb())
            {
                _statemachine.Next(State.Close);
            }
        }
    }
    IEnumerator Init()
    {
        
        Next(State.Open);
        yield return null;
    }
    IEnumerator Open()
    {
        Next(State.Display);
        yield return null;
    }
    IEnumerator Display()
    {
        yield return null;
    }
    IEnumerator Close()
    {
        Next(State.End);
        yield return null;
    }
}