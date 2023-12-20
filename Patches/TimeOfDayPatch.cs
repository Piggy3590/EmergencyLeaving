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
using EmergencyTakeoff;

namespace EmergencyTakeoff.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        public static bool isWeathed;
        public static float randomTriggerTimer;
        public static float randomTriggerPercentage;
        public static float randomTriggerCurrentPercentage = 100;
        public static bool isMessaged;
        public static bool isNotCalled;

        public static float normalizedTimeOfDayB;
        public static float shipLeaveAutomaticallyTimeB;
        public static DialogueSegment[] shipLeavingEarlyDialogueB;

        public static float minimumIncrease;
        public static float maximumIncrease;

        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        private static void Awake_Patch()
        {
            randomTriggerPercentage = 0;
            randomTriggerCurrentPercentage = 100;
            isNotCalled = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Patch(ref DialogueSegment[] ___shipLeavingEarlyDialogue)
        {
            ___shipLeavingEarlyDialogue[0].speakerText = "PILOT COMPUTER";
            ___shipLeavingEarlyDialogue[0].bodyText = "WARNING!!! A major disaster is expected in the area, and the ship will leave automatically in a moment.\nThe suit has activated emergency mode. Escape immediately.";
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_PostFix(ref float ___normalizedTimeOfDay, ref float ___shipLeaveAutomaticallyTime, ref DialogueSegment[] ___shipLeavingEarlyDialogue)
        {
            if (___normalizedTimeOfDay >= 0.3355214f && !isMessaged && EmergencyBool.canBeLeave)
            {
                randomTriggerTimer += Time.deltaTime;
            }
            if (randomTriggerTimer >= 5 && GameNetworkManager.Instance.isHostingGame)
            {
                randomTriggerPercentage += UnityEngine.Random.Range(minimumIncrease, maximumIncrease);
                randomTriggerCurrentPercentage = UnityEngine.Random.Range(2f, 100f);
                EmergencyTakeoffModBase.mls.LogInfo(string.Format("new emergency percentage: " + randomTriggerPercentage));
                EmergencyTakeoffModBase.mls.LogInfo(string.Format("emergency percentage now: " + randomTriggerCurrentPercentage));
                if (randomTriggerCurrentPercentage >= randomTriggerPercentage && !isMessaged)
                {
                    EmergencyTakeoffModBase.mls.LogInfo(string.Format("emergency leave failed"));
                }
                randomTriggerTimer = 0f;

                if (randomTriggerCurrentPercentage < randomTriggerPercentage && !isMessaged && GameNetworkManager.Instance.isHostingGame)
                {
                    ShipEmergencyLeaveRPC();
                }
            }

            normalizedTimeOfDayB = ___normalizedTimeOfDay;
            shipLeaveAutomaticallyTimeB = ___shipLeaveAutomaticallyTime;
            shipLeavingEarlyDialogueB = ___shipLeavingEarlyDialogue;
            if (isNotCalled)
            {
                ___shipLeaveAutomaticallyTime = ___normalizedTimeOfDay + 0.0777f;
                isNotCalled = false;
            }
        }

        [ClientRpc]
        public static void ShipEmergencyLeaveRPC()
        {
            HUDManager.Instance.ReadDialogue(shipLeavingEarlyDialogueB);
            EmergencyActivatedBool.leavingActivated = true;
            isMessaged = true;
            isNotCalled = true;
            HUDManagerPatch.isTimerActivated = true;
        }
    }
}