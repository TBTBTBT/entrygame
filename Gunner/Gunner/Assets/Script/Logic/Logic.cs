using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//送受信する
public class GunnerData
{
    public int id;
    public Vector2 sPos;
    public Vector2 cPos;
    public float speed;
    public int Direction = 1;// 左向き -1
    public int hp;
    public float kbLog;//今までのkb値の累計
    public float rad;
    public void CalcPos(float time)
    {
        cPos = sPos + new Vector2(Direction * (speed * time + kbLog),0);
    }

}
/*
public enum InputType
{
    BULLET = 1,
    SKIP = 2,
    FORWARD = 3,
    BACKWARD = 4
}*/
public class InputData
{
    public int gunnerId;
    public int bulletId;
    public int inFrame;
    public string type;
    public int strength;
    public int angle;
    public int number;//誤り制御
}
//送受信しない
public class BulletData
{
    public int number; //固有のID 追跡用
    public int bulletId; // MstBulletRecord のID
    public int gunnerId; // 砲撃者のID
    public int sFrame;//計算開始フレーム
    //public int ieFrame = -1;//計算終了フレーム(仮)
    public int eFrame = -1;//計算終了フレーム(真)
    public float strength;// 1/10
    public float angle;// 1/10
    public float cRad;
    public Vector2 sPos;//始点
    //public Vector2 iePos;//imaginary終点 そのまま着弾した場合の終点
    public Vector2 ePos;//true終点 相手の弾によっても変化
    public Dictionary<int,int> hitGunnerIdsAndDamage;//ヒットしたプレイヤーと受けたダメージのペア
    //見た目のみのために必要
    public Vector2 cPos;//現在の点

