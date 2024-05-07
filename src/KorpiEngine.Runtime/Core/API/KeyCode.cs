namespace KorpiEngine.Core.API;

public enum KeyCode
{
    /// <summary>An unknown key.</summary>
    Unknown = -1, // 0xFFFFFFFF
    /// <summary>The spacebar key.</summary>
    Space = 32, // 0x00000020
    /// <summary>The apostrophe key.</summary>
    Apostrophe = 39, // 0x00000027
    /// <summary>The comma key.</summary>
    Comma = 44, // 0x0000002C
    /// <summary>The minus key.</summary>
    Minus = 45, // 0x0000002D
    /// <summary>The period key.</summary>
    Period = 46, // 0x0000002E
    /// <summary>The slash key.</summary>
    Slash = 47, // 0x0000002F
    /// <summary>The 0 key.</summary>
    D0 = 48, // 0x00000030
    /// <summary>The 1 key.</summary>
    D1 = 49, // 0x00000031
    /// <summary>The 2 key.</summary>
    D2 = 50, // 0x00000032
    /// <summary>The 3 key.</summary>
    D3 = 51, // 0x00000033
    /// <summary>The 4 key.</summary>
    D4 = 52, // 0x00000034
    /// <summary>The 5 key.</summary>
    D5 = 53, // 0x00000035
    /// <summary>The 6 key.</summary>
    D6 = 54, // 0x00000036
    /// <summary>The 7 key.</summary>
    D7 = 55, // 0x00000037
    /// <summary>The 8 key.</summary>
    D8 = 56, // 0x00000038
    /// <summary>The 9 key.</summary>
    D9 = 57, // 0x00000039
    /// <summary>The semicolon key.</summary>
    Semicolon = 59, // 0x0000003B
    /// <summary>The equal key.</summary>
    Equal = 61, // 0x0000003D
    /// <summary>The A key.</summary>
    A = 65, // 0x00000041
    /// <summary>The B key.</summary>
    B = 66, // 0x00000042
    /// <summary>The C key.</summary>
    C = 67, // 0x00000043
    /// <summary>The D key.</summary>
    D = 68, // 0x00000044
    /// <summary>The E key.</summary>
    E = 69, // 0x00000045
    /// <summary>The F key.</summary>
    F = 70, // 0x00000046
    /// <summary>The G key.</summary>
    G = 71, // 0x00000047
    /// <summary>The H key.</summary>
    H = 72, // 0x00000048
    /// <summary>The I key.</summary>
    I = 73, // 0x00000049
    /// <summary>The J key.</summary>
    J = 74, // 0x0000004A
    /// <summary>The K key.</summary>
    K = 75, // 0x0000004B
    /// <summary>The L key.</summary>
    L = 76, // 0x0000004C
    /// <summary>The M key.</summary>
    M = 77, // 0x0000004D
    /// <summary>The N key.</summary>
    N = 78, // 0x0000004E
    /// <summary>The O key.</summary>
    O = 79, // 0x0000004F
    /// <summary>The P key.</summary>
    P = 80, // 0x00000050
    /// <summary>The Q key.</summary>
    Q = 81, // 0x00000051
    /// <summary>The R key.</summary>
    R = 82, // 0x00000052
    /// <summary>The S key.</summary>
    S = 83, // 0x00000053
    /// <summary>The T key.</summary>
    T = 84, // 0x00000054
    /// <summary>The U key.</summary>
    U = 85, // 0x00000055
    /// <summary>The V key.</summary>
    V = 86, // 0x00000056
    /// <summary>The W key.</summary>
    W = 87, // 0x00000057
    /// <summary>The X key.</summary>
    X = 88, // 0x00000058
    /// <summary>The Y key.</summary>
    Y = 89, // 0x00000059
    /// <summary>The Z key.</summary>
    Z = 90, // 0x0000005A
    /// <summary>The left bracket(opening bracket) key.</summary>
    LeftBracket = 91, // 0x0000005B
    /// <summary>The backslash.</summary>
    Backslash = 92, // 0x0000005C
    /// <summary>The right bracket(closing bracket) key.</summary>
    RightBracket = 93, // 0x0000005D
    /// <summary>The grave accent key.</summary>
    GraveAccent = 96, // 0x00000060
    /// <summary>The escape key.</summary>
    Escape = 256, // 0x00000100
    /// <summary>The enter key.</summary>
    Enter = 257, // 0x00000101
    /// <summary>The tab key.</summary>
    Tab = 258, // 0x00000102
    /// <summary>The backspace key.</summary>
    Backspace = 259, // 0x00000103
    /// <summary>The insert key.</summary>
    Insert = 260, // 0x00000104
    /// <summary>The delete key.</summary>
    Delete = 261, // 0x00000105
    /// <summary>The right arrow key.</summary>
    Right = 262, // 0x00000106
    /// <summary>The left arrow key.</summary>
    Left = 263, // 0x00000107
    /// <summary>The down arrow key.</summary>
    Down = 264, // 0x00000108
    /// <summary>The up arrow key.</summary>
    Up = 265, // 0x00000109
    /// <summary>The page up key.</summary>
    PageUp = 266, // 0x0000010A
    /// <summary>The page down key.</summary>
    PageDown = 267, // 0x0000010B
    /// <summary>The home key.</summary>
    Home = 268, // 0x0000010C
    /// <summary>The end key.</summary>
    End = 269, // 0x0000010D
    /// <summary>The caps lock key.</summary>
    CapsLock = 280, // 0x00000118
    /// <summary>The scroll lock key.</summary>
    ScrollLock = 281, // 0x00000119
    /// <summary>The num lock key.</summary>
    NumLock = 282, // 0x0000011A
    /// <summary>The print screen key.</summary>
    PrintScreen = 283, // 0x0000011B
    /// <summary>The pause key.</summary>
    Pause = 284, // 0x0000011C
    /// <summary>The F1 key.</summary>
    F1 = 290, // 0x00000122
    /// <summary>The F2 key.</summary>
    F2 = 291, // 0x00000123
    /// <summary>The F3 key.</summary>
    F3 = 292, // 0x00000124
    /// <summary>The F4 key.</summary>
    F4 = 293, // 0x00000125
    /// <summary>The F5 key.</summary>
    F5 = 294, // 0x00000126
    /// <summary>The F6 key.</summary>
    F6 = 295, // 0x00000127
    /// <summary>The F7 key.</summary>
    F7 = 296, // 0x00000128
    /// <summary>The F8 key.</summary>
    F8 = 297, // 0x00000129
    /// <summary>The F9 key.</summary>
    F9 = 298, // 0x0000012A
    /// <summary>The F10 key.</summary>
    F10 = 299, // 0x0000012B
    /// <summary>The F11 key.</summary>
    F11 = 300, // 0x0000012C
    /// <summary>The F12 key.</summary>
    F12 = 301, // 0x0000012D
    /// <summary>The F13 key.</summary>
    F13 = 302, // 0x0000012E
    /// <summary>The F14 key.</summary>
    F14 = 303, // 0x0000012F
    /// <summary>The F15 key.</summary>
    F15 = 304, // 0x00000130
    /// <summary>The F16 key.</summary>
    F16 = 305, // 0x00000131
    /// <summary>The F17 key.</summary>
    F17 = 306, // 0x00000132
    /// <summary>The F18 key.</summary>
    F18 = 307, // 0x00000133
    /// <summary>The F19 key.</summary>
    F19 = 308, // 0x00000134
    /// <summary>The F20 key.</summary>
    F20 = 309, // 0x00000135
    /// <summary>The F21 key.</summary>
    F21 = 310, // 0x00000136
    /// <summary>The F22 key.</summary>
    F22 = 311, // 0x00000137
    /// <summary>The F23 key.</summary>
    F23 = 312, // 0x00000138
    /// <summary>The F24 key.</summary>
    F24 = 313, // 0x00000139
    /// <summary>The F25 key.</summary>
    F25 = 314, // 0x0000013A
    /// <summary>The 0 key on the key pad.</summary>
    KeyPad0 = 320, // 0x00000140
    /// <summary>The 1 key on the key pad.</summary>
    KeyPad1 = 321, // 0x00000141
    /// <summary>The 2 key on the key pad.</summary>
    KeyPad2 = 322, // 0x00000142
    /// <summary>The 3 key on the key pad.</summary>
    KeyPad3 = 323, // 0x00000143
    /// <summary>The 4 key on the key pad.</summary>
    KeyPad4 = 324, // 0x00000144
    /// <summary>The 5 key on the key pad.</summary>
    KeyPad5 = 325, // 0x00000145
    /// <summary>The 6 key on the key pad.</summary>
    KeyPad6 = 326, // 0x00000146
    /// <summary>The 7 key on the key pad.</summary>
    KeyPad7 = 327, // 0x00000147
    /// <summary>The 8 key on the key pad.</summary>
    KeyPad8 = 328, // 0x00000148
    /// <summary>The 9 key on the key pad.</summary>
    KeyPad9 = 329, // 0x00000149
    /// <summary>The decimal key on the key pad.</summary>
    KeyPadDecimal = 330, // 0x0000014A
    /// <summary>The divide key on the key pad.</summary>
    KeyPadDivide = 331, // 0x0000014B
    /// <summary>The multiply key on the key pad.</summary>
    KeyPadMultiply = 332, // 0x0000014C
    /// <summary>The subtract key on the key pad.</summary>
    KeyPadSubtract = 333, // 0x0000014D
    /// <summary>The add key on the key pad.</summary>
    KeyPadAdd = 334, // 0x0000014E
    /// <summary>The enter key on the key pad.</summary>
    KeyPadEnter = 335, // 0x0000014F
    /// <summary>The equal key on the key pad.</summary>
    KeyPadEqual = 336, // 0x00000150
    /// <summary>The left shift key.</summary>
    LeftShift = 340, // 0x00000154
    /// <summary>The left control key.</summary>
    LeftControl = 341, // 0x00000155
    /// <summary>The left alt key.</summary>
    LeftAlt = 342, // 0x00000156
    /// <summary>The left super key.</summary>
    LeftSuper = 343, // 0x00000157
    /// <summary>The right shift key.</summary>
    RightShift = 344, // 0x00000158
    /// <summary>The right control key.</summary>
    RightControl = 345, // 0x00000159
    /// <summary>The right alt key.</summary>
    RightAlt = 346, // 0x0000015A
    /// <summary>The right super key.</summary>
    RightSuper = 347, // 0x0000015B
    /// <summary>The last valid key in this enum.</summary>
    LastKey = 348, // 0x0000015C
    /// <summary>The menu key.</summary>
    Menu = 348, // 0x0000015C
}