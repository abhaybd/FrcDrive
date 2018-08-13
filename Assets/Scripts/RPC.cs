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

    public object ExecuteMethod(Type returnType, string methodName, params object[] args)
    {
        RPCRequest request = new RPCRequest(id++, methodName, args);
        string jsonRequest = JsonConvert.SerializeObject(request);
        Debug.Log("Sending request: " + jsonRequest);
        byte[] msg = Encoding.UTF8.GetBytes(jsonRequest + "\n");
        stream.BaseStream.Write(msg, 0, msg.Length);
        stream.BaseStream.Flush();

        string jsonResponse = stream.ReadLine();
        Debug.Log("Received response: " + jsonResponse);
        return JsonConvert.DeserializeObject(jsonResponse, returnType);
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

    private class Nested
    {
        static Nested()
        {

        }

        internal static readonly RPC instance = new RPC();
    }
}
