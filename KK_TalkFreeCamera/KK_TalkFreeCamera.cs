using System.Reflection;

using BepInEx;
using HarmonyLib;

using KKAPI.MainGame;
using KKAPI.Utilities;

using UnityEngine;

namespace KK_TalkFreeCamera
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInPlugin(nameof(KK_TalkFreeCamera), nameof(KK_TalkFreeCamera), VERSION)]
    public class KK_TalkFreeCamera : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";

        private static TalkScene talkScene;
        private static SaveData.Heroine heroine;
        private static CameraControl_Ver2 ccv2;

        private void Awake()
        {
            var icon = GetIcon();
            if (icon == null)
                return;
            
            GameAPI.AddTouchIcon(icon, button => button.onClick.AddListener(ToggleCameraMode));
            Harmony.CreateAndPatchAll(typeof(KK_TalkFreeCamera));
        }
        
        private static void ToggleCameraMode()
        {
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

        private static Sprite GetIcon()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(KK_TalkFreeCamera)}.Resources.cam.png"))
            {
                if (stream == null) 
                    return null;
                
                var bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, bytesInStream.Length);

                if (bytesInStream.Length == 0)
                    return null;
                
                var texture = new Texture2D(64, 64);
                texture.LoadImage(bytesInStream);

                return texture == null ? null : texture.ToSprite();
            }
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