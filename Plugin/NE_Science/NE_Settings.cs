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
 *
 *   ------------------------------------------------------
 * 
 *   This file implements integration with the Game Settings UI.
 *   For details, see this forum post: https://forum.kerbalspaceprogram.com/index.php?showtopic=147576
 *
 *   TODO: It's not really "Game Difficulty" we're adjusting here, so we
 *   should build our own settings dialog. But that will mean spamming
 *   another button on the launcher.
 */

using System.Reflection;

namespace NE_Science
{
    class NE_Settings : GameParameters.CustomParameterNode
    {
#warning LOCALISE
        public override string Title { get { return "Kerbal Alarm Clock Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string DisplaySection { get { return "Nehemiah Engineering\nOrbital Science"; } }
        public override string Section { get { return "NEOS"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("KAC Enabled", newGameOnly = false, toolTip = "Enable integration with Kerbal Alarm Clock")]
        public bool KAC_Enabled = true;

        [GameParameters.CustomIntParameterUI("KAC Alarm Margin", newGameOnly = false, minValue = 0, maxValue = 600, toolTip = "The number of seconds to trigger the alarm before an experiment has completed.")]
        public int KAC_AlarmMargin = 0;

        /** Selects whether a UI element is visible or not. */
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            //if (member.Name == "KAC_AlarmMargin")
            //    return KAC_Enabled;

            // Basically enable (show) all fields, and select whether or not they are
            // interactible below.
            return true;
        }

        /** Selects whether a UI element is interactible or not. */
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "KAC_AlarmMargin")
                return KAC_Enabled;
            return true; //otherwise return true
        }

        /** Adjust mod parameters depending on the difficulty level selected by the player. */
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            // Nothing to adjust (yet)
        }
    }
}
