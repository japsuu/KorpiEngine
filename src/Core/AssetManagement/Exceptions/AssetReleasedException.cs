using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

internal class AssetReleasedException(string message) : KorpiException(message);