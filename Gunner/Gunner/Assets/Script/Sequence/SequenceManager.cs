using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SequenceManager : SingletonMonoBehaviourWithStatemachine<SequenceManager, SequenceManager.State>
{
    public enum State
    {
        Init,
        Advertise,
        Title,
        Home,


    }
    IEnumerator Init()
    {
        yield return MasterdataManager.Instance.InitMasterdataAsync();
        yield return null;
    }
    void NextScene(string scene){
        SceneManager.LoadScene(scene);
    }
}
