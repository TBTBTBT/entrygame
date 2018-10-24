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

    class GunnerViewData
    {
        public GameObject Model;//いずれ履帯、砲塔に分ける
        public int Id;
    }
    [SerializeField] GameObject _stageModel;
    [SerializeField] GameObject _bulletModel;
//    [SerializeField] GameObject _gunnerModel;
    [SerializeField] Transform _3DRoot;
    private List<BulletViewData> _bulletInstance = new List<BulletViewData>();
    private List<GunnerViewData> _gunnerInstance = new List<GunnerViewData>();
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
        foreach (var gunner in _logicManager.Gunners)
        {
            _gunnerInstance.Add(new GunnerViewData(){
                Model = Instantiate(_bulletModel, _3DRoot),
                Id = gunner.id
            });
        }
        Next(State.Loop);
        yield return null;
    }
	IEnumerator Loop(){
        while(!_logicManager.IsFinish){
            foreach (var gunner in _logicManager.Gunners)
            {
                GunnerViewData data = _gunnerInstance.Find(_ => _.Id == gunner.id);
                data.Model.transform.position = gunner.cPos / 10;
            }
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
