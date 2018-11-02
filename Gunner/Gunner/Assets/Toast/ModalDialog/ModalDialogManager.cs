using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class ModalDialogManager : SingletonMonoBehaviourWithStatemachine<ModalDialogManager,ModalDialogManager.State> {
    public enum State
    {
        Init,
        Standby,
        Open,
        Display,
        Close,
    }
    public enum DialogType
    {
        System,
        App,
    }

    [SerializeField] private RectTransform _dialogRoot;
    [SerializeField] private GameObject _dialogPrefab;

    IEnumerator Init()
    {
        yield return null;

    }
    IEnumerator Standby()
    {
        yield return null;

    }
    IEnumerator Open()
    {
        yield return null;

    }
    //出すだけ
    public static void Open(ModalDialogElement element,DialogType type = DialogType.App)
    {
        var dialog = Instance.InstantiateDialog(Instance._dialogPrefab);
        dialog.Setup(element);
    }
    //シーケンス内で呼び出し、閉じるまで待つ
    public static IEnumerator OpenInProcess(ModalDialogElement element, DialogType type = DialogType.App)
    {
        /*
        if (Instance.Current == State.Init)
        {
            yield break;
        }*/
        yield return Instance.OpenAndWaitForClose(element, type);
        
    }
    //エラー時など全削除
    public static void EmergencyDestroyAll()
    {
        foreach (Transform n in Instance._dialogRoot)
        {
            Destroy(n.gameObject);
        }
    }
    IEnumerator OpenAndWaitForClose(ModalDialogElement element, DialogType type = DialogType.App)
    {
        var dialog = InstantiateDialog(_dialogPrefab);
        dialog.Setup(element);
        yield return dialog.WaitForEndState();
        DestroyDialog(dialog);
    }

    void DestroyDialog(ModalDialogBase dialog)
    {
        Destroy(dialog.gameObject);
    }
    ModalDialogBase InstantiateDialog(GameObject prefab)
    {
        var dialog = Instantiate(prefab, _dialogRoot);
        return dialog.GetComponent<ModalDialogBase>();
    }
}

