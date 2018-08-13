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
        RPCRequest request = new RPCRequest(2, "hello", 3, 4, true, 3);
        Debug.Log(JsonConvert.SerializeObject(request));
	}

    [Serializable]
    private class RPCRequest
    {
        public long id;
        public string methodName;
        public List<object> args;

        public RPCRequest(long id, string methodName, params object[] args)
        {
            this.id = id;
            this.methodName = methodName;
            this.args = new List<object>(args);
        }
    }
}
