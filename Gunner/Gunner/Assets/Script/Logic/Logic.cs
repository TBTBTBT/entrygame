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

    public int hp;

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
    public Vector2 tePos;//true終点 相手の弾によっても変化

    //見た目のみのために必要
    public Vector2 cPos;//現在の点
}
public class MstBulletRecord
{
    public int id;
    public float gravx;//重力
    public float gravy;
    public float weight;//速さに影響 0~1 0が再重
    public int atk;//攻撃力
    public int atkRatio; //攻撃力増減
    public float rad;//半径
    public float radRatio;//半径増減
    public float expRad;//爆発半径
    public bool prevent;//弾同士ぶつかるか

}


public static class MasterDataManager
{
    public static T Get<T>(int id) where T : class
    {
        if (typeof(T) == typeof(MstBulletRecord))
        {
            return (T)(object)MstBulletRecordTable.First(_ => _.id == id);
        }
        return null;
    }
    public static MstBulletRecord[] MstBulletRecordTable = {
        new MstBulletRecord(){id = 10,gravy = -10f,gravx = 0.95f,rad = 10,weight = 1f}
    };
}

public class Logic
{
    //serverが計算する必要すらないかも
    //define
    private float SERVER_SPF = 0.2f;//サーバーの1計算fあたりの経過秒数
    private int CRIENT_CPF = 10;//サーバーの1計算fあたりのクライアントの計算数
    private int MAX_FRAME = 200;
    private int ADD_FRAME = 2;//serverで足される数値
    //data
    private int _serverFrameCount = 0;
    private int _clientFrameCount = 0;
    private int _bulletCount = 0;
    private List<InputData> _logInput = new List<InputData>();
    private List<BulletData> _logBullet = new List<BulletData>();
    public List<GunnerData> Gunners { get; set; }
    public int FrameCount { get { return _serverFrameCount; } }

    public void Init(List<GunnerData> gunners)
    {
        Gunners = new List<GunnerData>();
        foreach (var gunner in gunners)
        {
            Gunners.Add(gunner);
        }
        _serverFrameCount = 0;
        _bulletCount = 0;
    }


    void AddBullet(InputData input)
    {
        if (input.type == "bullet")
        {
            _logBullet.Add(InitBullet(input));
        }
        while ((input.inFrame - ADD_FRAME) > _serverFrameCount)
        {
            Debug.Log((input.inFrame - ADD_FRAME) - _serverFrameCount);
            CalcOneServerFrame();
            NextFrame();
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
        ret.sPos = new Vector2(gunner.x, gunner.y);
        ret.cPos = new Vector2(gunner.x, gunner.y);
        ret.ePos = new Vector2(0, 0);
        ret.strength = input.strength;
        ret.angle = (float)input.angle / 10;
        ret.cRad = 0;
        ret.number = input.number;
        //先に計算しておく
        //PreCalcBullet(ret);
        return ret;
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

    //リアルタイムに計算していく版
    bool IsNowCalculating(BulletData bullet)
    {
        return bullet?.sFrame <= _serverFrameCount && bullet?.eFrame <= -1;
    }
    void CalcBullet()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _serverFrameCount - bullet.sFrame;
            CalcOrbit(CalcTime(frame), bullet);
        }
    }

    void CalcCollision()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _serverFrameCount - bullet.sFrame;
            CalcLand(bullet);
        }
    }

    void HitBullet()
    {

    }
    void HitGunner()
    {

    }
    void CalcLand(BulletData bullet)
    {
        if (bullet.cPos.y < 0)
        {
            bullet.eFrame = _serverFrameCount;
        }
    }
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
    }
    public float CalcTime(int frame)
    {
        return SERVER_SPF * frame;
    }
    public int TimetoFrame(float time)
    {
        return Mathf.CeilToInt(time / SERVER_SPF);
    }
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
        if (input.number!=0 &&
            _logInput.Find(_ => _.number == input.number - 1) == null)
        {
            Debug.LogWarning("誤り検出");
            return;
        }

        _logInput.Add(input);
        AddBullet(input);
    }
    public void NextFrame(){
        _serverFrameCount++;
    }


    public void Recalc(int pastFrame)
    {

    }
    //1 サーバーフレームの計算
    public void CalcOneServerFrame()
    {
        //List<InputData> nowCalcInput = _logInput.FindAll(_=>_.inFrame == _frameCount);
        for (int i = 0; i < CRIENT_CPF;i ++){
            CalcOneClientFrame();
        }
    }
    //1 クライアントフレームの計算
    void CalcOneClientFrame(){
        CalcBullet();
        CalcCollision();
    }
    public bool IsFinish(){
        return FrameCount >= MAX_FRAME;
    }
    public List<BulletData> HistoryBullets() => _logBullet;
    public List<BulletData> NowBullets() => _logBullet.FindAll(IsNowCalculating);
    
//    public List<>
}