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
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace NE_Science
{
    class ESCStorageManifest : PartModule
    {
        private const int width = 400;
        private const int fixedHeight = 50;
        private const int slotHeight = 100;
        private static int maxHeight = Screen.height - 100;

        private Rect manifestWindowRect = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 250, 250, 550);
        List<ExperimentStorage> storageSlots = new List<ExperimentStorage>();

        [KSPEvent(guiActiveEditor = true, guiActive = true, guiName = "#ne_Storage_Manifest", active = true)]
        public void storageManifest()
        {
            storageSlots = new List<ExperimentStorage>(part.GetComponents<ExperimentStorage>());
            showManifestWindow();
        }

        void OnGUI()
        {
        }

        /* +---------------------------------------------------+
         * |              Storage Manifest                     |
         * +---------------------------------------------------+
         * | OMS ESC 4/1                                    |^||
         * |         Material Exposure Experiment 1         | ||
         * |         Needs: MEP-825 and MPL-600 or MSL-1000 | ||
         * |         State: Finished                        | ||
         * |                                                | ||
         * | OMS ESC 4/2                                    | ||
         * |         empty                                  | ||
         * |                                                | ||
         * | OMS ESC 4/3                                    | ||
         * |         3D Printer Demonstration Test          |v||
         * |                  [Close]                          |
         * +---------------------------------------------------+
        */
        private void showManifestWindow()
        {
            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            // TODO? Add description of part for which we're displaying the manifest
            //dialog.Add(new DialogGUILabel("Chooose lab"));
            //dialog.Add(new DialogGUISpace(4));

            // Build a button list of all available experiments with their descriptions
            int numSlots = storageSlots.Count;
            DialogGUIBase[] scrollList = new DialogGUIBase[numSlots + 1];
            scrollList[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < numSlots; i++)
            {
                var e = storageSlots[i];
                sb.Append("<b>").Append(e.identifier).Append("</b>\n");
                if(e.isEmpty())
                {
                    sb.Append("\t ").Append(Localizer.Format("#ne_empty")).AppendLine();
                }
                else
                {
                    ExperimentData exp = e.getStoredExperimentData();
                    sb.Append(exp.getDescription("\t ")).AppendLine();
                    sb.Append("\t ").Append(Localizer.Format("#ne_Status")).Append(": ").Append(exp.displayStateString()).AppendLine();
                }

                var label = new DialogGUILabel(sb.ToString(), true, true);
                var h = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, label);
                scrollList[i + 1] = h;
                sb.Length = 0;
            }

            // NB: Specify a minimum size; the scroll-list will automatically expand to fill available space in the parent form.
            dialog.Add(
                new DialogGUIScrollList(new Vector2(300,300), false, true,
                //new DialogGUIScrollList(Vector2.one, false, true,
                new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.UpperLeft, scrollList)
            ));

            dialog.Add(new DialogGUISpace(4));

            // Add a centered "Close" button
            dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBase[]
            {
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton(Localizer.Format("#ne_Close"), null, true),
                new DialogGUIFlexibleSpace(),
            }));

            // Calculate size of dialog window
            Rect pos = new Rect(0.5f, 0.5f, width, fixedHeight + numSlots * slotHeight);
            if( pos.height > maxHeight )
            {
                pos.height = maxHeight;
            }

            // Actually create and show the dialog
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("", "", Localizer.Format("#ne_Storage_Manifest"), HighLogic.UISkin, pos, dialog.ToArray()),
                false, HighLogic.UISkin);
        }
    }
}
