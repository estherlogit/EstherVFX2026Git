using System;
using System.Collections;
using System.Threading.Tasks;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Networking;
using Facebook.Workrooms.Services;
using Facebook.Xplat.Events;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.Settings;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Tests {
  [OnCall(OnCallName.workrooms_core)]
  public class PinCodeServiceTest {
    private IServiceContainer container_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      container_.Bind<IWorkroomsConfigurationOptions>().To(Substitute.For<IWorkroomsConfigurationOptions>());
      container_.Bind<ILogService>().To(new UnityLogService());
      container_.Bind<IDispatcher>().To(Substitute.For<IDispatcher>());
      container_.Bind<OCGatekeeperClient>().To(Substitute.For<OCGatekeeperClient>());

      var settingsFactory = new XmlSettingsFactory(PathUtils.GetPersistentDataPath());
      XmlSettingsFactory.machineId = SystemInfo.deviceUniqueIdentifier;

      var encryptedSettingsTask = Task.Run(() => settingsFactory.Get<WorkroomsEncryptedSettings>("test"));
      container_.Bind<WorkroomsEncryptedSettings>().To(encryptedSettingsTask.Result);

      container_.Bind<IWorkroomsPinCodeService>()
        .To(
          new WorkroomsPinCodeService(
            container_.Get<ILogService>(),
            container_.Get<IDispatcher>(),
            container_.Get<WorkroomsEncryptedSettings>(),
            container_.Get<IWorkroomsConfigurationOptions>(),
            container_.Get<OCGatekeeperClient>()
          )
        );
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [UnityTest]
    public IEnumerator PinCodeServiceStoresAndExpiresPinCorrectly() {
      var pinCodeService = container_.Get<IWorkroomsPinCodeService>();

      pinCodeService.SetNewPinCode("1111");

      Assert.IsTrue(pinCodeService.DoesPinCodeMatch("1111"));
      Assert.IsFalse(pinCodeService.DoesPinCodeMatch("1112"));
      Assert.IsFalse(pinCodeService.DoesPinCodeMatch("11122"));
      Assert.IsFalse(pinCodeService.DoesPinCodeMatch(""));

      Assert.IsFalse(pinCodeService.HasLastPinCheckExpired());
      Assert.IsFalse(
        pinCodeService.HasLastPinCheckExpired(
          DateTime.UtcNow.AddSeconds(WorkroomsPinCodeService.PIN_CODE_CHECK_EXPIRATION_PERIOD_SECONDS - 5)
        )
      );
      Assert.IsTrue(
        pinCodeService.HasLastPinCheckExpired(
          DateTime.UtcNow.AddSeconds(WorkroomsPinCodeService.PIN_CODE_CHECK_EXPIRATION_PERIOD_SECONDS + 5)
        )
      );

      // Wait for 10 seconds and confirm that PIN check correctly reflects the time passing.
      yield return new WaitForSeconds(10);
      Assert.IsFalse(pinCodeService.HasLastPinCheckExpired());
      Assert.IsTrue(
        pinCodeService.HasLastPinCheckExpired(
          DateTime.UtcNow.AddSeconds(WorkroomsPinCodeService.PIN_CODE_CHECK_EXPIRATION_PERIOD_SECONDS - 5)
        )
      );

      // Check the correct PIN again and confirm that expiration period got updated.
      Assert.IsTrue(pinCodeService.DoesPinCodeMatch("1111"));
      Assert.IsFalse(
        pinCodeService.HasLastPinCheckExpired(
          DateTime.UtcNow.AddSeconds(WorkroomsPinCodeService.PIN_CODE_CHECK_EXPIRATION_PERIOD_SECONDS - 5)
        )
      );

      // Reset the PIN and confirm the previously correct one does not match anymore.
      pinCodeService.ResetPinCode();
      Assert.IsFalse(pinCodeService.DoesPinCodeMatch("1111"));
      Assert.IsTrue(pinCodeService.DoesPinCodeMatch(""));

      yield return null;
    }
  }
}
