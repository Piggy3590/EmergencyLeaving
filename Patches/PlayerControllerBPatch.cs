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

namespace EmergencyTakeoff.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : MonoBehaviour
    {
        public static float timerFloat;
        public static float timerFloatPublic;
        public static string timerString;

        public static float removeTextTimer;
        public static int removeTextCount;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_PostFix()
        {
            timerFloat = 60;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        private static void Update_PreFix(ref float ___sprintMeter)
        {
            if (EmergencyActivatedBool.leavingActivated)
            {
                ___sprintMeter = 1f;

                if (timerFloat > 0f)
                {
                    timerFloat -= Time.deltaTime / 4;
                }

                if (timerFloat <= 0f)
                {
                    timerFloat = 0f;
                    timerString = "<color=red>00.00</color>";
                    removeTextTimer += Time.deltaTime / 4;
                }
                else
                {
                    timerString = (Mathf.Floor(timerFloat * 100f) / 100f).ToString();
                }

                if (removeTextTimer <= 0 && timerFloat >= 10 || timerFloat <= 0)
                {
                    HUDManagerPatch.TimerTextGUI.text = timerString;
                }
                else if (removeTextTimer <= 0 && timerFloat > 0 && timerFloat < 10)
                {
                    HUDManagerPatch.TimerTextGUI.text = "<color=red>0" + timerString + "</color>";
                }

                if (removeTextTimer <= 0.8f && removeTextTimer >= 0.5f && removeTextCount <= 3)
                {
                    HUDManagerPatch.TimerTextGUI.text = timerString;
                }
                if (removeTextTimer >= 1f && removeTextCount <= 3)
                {
                    HUDManagerPatch.TimerTextGUI.text = "";
                    removeTextCount += 1;
                    removeTextTimer = 0f;
                }

                if (removeTextCount >= 6)
                {
                    HUDManagerPatch.TimerTextGUI.text = "";
                }
            }
        }
    }
}