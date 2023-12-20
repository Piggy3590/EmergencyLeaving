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
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        public static int eventProbability;
        public static bool messageEnabled;

        [HarmonyPrefix]
        [HarmonyPatch("OnShipLandedMiscEvents")]
        private static void OnShipLandedMiscEvents_Patch()
        {
            if (UnityEngine.Random.Range(0, eventProbability) == 0)
            {
                EmergencyBool.canBeLeave = true;
                EmergencyTakeoffModBase.mls.LogInfo(string.Format("canbeleave"));
            }
            else
            {
                EmergencyBool.canBeLeave = false;
                EmergencyTakeoffModBase.mls.LogInfo(string.Format("cannotbeleave"));
            }

            if (EmergencyBool.canBeLeave && GameNetworkManager.Instance.isHostingGame && messageEnabled)
            {
                ShipEmergencyTextRPC();
            }
        }

        public static void ShipEmergencyText()
        {
            HUDManager.Instance.DisplayTip("???", "오늘은 무언가 다른 것 같습니다.", false, false, "LC_WhatIsThisThingy");
        }

        [ClientRpc]
        public static void ShipEmergencyTextRPC()
        {
            ShipEmergencyText();
        }

        [HarmonyPrefix]
        [HarmonyPatch("ShipHasLeft")]
        private static void ShipHasLeft_Patch()
        {
            HUDManagerPatch.isTimerActivated = false;
            EmergencyActivatedBool.leavingActivated = false;
            EmergencyBool.canBeLeave = false;
            PlayerControllerBPatch.timerFloat = 60;
            PlayerControllerBPatch.timerFloatPublic = 60;
            PlayerControllerBPatch.timerString = "";
            PlayerControllerBPatch.removeTextTimer = 0;
            PlayerControllerBPatch.removeTextCount = 0;

            TimeOfDayPatch.randomTriggerTimer = 0;
            TimeOfDayPatch.randomTriggerPercentage = 0;
            TimeOfDayPatch.randomTriggerCurrentPercentage = 100;
            TimeOfDayPatch.isMessaged = false;
            TimeOfDayPatch.isNotCalled = true;
            TimeOfDayPatch.normalizedTimeOfDayB = 0;
            TimeOfDayPatch.shipLeaveAutomaticallyTimeB = 0;
        }
    }
}