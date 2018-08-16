using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;

public class JsonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        TestObj obj = new TestObj
        {
            foo = 32,
            bar = "hello"
        };
        Debug.Log(JsonConvert.SerializeObject(obj));
        RPC.Instance.ExecuteStaticMethod<object>("test.Test","printObj", new string[] { "test.Test" }, new object[] { obj });
	}

    [Serializable]
    private class TestObj
    {
        public long foo;
        public String bar;
    }
}
