namespace KorpiEngine.Rendering.Cameras;

public enum CameraDebugDrawType
{
    OFF,
    ALBEDO,
    AO,
    NORMAL,
    METALLIC,
    POSITION,
    ROUGHNESS,
    EMISSION,
    VELOCITY,
    OBJECTID,
    DEPTH,
    UNLIT,
    WIREFRAME
}


public static class CameraDebugDrawTypeExtensions
{
    public static CameraDebugDrawType Next(this CameraDebugDrawType type)
    {
        return type switch
        {
            CameraDebugDrawType.OFF => CameraDebugDrawType.ALBEDO,
            CameraDebugDrawType.ALBEDO => CameraDebugDrawType.AO,
            CameraDebugDrawType.AO => CameraDebugDrawType.NORMAL,
            CameraDebugDrawType.NORMAL => CameraDebugDrawType.METALLIC,
            CameraDebugDrawType.METALLIC => CameraDebugDrawType.POSITION,
            CameraDebugDrawType.POSITION => CameraDebugDrawType.ROUGHNESS,
            CameraDebugDrawType.ROUGHNESS => CameraDebugDrawType.EMISSION,
            CameraDebugDrawType.EMISSION => CameraDebugDrawType.VELOCITY,
            CameraDebugDrawType.VELOCITY => CameraDebugDrawType.OBJECTID,
            CameraDebugDrawType.OBJECTID => CameraDebugDrawType.DEPTH,
            CameraDebugDrawType.DEPTH => CameraDebugDrawType.UNLIT,
            CameraDebugDrawType.UNLIT => CameraDebugDrawType.WIREFRAME,
            CameraDebugDrawType.WIREFRAME => CameraDebugDrawType.OFF,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    
    public static string AsShaderKeyword(this CameraDebugDrawType type)
    {
        return type switch
        {
            CameraDebugDrawType.OFF => "OFF",
            CameraDebugDrawType.ALBEDO => "ALBEDO",
            CameraDebugDrawType.AO => "AO",
            CameraDebugDrawType.NORMAL => "NORMAL",
            CameraDebugDrawType.METALLIC => "METALLIC",
            CameraDebugDrawType.POSITION => "POSITION",
            CameraDebugDrawType.ROUGHNESS => "ROUGHNESS",
            CameraDebugDrawType.EMISSION => "EMISSION",
            CameraDebugDrawType.VELOCITY => "VELOCITY",
            CameraDebugDrawType.OBJECTID => "OBJECTID",
            CameraDebugDrawType.DEPTH => "DEPTH",
            CameraDebugDrawType.UNLIT => "UNLIT",
            CameraDebugDrawType.WIREFRAME => "WIREFRAME",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}