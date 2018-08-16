using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class RPC
{
    public static RPC Instance
    {
        get { return Nested.instance; }
    }

    private TcpClient client;
    private StreamReader stream;
    private long id = 0;

    public RPC()
    {
        Debug.Log("Connecting...");
        client = new TcpClient("localhost", 4444);
        Debug.Log("Connected!");
        stream = new StreamReader(client.GetStream(), Encoding.UTF8);
    }

    public T ExecuteStaticMethod<T>(string className, string methodName)
    {
        return ExecuteStaticMethod<T>(className, methodName, new string[] { }, new object[] { });
    }

    public T ExecuteStaticMethod<T>(string className, string methodName, string[] argClassNames, object[] args)
    {
        RPCRequest request = new RPCRequest
        {
            id = id++,
            instantiate = false,
            className = className,
            objectName = "static",
            methodName = methodName,
            argClassNames = new List<string>(argClassNames),
            args = new List<object>(args)
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        Debug.Log("Sending request: " + jsonRequest);
        WriteLine(jsonRequest);

        string jsonResponse = stream.ReadLine();
        Debug.Log("Received response: " + jsonResponse);

        RPCResponse<T> response = JsonConvert.DeserializeObject<RPCResponse<T>>(jsonResponse);
        if (request.id == response.id)
        {
            return response.value;
        }
        Debug.LogWarning("Somehow the calls are out of sync! Are you using multithreading?");
        return default(T);
    }

    public T ExecuteMethod<T>(string objectName, string methodName)
    {
        return ExecuteMethod<T>(objectName, methodName, new string[] { }, new object[] { });
    }

    public T ExecuteMethod<T>(string objectName, string methodName, string[] argClassNames, object[] args)
    {
        RPCRequest request = new RPCRequest
        {
            id = id++,
            instantiate = false,
            className = "",
            objectName = objectName,
            methodName = methodName,
            argClassNames = new List<string>(argClassNames),
            args = new List<object>(args)
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        Debug.Log("Sending request: " + jsonRequest);
        WriteLine(jsonRequest);

        string jsonResponse = stream.ReadLine();
        Debug.Log("Received response: " + jsonResponse);

        RPCResponse<T> response = JsonConvert.DeserializeObject<RPCResponse<T>>(jsonResponse);
        if(request.id == response.id)
        {
            return response.value;
        }
        Debug.LogWarning("Somehow the calls are out of sync! Are you using multithreading?");
        return default(T);
    }
    
    public bool InstantiateObject(string className, string objectName)
    {
        return InstantiateObject(className, objectName, new string[] { }, new object[] { });
    }

    public bool InstantiateObject(string className, string objectName, string[] argClassNames, object[] args)
    {
        RPCRequest request = new RPCRequest
        {
            id = id++,
            instantiate = true,
            className = className,
            objectName = objectName,
            methodName = "",
            argClassNames = new List<string>(argClassNames),
            args = new List<object>(args)
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        Debug.Log("Sending request: " + jsonRequest);
        WriteLine(jsonRequest);

        string jsonResponse = stream.ReadLine();
        Debug.Log("Received response: " + jsonResponse);

        RPCResponse<bool> response = JsonConvert.DeserializeObject<RPCResponse<bool>>(jsonResponse);
        if (request.id == response.id)
        {
            return response.value;
        }
        Debug.LogWarning("Somehow the calls are out of sync! Are you using multithreading?");
        return false;
    }

    private void WriteLine(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message + "\n");
        stream.BaseStream.Write(msg, 0, msg.Length);
        stream.BaseStream.Flush();
    }

    [Serializable]
    private class RPCRequest
    {
        public long id;
        public bool instantiate;
        public string className;
        public string objectName;
        public string methodName;
        public List<string> argClassNames;
        public List<object> args;
    }

    [Serializable]
    private class RPCResponse<T>
    {
        public long id;
        public T value;
    }

    private class Nested
    {
        static Nested()
        {

        }

        internal static readonly RPC instance = new RPC();
    }
}
