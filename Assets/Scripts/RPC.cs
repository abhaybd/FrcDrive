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
    private static RPC instance = null;
    public static RPC Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new RPC();
            }
            return instance;
        }
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
        return SendRPCRequest<T>(false, className, "", methodName, argClassNames, args);
    }

    public T ExecuteMethod<T>(string objectName, string methodName)
    {
        return ExecuteMethod<T>(objectName, methodName, new string[] { }, new object[] { });
    }

    public T ExecuteMethod<T>(string objectName, string methodName, string[] argClassNames, object[] args)
    {
        return SendRPCRequest<T>(false, "", objectName, methodName, argClassNames, args);
    }
    
    public T InstantiateObject<T>(string className, string objectName)
    {
        return InstantiateObject<T>(className, objectName, new string[] { }, new object[] { });
    }

    public T InstantiateObject<T>(string className, string objectName, string[] argClassNames, object[] args)
    {
        return SendRPCRequest<T>(true, className, objectName, "", argClassNames, args);
    }

    private T SendRPCRequest<T>(bool instantiate, string className, string objectName, string methodName, string[] argClassNames, object[] args)
    {
        RPCRequest request = new RPCRequest
        {
            id = id++,
            instantiate = instantiate,
            className = className,
            objectName = objectName,
            methodName = methodName,
            argClassNames = new List<string>(argClassNames),
            args = new List<object>(args)
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        //Debug.Log("Sending request: " + jsonRequest);
        WriteLine(jsonRequest);

        string jsonResponse = stream.ReadLine();
        //Debug.Log("Received response: " + jsonResponse);

        RPCResponse<object> tempResponse = JsonConvert.DeserializeObject<RPCResponse<object>>(jsonResponse);

        if(tempResponse.isException)
        {
            RPCResponse<string> exceptionResponse = JsonConvert.DeserializeObject<RPCResponse<string>>(jsonResponse);
            throw new Exception(exceptionResponse.value);
        }

        RPCResponse<T> response = JsonConvert.DeserializeObject<RPCResponse<T>>(jsonResponse);
        if (request.id == response.id)
        {
            return response.value;
        }
        throw new RPCException("Somehow the calls are out of sync! Are you using multithreading?");
    }

    private void WriteLine(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message + "\n");
        stream.BaseStream.Write(msg, 0, msg.Length);
        stream.BaseStream.Flush();
    }

    private class RPCException : Exception
    {
        public RPCException(string message) : base(message) {}
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
        public bool isException;
        public T value;
    }
}
