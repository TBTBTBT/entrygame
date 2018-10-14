using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour {
    private Logic _logic = null;
	// Use this for initialization
	void Awake () {
        _logic = new Logic();
	}
	
	// Update is called once per frame
	void OnGUI () {
		
	}
}
