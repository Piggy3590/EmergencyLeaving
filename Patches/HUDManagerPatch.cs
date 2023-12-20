using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using EmergencyTakeoff.Patches;
using Unity.Netcode;
using System.Runtime.Remoting.Messaging;
using EmergencyTakeoff;

namespace EmergencyTakeoff.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        public static TextMeshProUGUI TimerTextGUI;
        public static bool isTimerActivated;
        public static HUDManager hudManager;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void Awake_Postfix(ref HUDManager __instance)
        {
            hudManager = __instance;
            if (GameNetworkManager.Instance.isHostingGame)
            {
                SetupHUDTimerRPC();
            }
        }

        [ClientRpc]
        public static void SetupHUDTimerRPC()
        {
            GameObject TimerTextGUIObject = new("TimerText");
            TimerTextGUIObject.AddComponent<RectTransform>();
            TimerTextGUI = TimerTextGUIObject.AddComponent<TextMeshProUGUI>();

            RectTransform rectTransform = TimerTextGUI.rectTransform;
            rectTransform.SetParent(GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD").transform, false);
            rectTransform.sizeDelta = new Vector2(600, 200);
            rectTransform.anchoredPosition = new Vector2(-60, -200);
            rectTransform.rotation = Quaternion.Euler(19.4f, -11.7f, 0);

            TimerTextGUI.alignment = TextAlignmentOptions.Left;
            TimerTextGUI.font = hudManager.controlTipLines[0].font;
            TimerTextGUI.fontSize = 30f;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix()
        {
            if (!isTimerActivated)
            {
                TimerTextGUI.text = "";
            }
        }
    }
}