using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterTable<T>
{
    public T[] Records { get; private set; } //静的に確保

}
