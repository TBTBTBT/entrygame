using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineSample : MonoBehaviour {
    public enum State
    {
        Init,
        State1,
        State2,
        State3

    }
    private Statemachine<State> _statemachine = new Statemachine<State>();

    IEnumerator Init(){
        _statemachine.Next(State.State1);
        yield return null;
    }
    IEnumerator State1()
    {
        Debug.Log("Enter1");
        yield return null;
        Debug.Log("Next1");
        _statemachine.Next(State.State2);
        yield return null;
        Debug.Log("Exit1");
    }
    IEnumerator State2()
    {
        Debug.Log("Enter2");
        yield return State2Nest1();
        Debug.Log("Next2");
        _statemachine.Next(State.State3);
        yield return null;
    }
    IEnumerator State2Nest1(){
        Debug.Log("Enter21");
        yield return State2Nest2();
        Debug.Log("Exit21");
    }
    IEnumerator State2Nest2()
    {
        Debug.Log("Enter22");
        yield return State2Nest3();
        Debug.Log("Exit22");
    }
    IEnumerator State2Nest3()
    {
        Debug.Log("Enter23");
        yield return null;
        Debug.Log("Exit23");
    }
    IEnumerator State3()
    {
        _statemachine.Next(State.State1);
        yield return null;
    }
	// Use this for initialization
	void Awake () {
        _statemachine.Init(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _statemachine.Update();
        }
	}
}
