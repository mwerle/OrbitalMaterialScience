/*
 *   This file is part of Orbital Material Science.
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

using KSP.Localization;

namespace NE_Science
{
    public class OMSExperiment : ModuleScienceExperiment
    {
        public const string COMPLETED = "completed";

        public const int NOT_READY = 0;
        public const int READY = 1;
        public const int RUNNING = 2;
        public const int FINISHED = 3;
        public const int FINALIZED = 4;
        public const int ERROR = 5;

        [KSPField(isPersistant = true)]
        public string last_subjectId = "";

        [KSPField(isPersistant = true)]
        public int state = NOT_READY;

        [KSPField(isPersistant = false, guiActive = true, guiName = "#ne_Status")]
        public string expStatus = "";

        public string notReadyStatus = Localizer.GetStringByTag("#ne_No_Lab_Available");
        public string readyStatus = Localizer.GetStringByTag("#ne_Lab_Available");
        public string runningStatus = Localizer.GetStringByTag("#ne_Running");
        public string finishedStatus = Localizer.GetStringByTag("#ne_Research_Done");
        public string finalizedStatus = Localizer.GetStringByTag("#ne_Finalized");
        public string errorStatus = Localizer.GetStringByTag("#ne_Lab_Failure");

        [KSPField(isPersistant = true)]
        public float launched = 0;

        [KSPField(isPersistant = true)]
        public float completed = 0;

        public static bool checkBoring(Vessel vessel, bool msg = false)
        {
            if (NE_Helper.debugging())
            {
                return false;
            }
            // MKW TODO: Check if CelestialBody can be compared like this
            if ((vessel.orbit.referenceBody == Planetarium.fetch.Home) && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.SPLASHED || vessel.altitude <= vessel.orbit.referenceBody.atmosphereDepth))
            {
                if (msg) ScreenMessages.PostScreenMessage("#ne_Too_boring_here_Go_to_space", 6, ScreenMessageStyle.UPPER_CENTER);
                return true;
            }
            return false;
        }

    }
}
