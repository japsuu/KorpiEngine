using System.Runtime.CompilerServices;
using ImGuiNET;
using KorpiEngine.Rendering;
using KorpiEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace KorpiEngine.UI.DearImGui;

internal class ImGuiController : GraphicsResource
{
    private readonly GameWindow _window;

    private bool _frameBegun;

    private int _vertexArray;
    private int _vertexBuffer;
    private int _vertexBufferSize;
    private int _indexBuffer;
    private int _indexBufferSize;

    private int _fontTexture;

    private int _shader;
    private int _shaderFontTextureLocation;
    private int _shaderProjectionMatrixLocation;

    private int _windowWidth;
    private int _windowHeight;

    private readonly System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    private static bool KhrDebugAvailable { get; set; }

    private readonly int _glVersion;
    private readonly bool _compatibilityProfile;


    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController(GameWindow window)
    {
        _window = window;
        window.Resize += args => WindowResized(args.Width, args.Height);
        window.TextInput += e => PressChar((char)e.Unicode);
        window.MouseWheel += e => MouseScroll(e.Offset);

        _windowWidth = window.ClientSize.X;
        _windowHeight = window.ClientSize.Y;

        int major = GL.GetInteger(GetPName.MajorVersion);
        int minor = GL.GetInteger(GetPName.MinorVersion);

        _glVersion = major * 100 + minor * 10;

        KhrDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

        _compatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.AddFontDefault();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        CreateDeviceResources();

        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();
        _frameBegun = true;
    }


