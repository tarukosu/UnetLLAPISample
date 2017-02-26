using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnetLLAPISample;
using UnityEngine;
using UnityEngine.Networking;

public class TextureReceiver : MonoBehaviour {
    public LLAPINetworkManager NetworkManager;
    Texture2D mainTexture;

    private void Awake()
    {
        mainTexture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        NetworkManager.OnDataReceived += OnDataReceived;
    }

    void Start () {
    }

    void Update () {
	}

    void OnDataReceived(object o, LLAPINetworkEventArgs args)
    {
        Debug.Log("texture data received");
        ApplyTextureData(args.data);
    }

    void ApplyTextureData(byte[] data)
    {
        using (MemoryStream inputStream = new MemoryStream(data))
        {
            BinaryReader reader = new BinaryReader(inputStream);
            byte[] readBinary = reader.ReadBytes((int)reader.BaseStream.Length);
            // get texture size
            int pos = 16;
            int width = 0;
            for (int i = 0; i < 4; i++)
            {
                width = width * 256 + readBinary[pos++];
            }
            int height = 0;
            for (int i = 0; i < 4; i++)
            {
                height = height * 256 + readBinary[pos++];
            }

            var texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);
            Destroy(GetComponent<Renderer>().material.mainTexture);
            GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
