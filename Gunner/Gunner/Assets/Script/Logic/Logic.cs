using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//serverが計算する必要すらないかも
//serverではフレームカウントしない方式に 20181022
public class Logic
{


    private readonly float SPF = 0.02f; //使わなくしたい
    //private List<InputData> _logInput = new List<InputData>();
    //private int _inputNumber = -1;
    private List<BulletData> _logBullet = new List<BulletData>();//計算終了
    private List<BulletData> _nowBullet = new List<BulletData>();//計算中
    private List<BulletData> _reserveBullet = new List<BulletData>();//計算予約　
    //----------------------------------------------------------------------------
    //public method
    //----------------------------------------------------------------------------
    public List<GunnerData> Gunners { get; set; }
    public List<BulletData> HistoryBullets() => _logBullet;
    public List<BulletData> NowBullets() => _nowBullet;


    //入力
    public void AddInput(InputData input)
    {

        //_inputNumber = input.number;
        AddBullet(input);
    }
    //初期化
    public void Init(List<GunnerData> gunners)
    {
        //_inputNumber = -1;
        Gunners = new List<GunnerData>();
        foreach (var gunner in gunners)
        {
            Gunners.Add(gunner);
        }
    }


    //1フレーム計算
    public void Calc(int frame)
    {
        UpdateBulletList(frame);
        //cPos更新
        CalcBullet(frame);
        CalcGunner(frame);
        //cPos使って判定
        CalcCollision(frame);
    }
    //特定フレームまで巻き戻し
    public void RollBack(int frameCount)
    {
        List<BulletData> reCalcBullet = _logBullet.FindAll(_ => IsNowCalculating(_, frameCount));
        List<BulletData> futureBullet = _logBullet.FindAll(_ => IsFutureCalcurate(_, frameCount));
        futureBullet.AddRange(_nowBullet.FindAll(_ => IsFutureCalcurate(_, frameCount)));
        _logBullet.RemoveAll(_ => IsNowCalculating(_, frameCount) || IsFutureCalcurate(_, frameCount));
        _nowBullet.RemoveAll(_ => IsFutureCalcurate(_, frameCount));
        _reserveBullet.AddRange(futureBullet);
        _nowBullet.AddRange(reCalcBullet);
    }

    int NowDamage(int id) => _logBullet.Aggregate(0, (dmg, bullet) => bullet.hitGunnerIdsAndDamage.ContainsKey(id) ? dmg + bullet.hitGunnerIdsAndDamage[id] : dmg);

    private void UpdateBulletList(int frameCount)
    {
        //状態アップデート
        //リストアップ
        List<BulletData> nowCalcBullet = _reserveBullet.FindAll(_ => IsNowCalculating(_, frameCount));
        List<BulletData> logBullet = _nowBullet.FindAll(_ => !IsNowCalculating(_, frameCount));
        //削除
        _reserveBullet.RemoveAll(_ => IsNowCalculating(_, frameCount));
        _nowBullet.RemoveAll(_ => !IsNowCalculating(_, frameCount));
        //追加
        _nowBullet.AddRange(nowCalcBullet);
        _logBullet.AddRange(logBullet);
    }

    void AddBullet(InputData input)
    {
        if (input.type == "bullet")
        {
            _reserveBullet.Add(InitBullet(input));
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

    float FrameToTime(int frame)
    {
        return frame * SPF;
    }
    //弾の処理
    void CalcBullet(int frameCount)
    {
        //List<BulletData> nowCalcBullet = _logBullet.FindAll(_ => IsNowCalculating(_, frameCount));//現在計算されるべき弾
        foreach (BulletData bullet in _nowBullet)
        {
            int frame = frameCount - bullet.sFrame;
            bullet.CalcOrbit(FrameToTime(frame));//cPos更新
           
        }
    }


    void CalcGunner(int frameCount)
    {
        foreach (var gunner in Gunners)
        {
            gunner.CalcPos(FrameToTime(frameCount));
        }
    }
    void CalcCollision(int frameCount)
    {
        
        foreach (BulletData bullet in _nowBullet)
        {
            int frame = frameCount - bullet.sFrame;
           // HitGunner(_frameCount,bullet,Gunners);
            //HitBullet(_frameCount,bullet);
            HitLand(frameCount, bullet);
            HitGunner(frameCount, bullet, Gunners);
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
        bool isHit = false;
        foreach (var gunner in gunners)
        {
            if(bullet.gunnerId == gunner.id){
                continue;
            }
            if (!CheckCollision(gunner.cPos, gunner.rad, bullet.cPos, bullet.cRad))
            {
                continue;
            }
            if (bullet?.eFrame == -1 || bullet?.eFrame >= frame)
            {
                isHit = true;
                bullet.eFrame = frame;
                //マスターから引っ張る
                bullet.hitGunnerIdsAndDamage.Add(gunner.id, 100);
            }
        }

        if (isHit)
        {
            foreach (var gunner in gunners)
            {
                gunner.cHp = NowDamage(gunner.id);
            }
        }
    }

    bool CheckCollision(Vector2 pos1 ,float rad1 ,Vector2 pos2 ,float rad2)
    {
        return Mathf.Abs((pos2 - pos1).magnitude) <= rad1 + rad2;
    }
   

    bool IsNowCalculating(BulletData bullet,int now)
    { //現在のフレームが計算開始フレーム以上かつ、（　計算が終了していない または 計算終了フレーム以下。(再計算対応))
        return bullet?.sFrame <= now && ( bullet?.eFrame == -1 || bullet?.eFrame >= now);
    }
    //まだ計算開始してない
    bool IsFutureCalcurate(BulletData bullet, int past)
    {
        return bullet?.sFrame > past ;
    }

}

#if false
//------------------------------------------
//ずれ訂正
//早送り(クライアントが遅れ)
//巻き戻し
//------------------------------------------
//誤り訂正
//再入力対策
//入力飛ばし対策

//------------------------------------------
//内部呼出し

/*
int loopMax = 500;
int loopCnt = 0;
while ((input.inFrame - _addFrame) > _frameCount && loopCnt < loopMax)
{
    SkipFrame();
    TimeCollectForward();
    Debug.Log((input.inFrame - _addFrame) - _frameCount);
    loopCnt++;
    //     CalcOneFrame();
    //     NextFrame();
}*/
/*
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

//誤りを探す
//1すでに受け取っていた場合
if (_inputNumber >= input.number)
{
    Debug.LogWarning("誤り検出");
    return;
}
//2飛ばして受け取った場合
if (_inputNumber + 1 < input.number)
{
    Debug.LogWarning("誤り検出");
    return;
}*/


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