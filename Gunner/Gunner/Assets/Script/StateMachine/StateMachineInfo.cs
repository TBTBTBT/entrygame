using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public static class StatemachineInfo
{
  
    public static List<WeakReference<IStatemachine>> Statemachines = new List<WeakReference<IStatemachine>>();
    public static void Add(IStatemachine sm ){
        
        Statemachines.Add(new WeakReference<IStatemachine>(sm));

        //Statemachines.Clear();
    }
}