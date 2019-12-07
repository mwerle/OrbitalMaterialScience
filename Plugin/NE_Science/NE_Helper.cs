/*
 *   This file is part of Orbital Material Science.
 *
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
 *
 *   Orbital Material Science is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Orbital Material Sciencee is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Orbital Material Science.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    using KAC;

    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    class NE_Helper : MonoBehaviour
    {

        private static string SETTINGS_FILE;
        private const string SETTINGS_DEBUG = "Debug";
        //private const string SETTINGS_NODE_KAC = "KerbalAlarmClock";
        //private const string SETTINGS_VALUE_ENABLED = "Enabled";
        //private const string SETTINGS_VALUE_ALARM_MARGIN = "AlarmMargin";
        private static bool debug = true;
        //private static bool setting_KAC_Enabled = false;
        //private static int setting_KAC_AlarmMargin = 0;

        void Start()
        {
            loadOrCreateSettings();
            DontDestroyOnLoad(this);
            if (!KACWrapper.APIReady)
            {
                KACWrapper.InitKACWrapper();
            }
        }

        /// <summary>
        /// Loads or creates the global settings.
        /// </summary>
        /// <remarks>
        /// TODO: Refactor and wrap in its own class if we're going to add
        /// more settings!
        /// </remarks>
        private void loadOrCreateSettings()
        {
            bool d = false;
            try
            {
                if (String.IsNullOrEmpty(SETTINGS_FILE)) {
                    SETTINGS_FILE = KSPUtil.ApplicationRootPath + "GameData/NehemiahInc/NE_Science_Common/Resources/settings.cfg";
                }
                ConfigNode settings = ConfigNode.Load(SETTINGS_FILE);
                if (settings == null)
                {
                    settings.AddValue(SETTINGS_DEBUG, false);
                    /*
                    ConfigNode cnSettingsKAC = settings.AddNode(SETTINGS_NODE_KAC);
                    cnSettingsKAC.AddValue(SETTINGS_VALUE_ENABLED, true);
                    cnSettingsKAC.AddValue(SETTINGS_VALUE_ALARM_MARGIN, 0);
                    */
                    settings.Save(SETTINGS_FILE);
                } else {
                    d = bool.Parse(settings.GetValue(SETTINGS_DEBUG));
                    /*
                    if (settings.HasNode(SETTINGS_NODE_KAC))
                    {
                        ConfigNode cnSettingsKAC = settings.GetNode(SETTINGS_NODE_KAC);
                        setting_KAC_Enabled = GetValueAsBool(cnSettingsKAC, SETTINGS_VALUE_ENABLED);
                        setting_KAC_AlarmMargin = GetValueAsInt(cnSettingsKAC, SETTINGS_VALUE_ALARM_MARGIN, 0);
                    }
                    else
                    {
                        ConfigNode cnSettingsKAC = settings.AddNode(SETTINGS_NODE_KAC);
                        cnSettingsKAC.AddValue(SETTINGS_VALUE_ENABLED, true);
                        cnSettingsKAC.AddValue(SETTINGS_VALUE_ALARM_MARGIN, 0);
                        settings.Save(SETTINGS_FILE);
                    }
                    */
                }
            }
            catch (Exception e)
            {
                NE_Helper.logError("Loading Settings: " + e.Message);
            }
            NE_Helper.debug = d;
        }

        /// <summary>
        /// Returns TRUE if the part technology is available.
        /// </summary>
        /// <returns><c>true</c>, if part technology available, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        public static bool IsPartTechAvailable(string name)
        {
            AvailablePart part = PartLoader.getPartInfoByName(name);
            return (part != null && ResearchAndDevelopment.PartTechAvailable(part));
        }

        /// <summary>
        /// Returns TRUE if the part is available, that is, the tech-node has been unlocked and
        /// the part has been purchased or is experimental.
        /// </summary>
        /// <returns><c>true</c>, if part available, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        public static bool IsPartAvailable(string name)
        {
            AvailablePart part = PartLoader.getPartInfoByName(name);
            return (part != null && ResearchAndDevelopment.PartModelPurchased(part));
        }


        /// <summary>
        /// Returns TRUE if the current view is inside the specified Part
        /// </summary>
        /// This code was largely copied from the "RasterPropMonitor" mod.
        /// <param name="p"></param>
        /// <returns>TRUE if the current view is in the IVA of the provided Part</returns>
        public static bool IsUserInIVA(Part p)
        {
           // Just in case, check for whether we're not in flight.
            if (!HighLogic.LoadedSceneIsFlight)
            {
                return false;
            }

            if( p == null )
            {
                return false;
            }

            // If the part does not have an instantiated IVA, or isn't attached to an active vessel the user can't be in it.
            if (p.internalModel == null || p.vessel == null || !p.vessel.isActiveVessel)
            {
                return false;
            }

            // If the camera view isn't in IVA then the user can't be in it either.
            if (CameraManager.Instance == null ||
                (CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA
                && CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.Internal))
            {
                return false;
            }

            // Now that we got that out of the way, we know that the user is in SOME pod on our ship. We just don't know which.
            // Let's see if he's controlling a kerbal in our pod.
            Kerbal currKerbal = CameraManager.Instance.IVACameraActiveKerbal;
            if (currKerbal != null && currKerbal.InPart == p)
            {
                return true;
            }

            // There still remains an option of InternalCamera which we will now sort out.
            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal)
            {
                // So we're watching through an InternalCamera. Which doesn't record which pod we're in anywhere, like with kerbals.
                // But we know that if the camera's transform parent is somewhere in our pod, it's us.
                // InternalCamera.Instance.transform.parent is the transform the camera is attached to that is on either a prop or the internal itself.
                // The problem is figuring out if it's in our pod, or in an identical other pod.
                // Unfortunately I don't have anything smarter right now than get a list of all transforms in the internal and cycle through it.
                // This is a more annoying computation than looking through every kerbal in a pod (there's only a few of those,
                // but potentially hundreds of transforms) and might not even be working as I expect. It needs testing.
                Transform[] componentTransforms = p.internalModel.GetComponentsInChildren<Transform>();
                foreach (Transform thisTransform in componentTransforms)
                {
                    if (thisTransform == InternalCamera.Instance.transform.parent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool isKacEnabled()
        {
            //return setting_KAC_Enabled;
            return HighLogic.CurrentGame.Parameters.CustomParams<NE_Settings>().KAC_Enabled;
        }

        public static int getKacAlarmMargin()
        {
            //return setting_KAC_AlarmMargin;
            return HighLogic.CurrentGame.Parameters.CustomParams<NE_Settings>().KAC_AlarmMargin;
        }

        public static bool debugging()
        {
            return debug;
        }

        public static void log( string msg)
        {
            if (debug)
            {
                Debug.Log("[NE] " + msg);
            }
        }

        public static void logError(string errMsg){
            Debug.LogError("[NE] Error: " + errMsg);
        }

        /** Prints the time in years, days, and hours (and optionally minutes) */
        public static string timeToStr(long timeInSeconds, bool printMinutes = false)
        {
            string ts = "";

            int HoursPerDay = GameSettings.KERBIN_TIME ? 6 : 24;
            int DaysPerYear = GameSettings.KERBIN_TIME ? 426 : 365;

            long minutes = timeInSeconds / 60;
            timeInSeconds -= minutes * 60;
            long hours = minutes / 60;
            minutes -= hours * 60;
            long days = hours / HoursPerDay;
            hours -= days * HoursPerDay;
            long years = days / DaysPerYear;
            days -= years * DaysPerYear;

            bool hasTime = false;
            try {
                if (years > 0)
                {
                    ts += " " + years + " " + Localizer.GetStringByTag("#ne_Years");
                    hasTime = true;
                }
                if (days > 0)
                {
                    ts += " " + days + " " + Localizer.GetStringByTag("#ne_Days");
                    hasTime = true;
                }
                if (hours > 0)
                {
                    ts += " " + hours + " " + Localizer.GetStringByTag("#ne_Hours");
                    hasTime = true;
                }
                if (printMinutes)
                {
                    if (minutes > 0)
                    {
                        ts += " " + minutes + " " + Localizer.GetStringByTag("#ne_Minutes");
                    }
                    else if(!hasTime)
                    {
                        ts += " < 1 " + Localizer.GetStringByTag("#ne_Minutes");
                    }
                }
                else if (!hasTime)
                {
                    ts += " < 1 " + Localizer.GetStringByTag("#ne_Hours");
                }
            }
            catch(Exception e)
            {
                NE_Helper.logError("Failed to convert time to string: " + e.ToString());
            }

            return ts;
        }

        // Returns the part which is currently under the mouse cursor
        // Thanks to KospY (http://forum.kerbalspaceprogram.com/index.php?/topic/99180-mouse-over-a-part/)
        // <returns>Part currently under the mouse cursor
        // <returns>null if no part is under the mouse cursor
        public static Part GetPartUnderCursor()
        {
            RaycastHit hit;
            Part part = null;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000, 557059))
            {
                part = hit.collider.gameObject.GetComponentInParent<Part>();
            }
            return part;
        }


        /// <summary>Acquires NEOS input lock on UI interactions.</summary>
        public static void LockUI()
        {
            InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "NEOS");
            NE_Helper.log("NEOS UI lock acquired");
        }

        /// <summary>Releases KIS input lock on UI interactions.</summary>
        public static void UnlockUI()
        {
            InputLockManager.RemoveControlLock("NEOS");
            NE_Helper.log("NEOS UI lock released");
        }

        /// <summary>
        /// Blocks until end of frame and then runs <em>action</em>.
        /// </summary>
        /// Ideally this should be called as a coroutine to allow the main logic to continue running.
        /// <param name="action">The action to run.</param>
        /// <returns></returns>
        private static System.Collections.IEnumerator _runAtEndOfFrame(Action action)
        {
            yield return null;
            action();
        }

        /// <summary>
        /// Schedule an action to be executed at the end of the frame.
        /// </summary>
        /// Note that the action will run as a coroutine, so any shared state must be protected by a lock.
        /// <param name="behaviour">The context in which to run the action.</param>
        /// <param name="action">The action to run at the end of the frame.</param>
        public static void RunOnEndOfFrame(MonoBehaviour behaviour, Action action)
        {
            behaviour.StartCoroutine(_runAtEndOfFrame(action));
        }

        private static KACWrapper.KACAPI ka = null;
        /** Wrapper around accessing the Kerbal Alarm Clock API.
         * This wrapper will initialize the KAC API if necessary.
         */
        public static KACWrapper.KACAPI KACAPI {
            get
            {
                if (ka == null)
                {
                    if (!KACWrapper.APIReady)
                    {
                        /* NB: Re-try initialization here because Start() seems to get called too early.. */
                        if(!KACWrapper.InitKACWrapper())
                        {
                            goto done;
                        }
                    }
                    ka = KACWrapper.KAC;
                }
            done:
                return ka;
            }
        }

        /** Returns true if the KAC API is available. */
        public static bool isKacAvailable()
        {
            return ka != null;
        }

        public static KACWrapper.KACAPI.KACAlarm FindAlarm(string alarmId)
        {
            return KACAPI?.Alarms.Find(z => z.ID == alarmId);
        }

        /** Adds an alarm for the experiment.
         * @param timeRemaining The time, in seconds, when the experiment will complete.
         * @param alarmTitle The title of the alarm, shown in the main KAC window, generally "NEOS Alarm" or "KEES Alarm" etc.
         * @param experimentName The name of the experiment.
         * @return On success, returns the id of the alarm which was created, on failure, null.
         */
        public static string AddExperimentAlarm(
                float timeRemaining, string alarmTitle, string experimentName, Vessel v)
        {
            string aID = null;

            if( timeRemaining < 10.0 )
            {
                goto done;
            }

            if (!isKacEnabled())
            {
                goto done;
            }

            var alarmMargin = getKacAlarmMargin();
            var alarmTime = Planetarium.GetUniversalTime() + timeRemaining - alarmMargin;

            aID = KACAPI?.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.ScienceLab, alarmTitle, alarmTime);
            if (string.IsNullOrEmpty(aID))
            {
                /* Unable to create alarm */
                goto done;
            }
            /* Set some additional alarm parameters */
            KACWrapper.KACAPI.KACAlarm alarm = FindAlarm(aID);
            alarm.Notes = "Alarm for " + experimentName;
            alarm.AlarmAction = KACWrapper.KACAPI.AlarmActionEnum.KillWarp;
            alarm.AlarmMargin = alarmMargin;
            alarm.VesselID = v?.id.ToString();

        done:
            return aID;
        }

        /** Deletes a KAC alarm by ID*/
        public static bool DeleteAlarm(string alarmId)
        {
            if( !isKacEnabled() || string.IsNullOrEmpty(alarmId) )
            {
                return false;
            }
            return KACAPI.DeleteAlarm(alarmId);
        }

        /** Deletes a KAC alarm */
        public static bool DeleteAlarm(KACWrapper.KACAPI.KACAlarm alarm)
        {
            if (!isKacEnabled() || alarm == null)
            {
                return false;

            }
            return KACAPI.DeleteAlarm(alarm.ID);
        }
    }

    /// <summary>
    /// This class implements Extension Methods for various other classes.
    /// </summary>
    /// It should be noted that Extension Methods can be called even if the
    /// variable is null, so care must be taken inside the extension methods
    /// not to cause NullReferenceExceptions.
    public static class NE_ExtensionMethods
    {
        /// <summary>
        /// Extension Method on Unity Objects returning whether the object really is null.
        /// </summary>
        /// Unity overloads the '==' operator so that it returns true on both null references
        /// as well as references to destroyed objects. This function only returns true if
        /// the reference truly is null, and returns false for "fake null" objects.
        public static bool IsTrueNull(this UnityEngine.Object ob)
        {
            return (object)ob == null;
        }

        /// <summary>
        /// Checks whether the current situation is "boring" or not.
        /// </summary>
        /// "Boring" is currently defined as being on Kerbin and within the atmosphere.
        /// <param name="vessel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool isBoring(this Vessel vessel, bool msg = false)
        {
            if (NE_Helper.debugging())
            {
                return false;
            }
            // MKW TODO: Check if CelestialBody can be compared like this
            if ((vessel.orbit.referenceBody == Planetarium.fetch.Home) &&
                (vessel.situation == Vessel.Situations.LANDED ||
                vessel.situation == Vessel.Situations.PRELAUNCH ||
                vessel.situation == Vessel.Situations.SPLASHED ||
                vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth))
            {
                if (msg) ScreenMessages.PostScreenMessage("#ne_Too_boring_here_Go_to_space", 6, ScreenMessageStyle.UPPER_CENTER);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns the ConfigNode's value as a bool, or the defaultValue on failure.
        /// </summary>
        /// If the `name` cannot be found in `node`, `defaultValue` is returned.
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(this ConfigNode node, string name, bool defaultValue = false)
        {
            bool rv = defaultValue;
            try {
                if (!node.TryGetValue(name, ref rv))
                {
                    rv = defaultValue;
                }
            } catch (Exception e) {
                NE_Helper.logError("NE_ExtensionMethods.GetBool - exception: " + e.Message);
            }
            return rv;
        }

        /// <summary>
        /// Returns the ConfigNode's value as an int, or the defaultValue on failure.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetInt(this ConfigNode node, string name, int defaultValue = 0)
        {
            int rv = defaultValue;
            try {
                if (!node.TryGetValue(name, ref rv))
                {
                    rv = defaultValue;
                }
            } catch (Exception e) {
                NE_Helper.logError("NE_ExtensionMethods.GetInt - exception: " + e.Message);
            }
            return rv;
        }

        /// <summary>
        /// Returns the ConfigNode's value as a float, or the defaultValue on failure.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float GetFloat(this ConfigNode node, string name, float defaultValue = 0.0f)
        {
            float rv = defaultValue;
            try {
                if (!node.TryGetValue(name, ref rv))
                {
                    rv = defaultValue;
                }
            } catch (Exception e) {
                NE_Helper.logError("NE_ExtensionMethods.GetFloat - exception: " + e.Message);
            }
            return rv;
        }

        /// <summary>
        /// Returns the ConfigNode's value as an enumeration, or the defaultValue on failure.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TEnum GetEnum<TEnum>(this ConfigNode node, string name, TEnum defaultValue) where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            TEnum rv = defaultValue;
            string val = "";
            try {
                if (!node.TryGetValue(name, ref val))
                {
                    if( !Enum.TryParse(val, true, out rv))
                    {
                        rv = defaultValue;
                    }
                }
            } catch (Exception e) {
                NE_Helper.logError("NE_ExtensionMethods.GetEnum - exception: " + e.Message);
            }
            return rv;
        }
    }
}
