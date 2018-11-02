using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUtility : MonoBehaviour {

	public DateTimeOffset Now => DateTimeOffset.Now;
    public TimeSpan ElapsedFrom(DateTimeOffset stamp)=> DateTimeOffset.Now - stamp;
}
