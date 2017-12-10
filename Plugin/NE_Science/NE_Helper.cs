﻿/*
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

namespace NE_Science
{
    using KAC;

    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    class NE_Helper : MonoBehaviour
    {

        private static string SETTINGS_FILE;
        private const string DEBUG_VALUE = "Debug";
        private static bool debug = true;

        void Start()
        {
            loadOrCreateSettings();
            DontDestroyOnLoad(this);
            if (!KACWrapper.APIReady)
            {
                KACWrapper.InitKACWrapper();
            }
        }

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
                    settings.AddValue(DEBUG_VALUE, false);
                    settings.Save(SETTINGS_FILE);
                } else {
                    d = bool.Parse(settings.GetValue(DEBUG_VALUE));
                }
            }
            catch (Exception e)
            {
                d = true;
                NE_Helper.logError("Loading Settings: " + e.Message);
            }
            NE_Helper.debug = d;
        }

        /// <summary>
        /// Returns the ConfigNode's value as an int, or 0 on failure.
        /// </summary>
        /// <returns>The node value as int.</returns>
        /// <param name="node">The Node from which to retrieve the Value.</param>
        /// <param name="name">The name of the Value to retrieve.</param>
        public static int GetValueAsInt(ConfigNode node, string name)
        {
            int rv = 0;
            try {
                if (!node.TryGetValue(name, ref rv))
                {
                    rv = 0;
                }
            } catch (Exception e) {
                logError("GetValueAsInt - exception: " + e.Message);
            }
            return rv;
        }

        /// <summary>
        /// Returns the ConfigNode's value as a float, or 0f on failure.
        /// </summary>
        /// <returns>The node value as float.</returns>
        /// <param name="node">The Node from which to retrieve the Value.</param>
        /// <param name="name">The name of the Value to retrieve.</param>
        public static float GetValueAsFloat(ConfigNode node, string name)
        {
            float rv = 0f;
            try {
                if (!node.TryGetValue(name, ref rv))
                {
                    rv = 0f;
                }
            } catch (Exception e) {
                logError("GetValueAsFloat - exception: " + e.Message);
            }
            return rv;
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
        public static KACWrapper.KACAPI getKAC()
        {
            KACWrapper.KACAPI ka = null;

            if (!KACWrapper.APIReady)
            {
                /* NB: Re-try initialization here because Start() seems to get called too early.. */
                if(!KACWrapper.InitKACWrapper())
                {
                    goto done;
                }
            }
            ka = KACWrapper.KAC;

        done:
            return ka;
        }

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

        /** Adds a Kerbal Alarm Clock alarm for the Experiment. */
        public static bool AddAlarm(StepExperimentData e)
        {
            bool rv = false;

            float timeRemaining = e.getTimeRemaining();

            String aID = KACAPI.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.ScienceLab, "NEOS Alarm", Planetarium.GetUniversalTime() + timeRemaining);
            if (aID == "")
            {
                /* Unable to create alarm */
                goto done;
            }
            /* Set some additional alarm parameters */
            KACWrapper.KACAPI.KACAlarm a = KACAPI.Alarms.Find(z=>z.ID==aID);
            a.Notes = "Alarm for " + e.getName();
            a.AlarmAction = KACWrapper.KACAPI.AlarmActionEnum.KillWarp;
            a.AlarmMargin = 30; /* Fire 30 seconds before due time */
            a.VesselID = e.store.getPart().vessel.id.ToString();

            rv = true;

        done:
            return rv;
        }
    }
}
