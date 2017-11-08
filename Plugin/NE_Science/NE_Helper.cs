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

namespace NE_Science
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class NE_Helper : MonoBehaviour
    {

        private static string SETTINGS_FILE;
        private const string DEBUG_VALUE = "Debug";
        private static bool debug = true;

        void Start()
        {
            loadOrCreateSettings();
            DontDestroyOnLoad(this);
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

            // If the part does not have an instantiated IVA, or isn't attached to an active vessel the user can't be in it.
            if (p == null || p.internalModel == null || p.vessel == null || !p.vessel.isActiveVessel)
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

        public static void log( string format, params object[] list )
        {
            if (debug)
            {
                Debug.LogFormat("[NE] " + format, list);
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
    } // END class NE_Helper


    public delegate void GameObjectVisitor(GameObject go, int indent);

    /// <summary>
    /// Class adding some extension methods to the GameObject class.
    /// </summary>
    public static class GOExtensions
    {
        // Dump object
        private static void internal_PrintComponents(GameObject go, int indent)
        {
            NE_Helper.log("{0}{1} has components:", indent > 0 ? new string('-', indent-2) + "> " : "", go.name);

            var components = go.GetComponents<Component>();
            foreach (var c in components)
                NE_Helper.log("{0}: {1} ({2})", new string('.', indent + 3) + "c", c.GetType().FullName, c.name);
        }

        public static void PrintComponents(this UnityEngine.GameObject go, int maxIndent = 0)
        {
            go.TraverseHierarchy(internal_PrintComponents, 0, maxIndent);
        }

        public static void TraverseHierarchy(this UnityEngine.GameObject go, GameObjectVisitor visitor, int indent = 0, int maxIndent = 0)
        {
            visitor(go, indent);

            for (int i = 0; i < go.transform.childCount; ++i)
            {
                go.transform.GetChild(i).gameObject.TraverseHierarchy(visitor, indent + 3, maxIndent);
                if (maxIndent > 0 && indent/3 > maxIndent)
                {
                    break;
                }
            }
        }
    } // END class GOExtensions

} // END namespace NE_Science
