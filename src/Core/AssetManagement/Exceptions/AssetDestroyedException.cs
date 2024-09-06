using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

internal class AssetDestroyedException(string message) : KorpiException(message);