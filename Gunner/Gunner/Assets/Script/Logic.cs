using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GunnerData
{
    public int id;
    public int x;
    public int y;
    public int hp;

}

public class BulletData
{
    public int sFrame;//計算開始フレーム
    public int eFeame;//計算終了フレーム
    public int sx;//始点
    public int sy;
    public int ex;//終点 相手の弾によっても変化
    public int ey;
    public int id; // MstBulletRecord のID
}
public class MstBulletRecord{
    public int id;
    public int gravx;//重力
    public int gravy;
    public int weight;//速さに影響
    public int atk;//攻撃力
    public int atkRatio; //攻撃力増減
    public int rad;//半径
    public int radRatio;//半径増減
    public int expRad;//爆発半径
}
public class InputData
{
    public int id;
    public int type;
    public int strength;
    public int angle;
}
public static class MasterDataManager{
    public static T Get<T>(int id) where T : class {
        if(typeof(T) == typeof(MstBulletRecord)){
            return (T)(object)MstBulletRecordTable.First(_ => _.id == id);
        }
        return null;
    }
    public static MstBulletRecord[] MstBulletRecordTable = {
        new MstBulletRecord(){id = 10}
    };
}

public class Logic
{
    //serverが計算する必要すらないかも
    //define
    private float SPF = 0.2f;//1計算fあたりの経過秒数
    //data
    private int _frameCount = 0;
    private List<InputData> _log = new List<InputData>();
    public List<GunnerData> Gunners { get; set; }
    public int FrameCount { get { return _frameCount; }}
    public void Init(List<GunnerData> gunners){
        Gunners = new List<GunnerData>();
        foreach(var gunner in gunners){
            Gunners.Add(gunner);
        }
        _frameCount = 0;
    }
    public float CalcTime(int frame)
    {
        return SPF * frame;
    }
    public void CalcOrbit(float time, InputData input, GunnerData gunner, BulletData bullet)
    {
        var gx = bullet.gravx;
        var gy = bullet.gravy;
        var wei = bullet.weight;
        var sx = gunner.x;
        var sy = gunner.y;
        var str = input.strength;
        var ang = input.angle;

    }
    public void NextFrame(){
        _frameCount++;
    }

    public void CalcFrame()
    {
        

    }
    public List<>
}