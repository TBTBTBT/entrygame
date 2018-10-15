using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//送受信する
public class GunnerData
{
    public int id;
    public int x;
    public int y;
    public int hp;

}
public enum InputType
{
    BULLET = 1,
    SKIP = 2,
    FORWARD = 3,
    BACKWARD = 4
}
public class InputData
{
    public int gunnerId;
    public int bulletId;
    public int inFrame;
    public InputType type;
    public int strength;
    public int angle;
}
//送受信しない
public class BulletData
{
    public int id; //固有のID 追跡用
    public int bulletId; // MstBulletRecord のID
    public int gunnerId; // 砲撃者のID
    public int sFrame;//計算開始フレーム
    public int eFrame = -1;//計算終了フレーム
    public float strength;// 1/10
    public float angle;// 1/10
    public float cRad;
    public Vector2 sPos;//始点
    public Vector2 cPos;//現在の点
   // public int ex;//終点 相手の弾によっても変化
   // public int ey;

}
public class MstBulletRecord{
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


public static class MasterDataManager{
    public static T Get<T>(int id) where T : class {
        if(typeof(T) == typeof(MstBulletRecord)){
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
    private float SPF = 0.2f;//1計算fあたりの経過秒数
    //data
    private int _frameCount = 0;
    private int _bulletCount = 0;
    private List<InputData> _logInput = new List<InputData>();
    private List<BulletData> _logBullet = new List<BulletData>();
    public List<GunnerData> Gunners { get; set; }
    public int FrameCount { get { return _frameCount; }}

    public void Init(List<GunnerData> gunners){
        Gunners = new List<GunnerData>();
        foreach(var gunner in gunners){
            Gunners.Add(gunner);
        }
        _frameCount = 0;
        _bulletCount = 0;
    }


    void AddBullet(InputData input)
    {
        if (input.type == InputType.BULLET)
        {
            _logBullet.Add(InitBullet(input));
        }
    }

    BulletData InitBullet(InputData input)
    {
        BulletData ret = new BulletData();
        GunnerData gunner = Gunners.Find(_=>_.id == input.gunnerId);
        if (gunner == null)
        {
            Debug.LogError("存在しないプレイヤーID");
            return ret;
        }
        ret.sFrame = input.inFrame;
        ret.eFrame = -1;
        ret.id = input.bulletId;
        ret.gunnerId = input.gunnerId;
        ret.sPos = new Vector2(gunner.x, gunner.y);
        ret.cPos = new Vector2(gunner.x, gunner.y);
        ret.strength = input.strength;
        ret.angle = (float)input.angle / 10;
        ret.cRad = 0;
        return ret;
    }

    bool IsNowCalculating(BulletData bullet)
    {
        return bullet.sFrame <= _frameCount && bullet.eFrame <= -1;
    }
    void CalcBullet()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _frameCount - bullet.sFrame;
            CalcOrbit(CalcTime(frame), bullet);
        }
    }

    void CalcCollision()
    {
        List<BulletData> nowCalcBullet = _logBullet.FindAll(IsNowCalculating);
        foreach (BulletData bullet in nowCalcBullet)
        {
            int frame = _frameCount - bullet.sFrame;
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
            bullet.eFrame = _frameCount;
        }
    }
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
        cy += gy * time * time;
//        cx *= gx;
        bullet.cRad = rad;
        bullet.cPos.x = cx;
        bullet.cPos.y = cy;
    }
    public float CalcTime(int frame)
    {
        return SPF * frame;
    }

    public void AddInput(InputData input)
    {
        _logInput.Add(input);
        AddBullet(input);
    }
    public void NextFrame(){
        _frameCount++;
    }


    public void Recalc(int pastFrame)
    {

    }

    public void CalcFrame()
    {
        //        List<InputData> nowCalcInput = _logInput.FindAll(_=>_.inFrame == _frameCount);
        CalcBullet();
        CalcCollision();
    }

    public List<BulletData> HistoryBullets() => _logBullet;
    public List<BulletData> NowBullets() => _logBullet.FindAll(IsNowCalculating);
    
//    public List<>
}