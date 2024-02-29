using System;
using BepInEx;
using KKAPI;
using KKAPI.MainGame;
using KKAPI.Utilities;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace KK_TalkFreeCamera
{
    [BepInProcess(KoikatuAPI.GameProcessName)]
#if KK
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
#endif
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginName, Version)]
    public class TalkFreeCameraPlugin : BaseUnityPlugin
    {
        public const string GUID = "KK_TalkFreeCamera";
        public const string PluginName = "TalkFreeCamera";
        public const string Version = "1.0.1";

        private IDisposable cleanup;
#if DEBUG // Hot reload
        private void OnDestroy() => cleanup.Dispose();
#endif

        private void Awake()
        {
            var icon = ResourceUtils.GetEmbeddedResource("cam.png", typeof(TalkFreeCameraPlugin).Assembly).LoadTexture().ToSprite();
            cleanup = GameAPI.AddTouchIcon(icon, button => button.onClick.AddListener(ToggleCameraMode), 1, -420);
        }

        private void ToggleCameraMode()
        {
            var talkScene = GameAPI.GetTalkScene();
            if (talkScene == null)
                return;
#if KKS
            if (!TalkScene.isPaly)
                return;
#endif
            var heroine = talkScene.targetHeroine;
            if (heroine == null)
                return;

            var maincamGo = Camera.main.gameObject;
            var ccv2 = maincamGo.GetComponent<CameraControl_Ver2>();
            if (ccv2 == null)
            {
                Logger.LogInfo("Adding CameraControl_Ver2 to: " + maincamGo.GetFullPath());
                ccv2 = maincamGo.AddComponent<CameraControl_Ver2>();
                ccv2.enabled = false;
                
                // Clean up after talk scene ends
                void Cleanup()
                {
                    Logger.LogInfo("Removing added CameraControl_Ver2");
                    Destroy(ccv2);
                }
#if KK          // In KK it's destroyed
                talkScene.OnDestroyAsObservable().Subscribe(_ => Cleanup());
#else           // In KKS it's only disabled and always kept loaded
                talkScene.cancellation.Token.Register(Cleanup);
#endif
            }

            ccv2.enabled = !ccv2.enabled;

            if (ccv2.enabled)
                ccv2.TargetSet(heroine.chaCtrl.objHead.transform, true);
            else
                ccv2.Reset(1);
        }
    }
}