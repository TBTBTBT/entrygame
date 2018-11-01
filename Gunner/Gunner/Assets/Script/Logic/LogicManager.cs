using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviourWithStatemachine<LogicManager.State>
{

    public enum State
    {
        Init,
        Wait,
        Entry,
        Loop,
        End
    }
    //通常計算で起こりうるフレームの状態
    public enum FrameState
    {
        None,
        New,
        Skip,//またぎ
        RollBack,//もどり
    }
    //通信などで発生しうるずれなどのエラー状態
    public enum ErrorState
    {
        None,
        RollBack,//過去の入力
        SkipInput,//またいでしゅとく
        SameInput,

    }
    [SerializeField] private NetworkModuleManager _networkModule;
    private Logic _logic = null;
    //----------------------------------------------------------------------------
    //data
    //----------------------------------------------------------------------------
    
    private readonly int TIME_DIVISION = 20;//1計算フレームの長さ 1000 / x
    public int FrameCount => _frameCount;
    private int _frameCount = 0;
    private int _addFrame = 0;
    private int _maxTime = 0;
    private int _startTime;
    private int _inputNumber = 0;
    private float _collectTime = 0;
    public List<BulletData> NowBullets => _logic.NowBullets();
    public List<GunnerData> Gunners => _logic.Gunners;
    public bool IsReady => Current == State.Entry || Current == State.Loop;
    public bool IsFinish() => FrameCount >= _maxTime / TIME_DIVISION;
    //
    // Use this for initialization
    IEnumerator Init()
    {
        _logic = new Logic();
        _logic.Init(new List<GunnerData>()
        {
            new GunnerData(){sHp = 1000,id = 1,sPos = new Vector2(100,10),Direction = -1,speed = 3f ,rad = 5, cKnockback = 0},
            new GunnerData(){sHp = 1000,id = 2,sPos = new Vector2(-100,10),Direction = 1,speed = 3f ,rad = 5, cKnockback = 0}
        });
        _frameCount = 0;
        _collectTime = 0;
        _inputNumber = -1;
        Next(State.Wait);
        yield return null;
    }

    IEnumerator Entry()
    {

        //StartCoroutine(UpdateLogic(0.1f));
        Next(State.Loop);
        yield return null;

    }
    IEnumerator Loop(){
        TimeStamp();
        while(!IsFinish())
        {

            yield return CalculateLogic();

        }
    }
    public void OnMessageGame(string msg)
    {
        MsgRoot<object> obj = JsonUtility.FromJson<MsgRoot<object>>(msg);
        switch (obj.type)
        {
            case "ready": ResReady(msg); break;
            case "start": StartLogic();  break;
            case "input": RecieveInput(msg); break;
            
        }
    }
    //----------------------------------------------------------------------------
    //初期化
    //----------------------------------------------------------------------------
    void SetServerData(int maxTime, int addFrame)
    {
        _maxTime = maxTime;
        _addFrame = addFrame;
    }

    public void TimeStamp()
    {
        _startTime = (int)(Time.realtimeSinceStartup * 1000.0f);
    }
    //----------------------------------------------------------------------------
    //Logic計算
    //----------------------------------------------------------------------------
    IEnumerator CalculateLogic()
    {
        int next = CalcNowFrameFromElapsedTime();
        switch (CalcNowFrameState(next))
        {

            case FrameState.None:
               // Debug.Log("同一F");
                break;
            case FrameState.Skip:
            case FrameState.New:
               // Debug.Log("new");
                _frameCount++;
                _logic.Calc(FrameCount);
                break;
            //case FrameState.Skip:

                //前フレームから今フレームまでの差を計算
            //    CalcDiffFrame();
            //    break;
            case FrameState.RollBack:
                Debug.Log("待ち");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _frameCount = next;
        yield return null;
    }
    //前フレームから今フレームまでの差を計算
    void CalcDiffFrame()
    {
        Debug.Log("F送り");
        int next = CalcNowFrameFromElapsedTime();
        for (int i = 0; i < 100; i++)
        {
            _frameCount++;
            if (CalcNowFrameState(next) == FrameState.None)
            {
                break;
            }
            _logic.Calc(FrameCount);
        }
    }

    void RecalcFrame()
    {

    }
    //----------------------------------------------------------------------------
    //フレーム計算
    //----------------------------------------------------------------------------

    //Unix時間から現フレーム計算
    //Unix時間ではなく立ち上げからの経過時間
    public int CalcNowFrameFromElapsedTime()
    {
        return (int)(Time.realtimeSinceStartup * 1000.0f - _startTime + _collectTime) / TIME_DIVISION;
    }
    /*
    bool IsNewFrame(int frame)
    {
        return frame > _frameCount;
    }*/
    public FrameState CalcNowFrameState(int frame)
    {
        FrameState ret = FrameState.None;
        if (frame < FrameCount     ) ret = FrameState.RollBack;//巻き戻った
        if (frame > FrameCount + 1 ) ret = FrameState.Skip;//またいだ
        if (frame == FrameCount + 1) ret = FrameState.New;//正しく次へ
        return ret;
    }
 
    public void StartLogic()
    {
        if (Current == State.Wait)
        {
            Next(State.Entry);
        }

    }
    void RecieveInput(string msg)
    {
        MsgRoot<MsgInput> obj = JsonUtility.FromJson<MsgRoot<MsgInput>>(msg);
        if ((obj.data.frame - _addFrame) > _frameCount)
        {//スキップ
            Debug.Log("Skip");
            _collectTime += ((obj.data.frame - _addFrame) - _frameCount);
            Debug.Log(_collectTime);
        }

        if (_frameCount >= obj.data.frame)
        {
            Debug.LogWarning("誤り検出");
            RollBack(obj.data.frame - 1);
        }
        if (_inputNumber >= 0)
        {
            //同一もしくは過去の入力
            if (_inputNumber >= obj.data.number)
            {
                Debug.LogWarning("誤り検出");
                return;
            }

            //2飛ばして受け取った場合
            if (_inputNumber + 1 < obj.data.number)
            {
                Debug.LogWarning("誤り検出");
                return;
            }

        }

        _logic.AddInput(new InputData()
        {
            angle = obj.data.angle,
            bulletId = 0,
            gunnerId = obj.data.pid,
            type = obj.data.type,
            inFrame = obj.data.frame,
            strength = obj.data.strong,
            number = obj.data.number
        });
        _inputNumber = obj.data.number;
    }
    //誤り制御
    void RollBack(int frame)
    {
        _frameCount = frame;
        _logic.RollBack(frame);
    }

    //----------------------------------------------------------------------------
    //レスポンス
    //----------------------------------------------------------------------------

    void ResReady(string msg)
    {
        MsgRoot<MsgReady> obj = JsonUtility.FromJson<MsgRoot<MsgReady>>(msg);
        var rule = obj.data.rule;
        SetServerData(rule.time, rule.add);
        Debug.Log("SetRule");
    }

#if true


    void OnGUI()
    {
        GUILayout.Label(FrameCount + " frame");/*
	    foreach (var bullet in _logic.HistoryBullets())
	    {
	        GUILayout.Label($"[bullet] { bullet.cPos.x } , { bullet.cPos.y } ");
        }*/
        foreach (var bullet in _logic.NowBullets())
        {
         //   GUILayout.Label($"[bullet] { bullet.cPos.x } , { bullet.cPos.y } ");

            //   DebugCircle(bullet.cPos,bullet.cRad, 20);
        }
        // DebugCircle(new Vector2(20,10),1, 5);
        foreach(var gunner in _logic.Gunners){
            GUILayout.Label(gunner.cHp.ToString());
        }

    }


#endif

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
/*
IEnumerator UpdateLogic(float span)
{
    while (!_logic.IsFinish())
    {

        _logic.CalcOneFrame();
        _logic.NextFrame();
        yield return new WaitForSeconds(span);
    }
}
*/
// Update is called once per frame

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
