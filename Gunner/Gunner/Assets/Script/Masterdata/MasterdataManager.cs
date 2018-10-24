using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MasterdataManager : SingletonMonoBehaviour<MasterdataManager>
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
        new MstBulletRecord(){id = 10,gravy = -20f,gravx = 0.95f,rad = 10,weight = 1f}
    };

}