    public void CalcOrbit(float time)
    {
        MstBulletRecord mstBullet = MasterdataManager.Get<MstBulletRecord>(bulletId);
        float cx = sPos.x + (time * strength * Mathf.Cos(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        float cy = sPos.y + (time * strength * Mathf.Sin(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        cy += mstBullet.gravy * time * time / 2;
        float rad = mstBullet.rad + mstBullet.exprad * time;
        cRad = rad;
        cPos.x = cx;
        cPos.y = cy;
    }
}



//serverが計算する必要すらないかも
//serverではフレームカウントしない方式に 20181022
public class Logic
{

    //----------------------------------------------------------------------------
    //define
    //----------------------------------------------------------------------------
    private readonly float SPF = 0.02f; //使わなくしたい
    private readonly int TIME_DIVISION = 20;//1計算フレームの長さ 1000 / x
    private readonly int MAX_FRAME = 2000;
    private readonly int ADD_FRAME = 20;//serverで足される数値
    //----------------------------------------------------------------------------
    //data
    //----------------------------------------------------------------------------
    private int _frameCount = 0;
    private List<InputData> _logInput = new List<InputData>();
    private List<BulletData> _logBullet = new List<BulletData>();
    private int _startTime;
    private float _collectTime = 0;
    //----------------------------------------------------------------------------
    //public method
    //----------------------------------------------------------------------------
    public List<GunnerData> Gunners { get; set; }
    public int FrameCount => _frameCount;
    public int NowDamage(int id) => _logBullet.Aggregate(0, (dmg, bullet) => bullet.hitGunnerIdsAndDamage.ContainsKey(id) ? bullet.hitGunnerIdsAndDamage[id] : 0);
    public bool IsFinish() => FrameCount >= MAX_FRAME;
    public List<BulletData> HistoryBullets() => _logBullet;
    public List<BulletData> NowBullets() => _logBullet.FindAll(IsNowCalculating);
    //入力
    public void AddInput(InputData input)
    {
        //誤りを探す
        //1すでに受け取っていた場合
        if (_logInput.Find(_ => _.number == input.number) != null)
        {
            Debug.LogWarning("誤り検出");
            return;
        }
        //2飛ばして受け取った場合
        if (input.number != 0 &&
            _logInput.Find(_ => _.number == input.number - 1) == null)
        {
            Debug.LogWarning("誤り検出");
            return;
        }

        _logInput.Add(input);
        AddBullet(input);
    }
    //初期化
    public void Init(List<GunnerData> gunners)
    {
        Gunners = new List<GunnerData>();
        foreach (var gunner in gunners)
        {
            Gunners.Add(gunner);
        }
        _frameCount = 0;
        _collectTime = 0;
    }
    //開始
    public void TimeStamp(){
        _startTime = (int) (Time.realtimeSinceStartup * 1000.0f);
    }
    //更新
    public void CalcFrame(){
        int next = (int)(Time.realtimeSinceStartup * 1000.0f - _startTime + _collectTime) / TIME_DIVISION;
        int loopMax = 500;
        int loopCnt = 0;
        while(next > _frameCount && loopCnt < loopMax){
            SkipFrame();
            loopCnt++;
        }
//        _frameCount = next;
    }
    //フレーム次へ
    public void NextFrame()
    {
        _frameCount++;
    }
    public void SkipFrame()
    {
        CalcOneFrame();
        NextFrame();
    }
    //1フレーム計算

    public void CalcOneFrame()
    {
        //cPos更新
        CalcBullet();
        //cPos使って判定
        CalcCollision();
        CalcGunner();
    }
    //------------------------------------------
    //内部呼出し
    void AddBullet(InputData input)
    {
        if (input.type == "bullet")
        {
            _logBullet.Add(InitBullet(input));
        }
        int loopMax = 500;
        int loopCnt = 0;
        while ((input.inFrame - ADD_FRAME) > _frameCount && loopCnt < loopMax)
        {
            SkipFrame();
            TimeCollectForward();
            Debug.Log((input.inFrame - ADD_FRAME) - _frameCount);
            loopCnt++;
            //     CalcOneFrame();
            //     NextFrame();
        }
    }
    BulletData InitBullet(InputData input)
    {
        BulletData ret = new BulletData();
        GunnerData gunner = Gunners.Find(_ => _.id == input.gunnerId);
        if (gunner == null)
        {
            Debug.LogError("存在しないプレイヤーID");
            return ret;
        }
        ret.sFrame = input.inFrame;
        ret.eFrame = -1;
        ret.bulletId = input.bulletId;
        ret.gunnerId = input.gunnerId;
        ret.sPos = new Vector2(gunner.cPos.x, gunner.cPos.y);
        ret.cPos = new Vector2(gunner.cPos.x, gunner.cPos.y);
        ret.ePos = new Vector2(0, 0);
        ret.strength = input.strength;
        ret.angle = (float)input.angle / 10;
        ret.cRad = 0;
        ret.number = input.number;
        ret.hitGunnerIdsAndDamage = new Dictionary<int, int>();
        return ret;
    }


    //弾の処理
    void CalcBullet()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);//現在計算されるべき弾
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _frameCount - bullet.sFrame;
            bullet.CalcOrbit(FrameToTime(frame));//cPos更新
           
        }
    }


    void CalcGunner()
    {
        foreach (var gunner in Gunners)
        {
            gunner.CalcPos(FrameToTime(_frameCount));
        }
    }
    void CalcCollision()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _frameCount - bullet.sFrame;
           // HitGunner(_frameCount,bullet,Gunners);
            //HitBullet(_frameCount,bullet);
            HitLand(_frameCount, bullet);

        }
    }
    void HitLand(int frame,BulletData bullet)
    {
        if (bullet.cPos.y < 0)
        {
            bullet.eFrame = frame;
        }
    }

    void HitBullet(int frame, BulletData bullet)
    {

    }
    void HitGunner(int frame,BulletData bullet,List<GunnerData> gunners)
    {
        foreach (var gunner in gunners)
        {
            if (CheckCollision(gunner.cPos, gunner.rad, bullet.cPos, bullet.cRad))
            {
                bullet.eFrame = frame;
            }
        }
    }

    bool CheckCollision(Vector2 pos1 ,float rad1 ,Vector2 pos2 ,float rad2)
    {
        return pos2.magnitude - pos1.magnitude <= rad1 + rad2;
    }
    //------------------------------------------
    //ずれ訂正
    //早送り(クライアントが遅れ)
    void TimeCollectForward()
    {
        //Debug.Log(_collectTime);
        _collectTime += TIME_DIVISION;
    }
    //巻き戻し
    //------------------------------------------
    //誤り訂正
    //再入力対策
    //入力飛ばし対策






    bool IsNowCalculating(BulletData bullet)
    { //現在のフレームが計算開始フレーム以上かつ、（　計算が終了していない または 計算終了フレーム以下。(再計算対応))
        return bullet?.sFrame <= _frameCount && ( bullet?.eFrame == -1 || bullet?.eFrame >= _frameCount);
    }
    



    
    public float FrameToTime(int frame)
    {
        return SPF * frame;
    }
    /*
    public int TimeToFrame(float time)
    {
        return Mathf.CeilToInt(time / SPF);
    }
    */



    public void Recalc(int pastFrame)
    {

    }

    //1 クライアントフレームの計算
    void CalcOneClientFrame(){

    }

    /*
//弾道計算
//斜方投射になる
//cx = strong x cos (angle) x time
//cy = stromg x sin (angle) x time - 1/2 x g x time^2
void CalcOrbit(float time, BulletData bullet)
{
    MstBulletRecord mstBullet = MasterDataManager.Get<MstBulletRecord>(bullet.bulletId);
    var gx = mstBullet.gravx;
    var gy = mstBullet.gravy;
    var wei = mstBullet.weight;
    var sx = bullet.sPos.x;
    var sy = bullet.sPos.y;
    var str = bullet.strength;
    var ang = bullet.angle;
    var rad = mstBullet.rad;
    float cx = sx + (time * bullet.strength * Mathf.Cos(ang * Mathf.Deg2Rad)) * wei;
    float cy = sy + (time * bullet.strength * Mathf.Sin(ang * Mathf.Deg2Rad)) * wei;
    cy += gy * time * time / 2;
    //        cx *= gx;
    bullet.cRad = rad;
    bullet.cPos.x = cx;
    bullet.cPos.y = cy;
}*/
    //    public List<>
}
#if false
//先に計算しておく版
    void PreCalcBullet(BulletData bullet){
        CalcImaginaryHitPosition(bullet);
    }
    //着地点
    void CalcImaginaryHitPosition(BulletData bullet){
        MstBulletRecord mstBullet = MasterDataManager.Get<MstBulletRecord>(bullet.bulletId);
        var gx = mstBullet.gravx;
        var gy = mstBullet.gravy;
        var sx = bullet.sPos.x;
        var sy = bullet.sPos.y;
        var str = bullet.strength;
        var ang = bullet.angle;
        //var rad = mstBullet.rad;
        //float cy = sy + (time * bullet.strength * Mathf.Sin(ang * Mathf.Deg2Rad)) * wei;
        float ietime = Mathf.Sqrt(Mathf.Abs((2 / gy) * (sy + str * Mathf.Sin(ang * Mathf.Deg2Rad))));
        int ieframe = TimetoFrame(ietime);
        bullet.iePos = GetPosition(ieframe, bullet);
    }
    //ぶつかった場合の終点
    void CalcTrueHitPosition(BulletData bullet){
        MstBulletRecord mstBullet = MasterDataManager.Get<MstBulletRecord>(bullet.bulletId);
        List<BulletData> possibleHit = _logBullet.FindAll(_ => _.sFrame <= bullet.ieFrame || _.ieFrame >= bullet.sFrame);

    }
    Vector2 GetPosition(int frame,BulletData bullet){
        float time = CalcTime(frame);
        Vector2 ret;
        MstBulletRecord mstBullet = MasterDataManager.Get<MstBulletRecord>(bullet.bulletId);
        ret.x = bullet.sPos.x + (time * bullet.strength * Mathf.Cos(bullet.angle * Mathf.Deg2Rad)) * mstBullet.weight;
        ret.y = bullet.sPos.y + (time * bullet.strength * Mathf.Sin(bullet.angle * Mathf.Deg2Rad)) * mstBullet.weight;
        ret.y += mstBullet.gravy * time * time / 2;
        return ret;
    }
    void PreCalcCollision(){
        
    }
    void CalcHitBulletToGunner(BulletData bullet){
        
    }
#endif