namespace KorpiEngine.Core.Rendering.Primitives;

public enum TextureImageFormat
{
    #region COMMON RGBA formats

    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// Unsigned 8-bit per channel.
    /// </summary>
    RGBA_8,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// Unsigned 16-bit per channel.
    /// </summary>
    RGBA_16,

    #endregion


    #region FLOAT formats

    /// <summary>
    /// Red.
    /// 32-bit float per channel.
    /// </summary>
    R_32_F,
    
    /// <summary>
    /// Red and Green.
    /// 32-bit float per channel.
    /// </summary>
    RG_32_F,
    
    /// <summary>
    /// Red, Green, and Blue.
    /// 32-bit float per channel.
    /// </summary>
    RGB_32_F,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// 32-bit float per channel.
    /// </summary>
    RGBA_32_F,

    #endregion


    #region INTEGER formats

    /// <summary>
    /// Red.
    /// 32-bit integer per channel.
    /// </summary>
    R_32_I,
    
    /// <summary>
    /// Red and Green.
    /// 32-bit integer per channel.
    /// </summary>
    RG_32_I,
    
    /// <summary>
    /// Red, Green, and Blue.
    /// 32-bit integer per channel.
    /// </summary>
    RGB_32_I,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// 32-bit integer per channel.
    /// </summary>
    RGBA_32_I,

    #endregion


    #region UNSIGNED INTEGER formats

    /// <summary>
    /// Red.
    /// 32-bit unsigned integer per channel.
    /// </summary>
    R_32_UI,
    
    /// <summary>
    /// Red and Green.
    /// 32-bit unsigned integer per channel.
    /// </summary>
    RG_32_UI,
    
    /// <summary>
    /// Red, Green, and Blue.
    /// 32-bit unsigned integer per channel.
    /// </summary>
    RGB_32_UI,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// 32-bit unsigned integer per channel.
    /// </summary>
    RGBA_32_UI,

    #endregion


    #region DEPTH formats

    /// <summary>
    /// Single-channel depth.
    /// 16-bits per channel.
    /// </summary>
    DEPTH_16,
    
    /// <summary>
    /// Single-channel depth.
    /// 24-bits per channel.
    /// </summary>
    DEPTH_24,
    
    /// <summary>
    /// Single-channel depth.
    /// 32-bit float per channel.
    /// </summary>
    DEPTH_32_F,

    /// <summary>
    /// 24-bit depth and 8-bit stencil.
    /// </summary>
    DEPTH_24_STENCIL_8

    #endregion
}