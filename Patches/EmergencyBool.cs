using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace EmergencyTakeoff.Patches
{
    public static class EmergencyBool
    {
        public static bool canBeLeave;
    }
}