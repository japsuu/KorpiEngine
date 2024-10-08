﻿namespace KorpiEngine.Threading;

/// <summary>
/// Represents an object that can pool jobs.
/// </summary>
public interface IJobPool
{
    public void EnqueueWorkItem(IKorpiJob korpiJob);
    
    
    public void FixedUpdate();
    
    
    public void Shutdown();
}