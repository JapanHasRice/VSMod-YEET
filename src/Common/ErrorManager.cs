using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Yeet.Common {
  public class ErrorManager {
    private YeetSystem System { get; }
    private readonly string langPrefix = $"{Constants.MOD_ID}:";

    public ErrorManager(YeetSystem system)
      => System = system;

    public void TriggerFromClient(string errorCode, params object[] args) {
      System.ClientAPI?.TriggerIngameError(System, errorCode, GetErrorText(errorCode, args));
    }

    public void TriggerFromServer(string errorCode, IServerPlayer forPlayer, params object[] args) {
      forPlayer?.SendIngameError(errorCode, GetErrorText(errorCode, args));
    }

    public string GetErrorText(string errorCode, params object[] args) {
      string prefixedCode = errorCode.StartsWith(langPrefix) ? errorCode : $"{langPrefix}{errorCode}";
      string displayMessage = Lang.GetMatching(prefixedCode, args).Replace(langPrefix, "");
      return displayMessage;
    }
  }
}
