using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum TouchMode
{

    PointerDown,
    PointerUp,
    EndHorizontalDrag,
    EndVerticalDrag,
    VerticalDrag,
    HorizontalDrag,
    Drag,

}

public class TouchUIManager : MonoBehaviour
{
    [SerializeField] private EventTrigger _trigger;
    private List<UnityAction<PointerEventData, TouchMode>> _callBack = new List<UnityAction<PointerEventData, TouchMode>>();
    private bool _isTouch = false;
    private float _time = 0;
    private Vector2 _anchorTouchPos;
    private TouchMode _nowMode;
    [SerializeField] private float _borderLength = 30;
    private float _arrowHorizontalAngle = 10f;
    void Awake()
    {
        AddButtonCallbackLocal(StartTouchMode, EventTriggerType.PointerDown);
        AddButtonCallbackLocal(UpdateTouchMode, EventTriggerType.Drag);
        AddButtonCallbackLocal(EndTouchMode, EventTriggerType.PointerUp);
    }
    public void AddCallback(UnityAction<PointerEventData> cb, TouchMode eid)
    {
        _callBack.Add((e, t) => {
            if (t == eid) cb(e);
        });
    }

    private void AddButtonCallbackLocal(UnityAction<PointerEventData> cb, EventTriggerType eid)
    {
        var trigger = new EventTrigger.Entry();
        trigger.eventID = eid;
        trigger.callback.AddListener(_=>cb((PointerEventData)_));
        _trigger.triggers.Add(trigger);
    }

    public void StartTouchMode(PointerEventData pe)
    {
        Debug.Log("touch");
        //タッチの瞬間
        Invoke(pe, TouchMode.PointerDown);
        _anchorTouchPos = pe.position;
        _nowMode = TouchMode.PointerDown;
    }
    public void UpdateTouchMode(PointerEventData pe)
    {
        Invoke(pe, TouchMode.Drag);
        _time++;

        // _acc += (pe.position.x - _beforeTouchPos.x);



        Vector2 dist = (pe.position - _anchorTouchPos);
        if (dist.magnitude > _borderLength && _nowMode == TouchMode.PointerDown)
        {//ある程度ドラッグ
            if (Mathf.Abs(dist.x) > Mathf.Abs(Mathf.Cos(_arrowHorizontalAngle * Mathf.PI / 180) * dist.magnitude))
            {//横方向への引張り
                _nowMode = TouchMode.HorizontalDrag;
            }
            else
            {//縦方向
                _nowMode = TouchMode.VerticalDrag;
            }


        }

        if (_nowMode != TouchMode.PointerDown)
        {
            Invoke(pe, _nowMode);
        }
        // if (Mathf.Abs(_acc) > 2)

        //_acc *= 0.9f;
        //_beforeTouchPos = pe.position;
    }

    public void EndTouchMode(PointerEventData pe)
    {
        _time = 0;
        //_acc = 0;
        if (_nowMode == TouchMode.VerticalDrag)
        {
            _nowMode = TouchMode.EndVerticalDrag;
        }
        if (_nowMode == TouchMode.HorizontalDrag)
        {
            _nowMode = TouchMode.EndHorizontalDrag;
        }
        Invoke(pe, _nowMode);
        Invoke(pe, TouchMode.PointerUp);
    }

    void Invoke(PointerEventData e, TouchMode type)
    {
        _callBack.ForEach(cb => cb(e, type));
    }
}
