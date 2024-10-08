﻿using KorpiEngine.Networking.Multiplayer.LowLevel;

namespace KorpiEngine.Networking.Multiplayer.HighLevel.Messages;

public class AuthRequestNetMessage : NetMessage
{
    public byte AuthenticationMethod { get; private set; }


    public AuthRequestNetMessage(byte authenticationMethod)
    {
        AuthenticationMethod = authenticationMethod;
    }


    protected override void SerializeInternal(BitBuffer buffer)
    {
        buffer.AddByte(AuthenticationMethod);
    }


    protected override void DeserializeInternal(BitBuffer buffer)
    {
        AuthenticationMethod = buffer.ReadByte();
    }
}