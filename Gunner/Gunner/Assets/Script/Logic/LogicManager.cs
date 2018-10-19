using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviourWithStatemachine<LogicManager.State> {

    public enum State
    {
        Init,
        Wait,
        Entry,
        Main,
        End
    }
    [SerializeField] private NetworkModuleManager _networkModule;
    private Logic _logic = null;
	// Use this for initialization
	IEnumerator Init () {
        _logic = new Logic();
        _logic.Init(new List<GunnerData>()
        {
            new GunnerData(){hp = 1000,id = 0,x = 100,y = 10},
            new GunnerData(){hp = 1000,id = 1,x = -100,y = 10}
        });
        Next(State.Wait);
	    yield return null;
	}

    IEnumerator Entry()
    {

        StartCoroutine(UpdateLogic(0.2f));
        Next(State.Main);
        yield return null;

    }
    public void OnMessageGame(string msg){
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(msg);
        switch(obj.type){
            case "start" : StartLogic(); break;
            case "input" : RecieveInput(msg); break;
        }
    }
    void RecieveInput(string msg){
        MsgRoot<MsgInput> obj = JsonUtility.FromJson<MsgRoot<MsgInput>>(msg);

        _logic.AddInput(new InputData()
        {
            angle = obj.data.angle,
            bulletId = 10,
            gunnerId = 0,
            type = obj.data.type,
            inFrame = obj.data.frame,
            strength = obj.data.strong,
            number = obj.data.number
        });
        
    }
    public void StartLogic()
    {
        if (Current == State.Wait)
        {
            Next(State.Entry);
        }

    }
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
        //    _logic.NextFrame();
        //    _logic.CalcFrame();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            _logic.AddInput(new InputData()
            {
                angle = 1350,
                bulletId = 10,
                gunnerId = 0,
                type = InputType.BULLET,
                inFrame = _logic.FrameCount + 2,
                strength = 100,
            });
        }
    }
    */

    IEnumerator UpdateLogic(float span)
    {
        while (!_logic.IsFinish())
        {

            _logic.CalcFrame();
            _logic.NextFrame();
            yield return new WaitForSeconds(span);
        }
    }
	// Update is called once per frame
	void OnGUI () {
		GUILayout.Label(_logic.FrameCount + " frame");/*
	    foreach (var bullet in _logic.HistoryBullets())
	    {
	        GUILayout.Label($"[bullet] { bullet.cPos.x } , { bullet.cPos.y } ");
        }*/
        foreach (var bullet in _logic.NowBullets())
	    {
	        GUILayout.Label($"[bullet] { bullet.cPos.x } , { bullet.cPos.y } ");
	     //   DebugCircle(bullet.cPos,bullet.cRad, 20);
        }
	   // DebugCircle(new Vector2(20,10),1, 5);

    }
    /*
    void DebugCircle(Vector2 center, float rad , int points)
    {

        GL.Begin(GL.LINE_STRIP);
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        center += new Vector2(Screen.width/2,Screen.height/2);
        for (int i = 0; i < points; i++)
        {
            float radian = ((float)i / points) * Mathf.PI*2;
            GL.Color(new Color(5,5,5,5));
            // One vertex at transform position
            //GL.Vertex3(0, 0, 0);
            // Another vertex at edge of circle
            
            GL.Vertex3((center.x  + Mathf.Cos(radian) * rad) / 1, (center.y + Mathf.Sin(radian) * rad) / 1, 0);
        }
        GL.End();
        GL.PopMatrix();
    }*/
}