    private void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }


    private void CreateDeviceResources()
    {
        _vertexBufferSize = 10000;
        _indexBufferSize = 2000;

        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);
        LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

        _vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        _indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        RecreateFontDeviceTexture();

        const string vertexSource = """
                                    #version 330 core

                                    uniform mat4 projection_matrix;

                                    layout(location = 0) in vec2 in_position;
                                    layout(location = 1) in vec2 in_texCoord;
                                    layout(location = 2) in vec4 in_color;

                                    out vec4 color;
                                    out vec2 texCoord;

                                    void main()
                                    {
                                        gl_Position = projection_matrix * vec4(in_position, 0, 1);
                                        color = in_color;
                                        texCoord = in_texCoord;
                                    }
                                    """;
        const string fragmentSource = """
                                      #version 330 core

                                      uniform sampler2D in_fontTexture;

                                      in vec4 color;
                                      in vec2 texCoord;

                                      out vec4 outputColor;

                                      void main()
                                      {
                                          outputColor = color * texture(in_fontTexture, texCoord);
                                      }
                                      """;

        _shader = CreateProgram("ImGui", vertexSource, fragmentSource);
        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(prevVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

        CheckGlError("End of ImGui setup");
    }


    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    private void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int _);

        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
        LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        // Restore state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);

        io.Fonts.SetTexID(_fontTexture);

        io.Fonts.ClearTexData();
    }


    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (!_frameBegun)
            return;
        
        _frameBegun = false;

        ImGui.Render();
        RenderImDrawData(ImGui.GetDrawData());
    }


    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update()
    {
        // Emulate the cursor being fixed to the center of the screen, as OpenTK doesn't fix the cursor position when it's grabbed.
        OpenTK.Mathematics.Vector2 mousePos = Input.Input.MouseState.Position;
        if (_window.CursorState == CursorState.Grabbed)
            mousePos = new OpenTK.Mathematics.Vector2(_window.ClientSize.X / 2f, _window.ClientSize.Y / 2f);

        float deltaSeconds = Time.DeltaTime;

        if (_frameBegun)
            ImGui.Render();

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(mousePos);

        _frameBegun = true;
        ImGui.NewFrame();
    }


    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// Update calls this(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }


    private readonly List<char> _pressedChars = new();

    private static readonly Keys[] AllKeys = Enum.GetValues(typeof(Keys))
        .Cast<Keys>()
        .Where(k => k != Keys.Unknown)
        .ToArray();


    // NOTE: Modified to take in an override mouse position
    private void UpdateImGuiInput(OpenTK.Mathematics.Vector2 mousePos)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        
        GUI.WantCaptureKeyboard = io.WantCaptureKeyboard;
        GUI.WantCaptureMouse = io.WantCaptureMouse;

        MouseState mouseState = Input.Input.MouseState;
        KeyboardState keyboardState = Input.Input.KeyboardState;

        io.MouseDown[0] = mouseState[MouseButton.Left];
        io.MouseDown[1] = mouseState[MouseButton.Right];
        io.MouseDown[2] = mouseState[MouseButton.Middle];
        io.MouseDown[3] = mouseState[MouseButton.Button4];
        io.MouseDown[4] = mouseState[MouseButton.Button5];

        io.MousePos = new System.Numerics.Vector2((int)mousePos.X, (int)mousePos.Y);

        // NOTE: Optimized to allocation free
        foreach (Keys key in AllKeys)
            if (TryMapKey(key, out ImGuiKey imKey))
                io.AddKeyEvent(imKey, keyboardState.IsKeyDown(key));

        foreach (char c in _pressedChars)
            io.AddInputCharacter(c);
        _pressedChars.Clear();

        io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
        io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
        io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
        io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
    }


    private bool TryMapKey(Keys key, out ImGuiKey result)
    {
        ImGuiKey KeyToImGuiKeyShortcut(Keys keyToConvert, Keys startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }

        result = key switch
        {
            >= Keys.F1 and <= Keys.F24 => KeyToImGuiKeyShortcut(key, Keys.F1, ImGuiKey.F1),
            >= Keys.KeyPad0 and <= Keys.KeyPad9 => KeyToImGuiKeyShortcut(key, Keys.KeyPad0, ImGuiKey.Keypad0),
            >= Keys.A and <= Keys.Z => KeyToImGuiKeyShortcut(key, Keys.A, ImGuiKey.A),
            >= Keys.D0 and <= Keys.D9 => KeyToImGuiKeyShortcut(key, Keys.D0, ImGuiKey._0),
            Keys.LeftShift or Keys.RightShift => ImGuiKey.ModShift,
            Keys.LeftControl or Keys.RightControl => ImGuiKey.ModCtrl,
            Keys.LeftAlt or Keys.RightAlt => ImGuiKey.ModAlt,
            Keys.LeftSuper or Keys.RightSuper => ImGuiKey.ModSuper,
            Keys.Menu => ImGuiKey.Menu,
            Keys.Up => ImGuiKey.UpArrow,
            Keys.Down => ImGuiKey.DownArrow,
            Keys.Left => ImGuiKey.LeftArrow,
            Keys.Right => ImGuiKey.RightArrow,
            Keys.Enter => ImGuiKey.Enter,
            Keys.Escape => ImGuiKey.Escape,
            Keys.Space => ImGuiKey.Space,
            Keys.Tab => ImGuiKey.Tab,
            Keys.Backspace => ImGuiKey.Backspace,
            Keys.Insert => ImGuiKey.Insert,
            Keys.Delete => ImGuiKey.Delete,
            Keys.PageUp => ImGuiKey.PageUp,
            Keys.PageDown => ImGuiKey.PageDown,
            Keys.Home => ImGuiKey.Home,
            Keys.End => ImGuiKey.End,
            Keys.CapsLock => ImGuiKey.CapsLock,
            Keys.ScrollLock => ImGuiKey.ScrollLock,
            Keys.PrintScreen => ImGuiKey.PrintScreen,
            Keys.Pause => ImGuiKey.Pause,
            Keys.NumLock => ImGuiKey.NumLock,
            Keys.KeyPadDivide => ImGuiKey.KeypadDivide,
            Keys.KeyPadMultiply => ImGuiKey.KeypadMultiply,
            Keys.KeyPadSubtract => ImGuiKey.KeypadSubtract,
            Keys.KeyPadAdd => ImGuiKey.KeypadAdd,
            Keys.KeyPadDecimal => ImGuiKey.KeypadDecimal,
            Keys.KeyPadEnter => ImGuiKey.KeypadEnter,
            Keys.GraveAccent => ImGuiKey.GraveAccent,
            Keys.Minus => ImGuiKey.Minus,
            Keys.Equal => ImGuiKey.Equal,
            Keys.LeftBracket => ImGuiKey.LeftBracket,
            Keys.RightBracket => ImGuiKey.RightBracket,
            Keys.Semicolon => ImGuiKey.Semicolon,
            Keys.Apostrophe => ImGuiKey.Apostrophe,
            Keys.Comma => ImGuiKey.Comma,
            Keys.Period => ImGuiKey.Period,
            Keys.Slash => ImGuiKey.Slash,
            Keys.Backslash => ImGuiKey.Backslash,
            _ => ImGuiKey.None
        };

        return result != ImGuiKey.None;
    }


    private void PressChar(char keyChar)
    {
        _pressedChars.Add(keyChar);
    }


    private static void MouseScroll(OpenTK.Mathematics.Vector2 offset)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }


    private void RenderImDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
            return;

        // Get initial state.
        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
        int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
        int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
        int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
        bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
        Span<int> prevScissorBox = stackalloc int[4];
        unsafe
        {
            fixed (int* iptr = &prevScissorBox[0])
            {
                GL.GetInteger(GetPName.ScissorBox, iptr);
            }
        }

        Span<int> prevPolygonMode = stackalloc int[2];
        unsafe
        {
            fixed (int* iptr = &prevPolygonMode[0])
            {
                GL.GetInteger(GetPName.PolygonMode, iptr);
            }
        }

        if (_glVersion <= 310 || _compatibilityProfile)
        {
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
        }
        else
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        // Bind the element buffer (through the VAO) so that we can resize it.
        GL.BindVertexArray(_vertexArray);

        // Bind the vertex buffer so that we can resize it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];

            int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _vertexBufferSize = newSize;
            }

            int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _indexBufferSize = newSize;
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        GL.UseProgram(_shader);
        GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
        GL.Uniform1(_shaderFontTextureLocation, 0);
        CheckGlError("Projection");

        GL.BindVertexArray(_vertexArray);
        CheckGlError("VAO");

        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Draw command lists
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data);
            CheckGlError($"Data Vert {i}");

            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data);
            CheckGlError($"Data Idx {i}");

            DrawCmdList(cmdList, io);
        }

        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);

        // Reset state
        ResetGLState(prevTexture2D, prevActiveTexture, prevProgram, prevVao, prevScissorBox, prevArrayBuffer, prevBlendEquationRgb, prevBlendEquationAlpha, prevBlendFuncSrcRgb, prevBlendFuncDstRgb, prevBlendFuncSrcAlpha, prevBlendFuncDstAlpha, prevBlendEnabled, prevDepthTestEnabled, prevCullFaceEnabled, prevScissorTestEnabled, prevPolygonMode);
    }


    private void DrawCmdList(ImDrawListPtr cmdList, ImGuiIOPtr io)
    {
        for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
        {
            ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdI];
            if (pcmd.UserCallback != IntPtr.Zero)
            {
                throw new NotImplementedException();
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
            CheckGlError("Texture");

            // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
            System.Numerics.Vector4 clip = pcmd.ClipRect;
            GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
            CheckGlError("Scissor");

            if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                GL.DrawElementsBaseVertex(
                    PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)),
                    unchecked((int)pcmd.VtxOffset));
            else
                GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
            CheckGlError("Draw");
        }
    }


    private void ResetGLState(int prevTexture2D, int prevActiveTexture, int prevProgram, int prevVao, Span<int> prevScissorBox, int prevArrayBuffer,
        int prevBlendEquationRgb, int prevBlendEquationAlpha, int prevBlendFuncSrcRgb, int prevBlendFuncDstRgb, int prevBlendFuncSrcAlpha,
        int prevBlendFuncDstAlpha, bool prevBlendEnabled, bool prevDepthTestEnabled, bool prevCullFaceEnabled, bool prevScissorTestEnabled, Span<int> prevPolygonMode)
    {
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);
        GL.UseProgram(prevProgram);
        GL.BindVertexArray(prevVao);
        GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
        GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
        GL.BlendFuncSeparate(
            (BlendingFactorSrc)prevBlendFuncSrcRgb,
            (BlendingFactorDest)prevBlendFuncDstRgb,
            (BlendingFactorSrc)prevBlendFuncSrcAlpha,
            (BlendingFactorDest)prevBlendFuncDstAlpha);
        if (prevBlendEnabled)
            GL.Enable(EnableCap.Blend);
        else
            GL.Disable(EnableCap.Blend);
        if (prevDepthTestEnabled)
            GL.Enable(EnableCap.DepthTest);
        else
            GL.Disable(EnableCap.DepthTest);
        if (prevCullFaceEnabled)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);
        if (prevScissorTestEnabled)
            GL.Enable(EnableCap.ScissorTest);
        else
            GL.Disable(EnableCap.ScissorTest);
        if (_glVersion <= 310 || _compatibilityProfile)
        {
            GL.PolygonMode(MaterialFace.Front, (PolygonMode)prevPolygonMode[0]);
            GL.PolygonMode(MaterialFace.Back, (PolygonMode)prevPolygonMode[1]);
        }
        else
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
        }
    }


    protected override void Dispose(bool manual)
    {
        if (IsDisposed)
            return;
        base.Dispose(manual);
        
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_indexBuffer);

        GL.DeleteTexture(_fontTexture);
        GL.DeleteProgram(_shader);
    }


    private static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
    {
        if (KhrDebugAvailable)
            GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
    }


    private static bool IsExtensionSupported(string name)
    {
        int n = GL.GetInteger(GetPName.NumExtensions);
        for (int i = 0; i < n; i++)
        {
            string extension = GL.GetString(StringNameIndexed.Extensions, i);
            if (extension == name)
                return true;
        }

        return false;
    }


    private static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
    {
        int program = GL.CreateProgram();
        LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

        int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
        int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetProgramInfoLog(program);
            System.Diagnostics.Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
        }

        GL.DetachShader(program, vertex);
        GL.DetachShader(program, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }


    private static int CompileShader(string name, ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            System.Diagnostics.Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
        }

        return shader;
    }


    public static void CheckGlError(string title)
    {
        ErrorCode error;
        int i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
            System.Diagnostics.Debug.Print($"{title} ({i++}): {error}");
    }
}