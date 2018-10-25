using System;
//-----------------------------------------------------------------------------

//マスターデータの定義ファイル
//定義方法 
// IMasterRecordを実装したクラスを作る
// MasterPath属性でjsonのパスを指定(Asset/Resource以下)
//ひな形
//[MasterPath("")]
//public class MstNameRecord : IMasterRecord{public int id { get; set; }}

//-----------------------------------------------------------------------------

[Serializable]
[MasterPath("/Master/mst_bullet.json")]
public class MstBulletRecord : IMasterRecord
{
    public int id { get; set; }
    public float gravx;//重力
    public float gravy;
    public float weight;//速さに影響 0~1 0が再重
    public int atk;//攻撃力
    public int atkratio; //攻撃力増減
    public float rad;//半径
    public float radratio;//半径増減
    public float exprad;//爆発半径
    public int prevent;//弾同士ぶつかるか

}
[Serializable]
[MasterPath("/Master/mst_gunner.json")]
public class MstGunnerRecord : IMasterRecord
{
    public int id { get; set; }
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