using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmergencyTakeoff.Patches;
using UnityEngine;

namespace EmergencyTakeoff
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class EmergencyTakeoffModBase : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.EmergencyTakeoff";
        private const string modName = "EmergencyTakeoff";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static EmergencyTakeoffModBase Instance;

        public static ManualLogSource mls;

        public static int EventProbability;
        public static float maxProbability;
        public static float minProbability;

        public static bool showMessage;
        public static bool showTimer;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Emergency Takeoff is loaded");

            EmergencyTakeoffModBase.EventProbability = (int)base.Config.Bind<int>("General", "EventProbability", 8, "Chance of emergency return occurring (Probability and numbers are inversely proportional. If set to 1, it will always occur.)").Value;
            EmergencyTakeoffModBase.maxProbability = base.Config.Bind<float>("General", "MaximumProbabilityIncrease", 0.5f, "This is the maximum value at which the probability increases continuously. Must be higher than the minimum.").Value;
            EmergencyTakeoffModBase.minProbability = base.Config.Bind<float>("General", "MinimumProbabilityIncrease", 0.3f, "It is the minimum value at which the probability increases continuously. It must be lower than the maximum.").Value;

            EmergencyTakeoffModBase.showMessage = base.Config.Bind<bool>("General", "ShowTipMessage", false, "If enabled, displays a message after landing if the probability of an event occurring exists.").Value;
            EmergencyTakeoffModBase.showTimer = base.Config.Bind<bool> ("General", "ShowTimer", true, "Shows the departure timer in the lower left corner.").Value;

            StartOfRoundPatch.eventProbability = EventProbability;
            TimeOfDayPatch.minimumIncrease = minProbability;
            TimeOfDayPatch.maximumIncrease = maxProbability;


            HUDManagerPatch.isTimerActivated = showTimer;

            harmony.PatchAll(typeof(EmergencyTakeoffModBase));

            harmony.PatchAll(typeof(TimeOfDayPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));

            harmony.PatchAll(typeof(EmergencyActivatedBool));
            harmony.PatchAll(typeof(EmergencyBool));
        }
    }
}
