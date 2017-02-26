using System.Collections;
using System.Collections.Generic;
using UnetLLAPISample;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TextureSender : MonoBehaviour
{
    public LLAPINetworkManager NetworkManager;
    public float Interval = 3f;
    Type textureType;
    Texture2D texture2D;
    RenderTexture renderTexture;

    void Start()
    {
        StartCoroutine(SendTextureLoop());
    }

    void Update()
    {
    }

    void SendTexture()
    {
        var texture = GetComponent<Renderer>().material.mainTexture;
        textureType = texture.GetType();
        if (textureType == typeof(Texture2D))
        {
            texture2D = texture as Texture2D;
        }
        else if (textureType == typeof(RenderTexture))
        {
            renderTexture = texture as RenderTexture;
        }

        if (textureType == typeof(RenderTexture))
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture2D = new Texture2D(renderTexture.width, renderTexture.height);
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
        }

        var pngTexture = texture2D.EncodeToPNG();
        NetworkManager.SendPacketData(pngTexture, QosType.UnreliableFragmented);
    }

    IEnumerator SendTextureLoop()
    {
        while (true)
        {
            SendTexture();
            yield return new WaitForSeconds(Interval);
        }
    }
}
