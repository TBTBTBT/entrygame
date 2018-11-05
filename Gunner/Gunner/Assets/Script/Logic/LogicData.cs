using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//送受信する
public class PlayerData
{
    public int id;
    public Vector2 sPos;
    public Vector2 cPos;
    public float speed;
    public int Direction = 1;// 左向き -1
    public int sHp;
    public int cHp;
    public int cKnockback;//今までのkb値の累計
    public float rad;
    public void CalcPos(float time)
    {
        cPos = sPos + new Vector2(Direction * (speed * time) + cKnockback, 0);
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
public class UnitData
{
    public int number; //固有のID 追跡用

    public int weight;
    public int unitId; // MstBulletRecord のID
    public int playerId; // 砲撃者のID
    public int sFrame;//計算開始フレーム
    //public int ieFrame = -1;//計算終了フレーム(仮)
    public int eFrame = -1;//計算終了フレーム(真)
    public float strength;// 1/10
    public float angle;// 1/10
    public float cRad;
    public Vector2 sPos;//始点

    //public Vector2 iePos;//imaginary終点 そのまま着弾した場合の終点
    public Vector2 ePos;//true終点 相手の弾によっても変化
    public Dictionary<int, int> hitGunnerIdsAndDamage;//ヒットしたプレイヤーと受けたダメージのペア
    //見た目のみのために必要
    public Vector2 cPos;//現在の点

    public void CalcOrbit(float time)
    {
        MstBulletRecord mstBullet = MasterdataManager.Get<MstBulletRecord>(bulletId);
        float cx = sPos.x + (time * strength * Mathf.Cos(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        float cy = sPos.y + (time * strength * Mathf.Sin(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        cy += mstBullet.gravy * time * time / 2;
        float rad = mstBullet.rad + mstBullet.radratio * time;
        cRad = rad;
        cPos.x = cx;
        cPos.y = cy;
    }
}
#if false
//送受信する
public class GunnerData
{
    public int id;
    public Vector2 sPos;
    public Vector2 cPos;
    public float speed;
    public int Direction = 1;// 左向き -1
    public int sHp;
    public int cHp;
    public int cKnockback;//今までのkb値の累計
    public float rad;
    public void CalcPos(float time)
    {
        cPos = sPos + new Vector2(Direction * (speed * time) + cKnockback, 0);
    }

}
//送受信しない
public class BulletData
{
    public int knockback;
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
    public Dictionary<int, int> hitGunnerIdsAndDamage;//ヒットしたプレイヤーと受けたダメージのペア
    //見た目のみのために必要
    public Vector2 cPos;//現在の点

    public void CalcOrbit(float time)
    {
        MstBulletRecord mstBullet = MasterdataManager.Get<MstBulletRecord>(bulletId);
        float cx = sPos.x + (time * strength * Mathf.Cos(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        float cy = sPos.y + (time * strength * Mathf.Sin(angle * Mathf.Deg2Rad)) * mstBullet.weight;
        cy += mstBullet.gravy * time * time / 2;
        float rad = mstBullet.rad + mstBullet.radratio * time;
        cRad = rad;
        cPos.x = cx;
        cPos.y = cy;
    }
}
#endif

