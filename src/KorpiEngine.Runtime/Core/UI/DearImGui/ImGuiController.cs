using System.Runtime.CompilerServices;
using ImGuiNET;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Windowing;
using OpenTK.Windowing.Common;

namespace KorpiEngine.Core.UI.DearImGui;

internal class ImGuiController : IDisposable
{
    private readonly KorpiWindow _window;
    private bool _frameBegun;
    private Material? _material;
    private Texture2D _fontTexture;
    private GraphicsBuffer? _vertexBuffer;
    private GraphicsBuffer? _indexBuffer;
    private GraphicsVertexArrayObject? _vao;

    /*private DeviceBuffer _vertexBuffer;
private DeviceBuffer _indexBuffer;
private DeviceBuffer _projMatrixBuffer;
private Texture _fontTexture;
private TextureView _fontTextureView;
private Shader _vertexShader;
private Shader _fragmentShader;
private ResourceLayout _layout;
private ResourceLayout _textureLayout;
private Pipeline _pipeline;
private ResourceSet _mainResourceSet;
private ResourceSet _fontTextureResourceSet;*/

    private IntPtr _fontAtlasID = 1;
    private bool _controlDown;
    private bool _shiftDown;
    private bool _altDown;
    private bool _winKeyDown;

    private int _windowWidth;
    private int _windowHeight;
    private Vector2 _scaleFactor = Vector2.One;

    // Image trackers
    private readonly Dictionary<IntPtr, Texture2D> _texturesById = new();
    private readonly Dictionary<Texture2D, IntPtr> _idsByTexture = new();
    private readonly List<char> _pressedChars = [];
    private int _lastAssignedID = 100;


    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController(KorpiWindow window)
    {
        _window = window;
        _windowWidth = window.FramebufferSize.X;
        _windowHeight = window.FramebufferSize.Y;
        
        _window.TextInput += WindowOnTextInput;
        _window.Resize += resizeEvent => WindowResized(resizeEvent.Width, resizeEvent.Height);

        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard |
                          ImGuiConfigFlags.DockingEnable;
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

        CreateDeviceResources();
        SetPerFrameImGuiData(1f / 60f);
        ImGui.NewFrame();
        _frameBegun = true;
    }


