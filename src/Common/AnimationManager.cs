using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Yeet.Common {
  public class AnimationManager {
    protected YeetSystem System { get; }
    protected float ScreenShakeIntensity { get; set; }

    public AnimationManager(YeetSystem system) {
      System = system;

      System.Event.OnAfterInput += OnAfterInput;
      System.Event.OnServerReceivedYeetEvent += OnServerReceivedYeetEvent;
      System.Event.OnAfterServerHandledEvent += OnAfterServerHandledEvent;
      System.Event.OnClientReceivedYeetEvent += OnClientReceivedYeetEvent;

      if (system.Api is ICoreClientAPI capi) {
        LoadClientSettings(capi);
        ScreenShakeIntensity = capi.ModLoader.GetModSystem<YeetConfigurationSystem>()?.ClientSettings?.ScreenShakeIntensity.Value ?? new ClientConfig().ScreenShakeIntensity.Value;
      }
    }

    protected virtual void LoadClientSettings(ICoreClientAPI capi) {
      var clientSettings = capi.ModLoader.GetModSystem<YeetConfigurationSystem>()?.ClientSettings ?? new ClientConfig();
      ScreenShakeIntensity = clientSettings.ScreenShakeIntensity.Value;
    }

    protected virtual void OnAfterInput(YeetEventArgs eventArgs) {
      if (!eventArgs.Successful) {
        return;
      }

      StartWindup(System.ClientAPI.World.Player.Entity);
    }

    protected virtual void OnServerReceivedYeetEvent(YeetEventArgs eventArgs) {
      StartWindup(eventArgs.ForPlayer.Entity);
    }

    protected virtual void OnAfterServerHandledEvent(YeetEventArgs eventArgs) {
      if (!eventArgs.Successful || eventArgs.YeetedEntityItem == null) {
        return;
      }

      eventArgs.YeetedEntityItem.AddBehavior(SpawnShockwavesEntityBehavior.Create(eventArgs.YeetedEntityItem));
    }

    protected virtual void OnClientReceivedYeetEvent(YeetEventArgs eventArgs) {
      if (!eventArgs.Successful) {
        OnFailedYeet(System.ClientAPI?.World.Player);
        return;
      }

      OnSuccessfulYeet(eventArgs);
    }

    protected virtual void OnFailedYeet(IPlayer yeeter) {
      StopWindup(yeeter.Entity);
    }

    protected virtual void OnSuccessfulYeet(YeetEventArgs eventArgs) {
      ShakeScreen();
      var playerEntity = System.ClientAPI.World.Player.Entity;
      StopWindup(playerEntity);
      StrongYeet(playerEntity);
    }

    protected virtual void ShakeScreen() {
      if (ShouldShakeScreen()) {
        System.ClientAPI.World.SetCameraShake(ScreenShakeIntensity);
      }
    }

    protected virtual bool ShouldShakeScreen() {
      return System.Side == EnumAppSide.Client
             && ScreenShakeIntensity > 0f
             && System.ClientAPI.World.Player.CameraMode == EnumCameraMode.FirstPerson;
    }

    protected virtual void StartWindup(EntityPlayer yeeter) {
      yeeter.StartAnimation("aim");
    }

    protected virtual void StopWindup(EntityPlayer yeeter) {
      yeeter.StopAnimation("aim");
    }

    protected virtual void StrongYeet(EntityPlayer yeeter) {
      yeeter.StartAnimation("throw"); // resets automatically
      yeeter.StartAnimation("sneakidle");
      System.Api.World.RegisterCallback((float dt) => { yeeter.StopAnimation("sneakidle"); }, 600);
    }
  }
}
