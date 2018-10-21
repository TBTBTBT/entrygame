using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameViewManager : MonoBehaviourWithStatemachine<GameViewManager.State> {
    public enum State{
        None,
        Init,
        Loop,
        End,
    }
    class BulletViewData
    {
        public GameObject Model;
        public int number;
    }    
    [SerializeField] GameObject _stageModel;
    [SerializeField] GameObject _bulletModel;
    [SerializeField] Transform _3DRoot;
    List<BulletViewData> _bulletInstance = new List<BulletViewData>();
    LogicManager _logicManager;
    public void SetupView(LogicManager logic){
        _bulletInstance.Clear();
        _logicManager = logic;
        Next(State.Init);
    }
    IEnumerator Init(){
        Instantiate(_stageModel, _3DRoot);
        while(!_logicManager.IsReady){
            yield return null;
        }
        Next(State.Loop);
        yield return null;
    }
	IEnumerator Loop(){
        while(!_logicManager.IsFinish){
            foreach(var bullet in _logicManager.NowBullets){
                BulletViewData data = _bulletInstance.Find(_ => _.number == bullet.number);
                if(data == null){
                    data = new BulletViewData()
                    {
                        number = bullet.number,
                        Model = Instantiate(_bulletModel, _3DRoot)
                    };
                    _bulletInstance.Add(data);
                }
                data.Model.transform.localPosition = bullet.cPos/10;
            }
            yield return null;
        }
        Next(State.End);
    }
}
