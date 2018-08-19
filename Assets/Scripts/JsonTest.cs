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
        Debug.Log(JsonConvert.SerializeObject(new Exception("Hello!")));
	}
}
