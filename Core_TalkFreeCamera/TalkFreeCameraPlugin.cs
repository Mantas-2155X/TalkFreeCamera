using System.Reflection;

using BepInEx;
using HarmonyLib;
using KKAPI;
using KKAPI.MainGame;
using KKAPI.Utilities;

using UnityEngine;

namespace KK_TalkFreeCamera
{
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginName, Version)]
    public class TalkFreeCameraPlugin : BaseUnityPlugin
    {
        public const string GUID = "KK_TalkFreeCamera";
        public const string PluginName = "TalkFreeCamera";
        public const string Version = "1.0.0";

        private static TalkScene talkScene;
        private static SaveData.Heroine heroine;
        private static CameraControl_Ver2 ccv2;

        private void Awake()
        {
            var icon = ResourceUtils.GetEmbeddedResource("cam.png", typeof(TalkFreeCameraPlugin).Assembly).LoadTexture().ToSprite();
            GameAPI.AddTouchIcon(icon, button => button.onClick.AddListener(ToggleCameraMode));

            Harmony.CreateAndPatchAll(typeof(TalkFreeCameraPlugin), GUID);
        }

        private static void ToggleCameraMode()
        {
            var talkScene = GameAPI.GetTalkScene();
            if (ccv2 == null || talkScene == null)
                return;

            heroine = talkScene.targetHeroine;
            if (heroine == null)
                return;

            ccv2.enabled = !ccv2.enabled;

            if (ccv2.enabled)
                ccv2.TargetSet(heroine.chaCtrl.objHead.transform, true);
            else
                ccv2.Reset(1);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(TalkScene), "Awake")]
        private static void TalkScene_Awake_Postfix(TalkScene __instance)
        {
            talkScene = __instance;

            if (Camera.main == null)
                return;

            ccv2 = Camera.main.gameObject.AddComponent<CameraControl_Ver2>();
            if (ccv2 == null)
                return;

            ccv2.enabled = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TalkScene), "OnDestroy")]
        private static void TalkScene_OnDestroy_Prefix()
        {
            if (ccv2 == null)
                return;

            Destroy(ccv2);

            heroine = null;
            ccv2 = null;
        }
    }
}