    private void WindowOnTextInput(TextInputEventArgs obj)
    {
        _pressedChars.Add((char)obj.Unicode);
    }


    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }


    public void DestroyDeviceObjects()
    {
        Dispose();
    }


    public void CreateDeviceResources()
    {
        RecreateFontDeviceTexture();
    }


    private IntPtr GetNextImGuiBindingID()
    {
        int newID = _lastAssignedID++;
        return newID;
    }


    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetImGuiTextureHandle(Texture2D texture)
    {
        if (_idsByTexture.TryGetValue(texture, out IntPtr id))
            return id;
        
        id = GetNextImGuiBindingID();
            
        _texturesById[id] = texture;
        _idsByTexture[texture] = id;

        return id;
    }


    private Texture2D GetImGuiTexture(IntPtr handle)
    {
        if (_texturesById.TryGetValue(handle, out Texture2D? texture))
            return texture;

        throw new InvalidOperationException("No texture found for the given handle.");
    }


    public void ClearCachedImageResources()
    {
        _texturesById.Clear();
        _idsByTexture.Clear();
        _lastAssignedID = 100;
    }


    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();

        // Build
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        // Store our identifier
        io.Fonts.SetTexID(_fontAtlasID);

        _fontTexture = new Texture2D(width, height, false, TextureImageFormat.RGBA_8_UF);   // Was PixelFormat.R8_G8_B8_A8_UNorm
        _fontTexture.Name = "ImGui.NET Font Texture";
        
        Graphics.Device.TexSubImage2D(_fontTexture.Handle, 0, 0, 0, width, height, pixels);

        io.Fonts.ClearTexData();
    }


    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        _material ??= new Material(Shader.Find("Defaults/ImGui.kshader"), "ImGui material");
        
        if (_frameBegun)
        {
            _frameBegun = false;

            ImGui.Begin("test");
            ImGui.Text("dddddsaaaaaaaaa");
            ImGui.End();
            
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }
    }


    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update()
    {
        if (_frameBegun)
            ImGui.Render();

        SetPerFrameImGuiData(Time.DeltaTime);
        UpdateImGuiInput();

        _frameBegun = true;
        ImGui.NewFrame();
    }


    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }


    /*private bool TryMapKey(Key key, out ImGuiKey result)
{
    ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
    {
        int changeFromStart1 = (int)keyToConvert - (int)startKey1;
        return startKey2 + changeFromStart1;
    }

    result = key switch
    {
        >= Key.F1 and <= Key.F24 => KeyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1),
        >= Key.Keypad0 and <= Key.Keypad9 => KeyToImGuiKeyShortcut(key, Key.Keypad0, ImGuiKey.Keypad0),
        >= Key.A and <= Key.Z => KeyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A),
        >= Key.Number0 and <= Key.Number9 => KeyToImGuiKeyShortcut(key, Key.Number0, ImGuiKey._0),
        Key.ShiftLeft or Key.ShiftRight => ImGuiKey.ModShift,
        Key.ControlLeft or Key.ControlRight => ImGuiKey.ModCtrl,
        Key.AltLeft or Key.AltRight => ImGuiKey.ModAlt,
        Key.WinLeft or Key.WinRight => ImGuiKey.ModSuper,
        Key.Menu => ImGuiKey.Menu,
        Key.Up => ImGuiKey.UpArrow,
        Key.Down => ImGuiKey.DownArrow,
        Key.Left => ImGuiKey.LeftArrow,
        Key.Right => ImGuiKey.RightArrow,
        Key.Enter => ImGuiKey.Enter,
        Key.Escape => ImGuiKey.Escape,
        Key.Space => ImGuiKey.Space,
        Key.Tab => ImGuiKey.Tab,
        Key.BackSpace => ImGuiKey.Backspace,
        Key.Insert => ImGuiKey.Insert,
        Key.Delete => ImGuiKey.Delete,
        Key.PageUp => ImGuiKey.PageUp,
        Key.PageDown => ImGuiKey.PageDown,
        Key.Home => ImGuiKey.Home,
        Key.End => ImGuiKey.End,
        Key.CapsLock => ImGuiKey.CapsLock,
        Key.ScrollLock => ImGuiKey.ScrollLock,
        Key.PrintScreen => ImGuiKey.PrintScreen,
        Key.Pause => ImGuiKey.Pause,
        Key.NumLock => ImGuiKey.NumLock,
        Key.KeypadDivide => ImGuiKey.KeypadDivide,
        Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
        Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
        Key.KeypadAdd => ImGuiKey.KeypadAdd,
        Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
        Key.KeypadEnter => ImGuiKey.KeypadEnter,
        Key.Tilde => ImGuiKey.GraveAccent,
        Key.Minus => ImGuiKey.Minus,
        Key.Plus => ImGuiKey.Equal,
        Key.BracketLeft => ImGuiKey.LeftBracket,
        Key.BracketRight => ImGuiKey.RightBracket,
        Key.Semicolon => ImGuiKey.Semicolon,
        Key.Quote => ImGuiKey.Apostrophe,
        Key.Comma => ImGuiKey.Comma,
        Key.Period => ImGuiKey.Period,
        Key.Slash => ImGuiKey.Slash,
        Key.BackSlash or Key.NonUSBackSlash => ImGuiKey.Backslash,
        _ => ImGuiKey.None
    };

    return result != ImGuiKey.None;
}*/


    private void UpdateImGuiInput()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        
        io.AddMousePosEvent((float)Input.MousePosition.X, (float)Input.MousePosition.Y);
        io.AddMouseButtonEvent(0, Input.GetMouseDown(MouseButton.Left));
        io.AddMouseButtonEvent(1, Input.GetMouseDown(MouseButton.Right));
        io.AddMouseButtonEvent(2, Input.GetMouseDown(MouseButton.Middle));
        io.AddMouseButtonEvent(3, Input.GetMouseDown(MouseButton.Button1));
        io.AddMouseButtonEvent(4, Input.GetMouseDown(MouseButton.Button2));
        io.AddMouseWheelEvent(0f, (float)Input.ScrollDelta.Y);
        
        foreach (char c in _pressedChars)
            io.AddInputCharacter(c);

        //BUG: FIX THIS                                                       for (int i = 0; i < Input.KeyEvents.Count; i++)
        //BUG: FIX THIS                                                       {
        //BUG: FIX THIS                                                           KeyEvent keyEvent = Input.KeyEvents[i];
        //BUG: FIX THIS                                                           if (TryMapKey(keyEvent.Key, out ImGuiKey imguikey))
        //BUG: FIX THIS                                                               io.AddKeyEvent(imguikey, keyEvent.Down);
        //BUG: FIX THIS                                                       }
        _pressedChars.Clear();
    }


    private void RenderImDrawData(ImDrawDataPtr drawData)
    {
        // Below variables allow us to store all data in a single mega-buffer if desired.
        const int vertexOffsetInVertices = 0;
        const int indexOffsetInElements = 0;

        if (drawData.CmdListsCount == 0)
            return;

        int vertexSize = Unsafe.SizeOf<ImDrawVert>();

        CheckBuffers(drawData, vertexSize, vertexOffsetInVertices, indexOffsetInElements);

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        Matrix4x4 projMat = Matrix4x4.CreateOrthographicOffCenter(
            0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        _material.SetMatrix("_MatProjection", projMat);

        drawData.ScaleClipRects(io.DisplayFramebufferScale);
        
        // Render command lists
        int vtxOffset = 0;
        int idxOffset = 0;
        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[n];
            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                ImDrawCmdPtr cmdPtr = cmdList.CmdBuffer[cmdIndex];
                if (cmdPtr.UserCallback != IntPtr.Zero)
                    throw new NotImplementedException();

                if (cmdPtr.TextureId != IntPtr.Zero)
                {
                    if (cmdPtr.TextureId == _fontAtlasID)
                        _material.SetTexture("_MainTexture", _fontTexture);
                    else
                        _material.SetTexture("_MainTexture", GetImGuiTexture(cmdPtr.TextureId));
                }

                Graphics.Device.SetScissorRect(
                    0,
                    (int)cmdPtr.ClipRect.X,
                    (int)cmdPtr.ClipRect.Y,
                    (int)(cmdPtr.ClipRect.Z - cmdPtr.ClipRect.X),
                    (int)(cmdPtr.ClipRect.W - cmdPtr.ClipRect.Y));

                _material.SetPass(0, true);
                
                int elementCount = (int)cmdPtr.ElemCount;
                int indexOffset = (int)cmdPtr.IdxOffset + idxOffset;
                int vertexOffset = (int)cmdPtr.VtxOffset + vtxOffset;
                
                Graphics.Device.BindVertexArray(_vao);
                Graphics.Device.DrawElements(Topology.Triangles, elementCount, false, indexOffset, vertexOffset);
                Graphics.Device.BindVertexArray(null);
            }

            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }
    }


    private void CheckBuffers(ImDrawDataPtr drawData, int vertexSize, int vertexOffsetInVertices, int indexOffsetInElements)
    {
        bool changed = false;
        uint totalVbSize = (uint)(drawData.TotalVtxCount * vertexSize);
        if (_vertexBuffer == null || totalVbSize > _vertexBuffer.SizeInBytes)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = Graphics.Device.CreateBuffer(BufferType.VertexBuffer, new nint[(uint)(totalVbSize * 1.5f)], true);
            changed = true;
        }

        uint totalIbSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
        if (_indexBuffer == null || totalIbSize > _indexBuffer.SizeInBytes)
        {
            _indexBuffer?.Dispose();
            _indexBuffer = Graphics.Device.CreateBuffer(BufferType.ElementsBuffer, new nint[(uint)(totalVbSize * 1.5f)], true);
            changed = true;
        }

        if (changed)
        {
            _vao?.Dispose();
            _vao = Graphics.Device.CreateVertexArray(new MeshVertexLayout(
            [
                new MeshVertexLayout.VertexAttributeDescriptor(0, VertexAttributeType.Float, 2),
                new MeshVertexLayout.VertexAttributeDescriptor(1, VertexAttributeType.Float, 2),
                new MeshVertexLayout.VertexAttributeDescriptor(2, VertexAttributeType.Byte, 4, true)    //BUG: Normalization does not go through if not float?
            ]), _vertexBuffer, _indexBuffer);
        }

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];
            
            Graphics.Device.UpdateBuffer(_vertexBuffer,
                vertexOffsetInVertices * vertexSize,
                cmdList.VtxBuffer.Size * vertexSize,
                cmdList.VtxBuffer.Data);

            Graphics.Device.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                cmdList.IdxBuffer.Size * sizeof(ushort),
                cmdList.IdxBuffer.Data);

            vertexOffsetInVertices += cmdList.VtxBuffer.Size;
            indexOffsetInElements += cmdList.IdxBuffer.Size;
        }
    }


    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _material.Destroy();
        _fontTexture.Destroy();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vao?.Dispose();
    }
}