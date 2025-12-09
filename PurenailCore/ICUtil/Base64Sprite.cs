using ItemChanger;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace PurenailCore.ICUtil;

internal class Base64Sprite : ISprite
{
    public string Base64Payload = "";

    Base64Sprite() { }
    Base64Sprite(string base64Payload)
    {
        Base64Payload = base64Payload;
    }

    private Sprite? sprite;

    private Sprite LoadSprite()
    {
        if (sprite != null) return sprite;
        if (Base64Payload == null) return null;

        byte[] data = Convert.FromBase64String(Base64Payload);
        MemoryStream stream = new(data);
        sprite = ItemChanger.Internal.SpriteManager.Load(stream);
        return sprite;
    }

    [JsonIgnore]
    public Sprite Value => LoadSprite();

    public ISprite Clone() => new Base64Sprite(Base64Payload);
}
