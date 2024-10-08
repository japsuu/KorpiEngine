﻿namespace KorpiEngine.Rendering;

/// <summary>
/// The format of a texture image.<br/>
/// The formats are named according to the following syntax:<br/>
/// [COMPONENTS]_[SIZE]_[TYPE]<br/>
/// - COMPONENTS: List of components the image stores per pixel.<br/>
/// - SIZE: The bit-depth of each component.<br/>
/// - TYPE: The type of the component. (F = Float, UF = Unsigned Float, I = Integer, UI = Unsigned Integer)
/// </summary>
public enum TextureImageFormat
{
    #region BYTE formats

    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// Unsigned 8-bit per channel.
    /// </summary>
    RGBA_8_UF,

    #endregion
    
    
    #region SHORT formats
    
    /// <summary>
    /// Red.
    /// Signed 16-bit per channel.
    /// </summary>
    R_16_F,
    
    /// <summary>
    /// Red and Green.
    /// Signed 16-bit per channel.
    /// </summary>
    RG_16_F,
    
    /// <summary>
    /// Red, Green and Blue.
    /// Signed 16-bit per channel.
    /// </summary>
    RGB_16_F,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// Signed 16-bit per channel.
    /// </summary>
    RGBA_16_F,
    
    /// <summary>
    /// Red.
    /// Unsigned 16-bit per channel.
    /// </summary>
    R_16_UF,
    
    /// <summary>
    /// Red and Green.
    /// Unsigned 16-bit per channel.
    /// </summary>
    RG_16_UF,
    
    /// <summary>
    /// Red, Green and Blue.
    /// Unsigned 16-bit per channel.
    /// </summary>
    RGB_16_UF,
    
    /// <summary>
    /// Red, Green, Blue, and Alpha.
    /// Unsigned 16-bit per channel.
    /// </summary>
    RGBA_16_UF,
    
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