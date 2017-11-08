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
    class ChooseTestSubject : MonoBehaviour
    {
        private List<string> availableSubjects = new List<string>();
        private KerbalResearchExperimentData exp;

        private ExperimentStep.startCallback cbMethod;

        internal void showDialog(List<string> avilableSubjects, KerbalResearchExperimentData experimentData, ExperimentStep.startCallback cbMethod)
        {
            this.availableSubjects = avilableSubjects;
            exp = experimentData;
            this.cbMethod = cbMethod;
            showMoveWindow();
        }

        void OnGUI()
        {
            // TODO: Dismiss dialog if user clicks outside of the window
        }

        /* +------------------------------------------------+
         * |         Choose Test Subject                    |
         * +------------------------------------------------+
         * | | Choose a Kerbal:                         |^| |
         * | | <Kerbal>                          [Test] | | |
         * | | <Kerbal>                          [Test] | | |
         * | | <Kerbal>                          [Test] | | |
         * | |                                          | | |
         * | | Already Tested:                          | | |
         * | | <Kerbal>                                 | | |
         * | | <Kerbal>                                 |v| |
         * | Test subjects needed: <x> / <y>                |
         * |                  [Close]                       |
         * +------------------------------------------------+
        */
        void showMoveWindow()
        {
            // Adjust the default UI style for our window
            var style = HighLogic.UISkin;
            style.button.fontSize = 15;
            style.button.stretchHeight = false;
            style.button.stretchWidth = false;
            style.label.fontSize = 15;
            style.label.stretchHeight = false;
            style.label.stretchWidth = false;
            style.label.wordWrap = false;

            var noPad = new RectOffset();
            DialogGUIButton b;
            DialogGUILabel l;
            DialogGUIHorizontalLayout hl;
            DialogGUIVerticalLayout vl;

            int numAvailable = availableSubjects.Count;
            List<string> testedSubjects = getTestedKerbalNames();
            int numTested = testedSubjects.Count;

            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            // Window Header - info string
            l = new DialogGUILabel(Localizer.Format("#ne_For_1", exp.getName()));
            hl = new DialogGUIHorizontalLayout(false, false, 4, noPad, TextAnchor.MiddleCenter, l);
            dialog.Add(hl);

            // Window Contents - scroll list of available and tested Kerbals
            vl = new DialogGUIVerticalLayout(true, false);
            vl.padding = new RectOffset(6, 24, 6, 6); // Padding between border and contents - ensure we don't overlay content over scrollbar
            vl.spacing = 4; // Spacing between elements
            vl.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));


            // Show available Kerbals
            // TODO: Make this a bigger font / different colour
            l = new DialogGUILabel("#ne_Available_Kerbals");
            hl = new DialogGUIHorizontalLayout(false, false, 4, noPad, TextAnchor.MiddleCenter, l);
            vl.AddChild(hl);
            if (numAvailable > 0)
            {
                for (int i = 0, count = availableSubjects.Count; i < count; i++)
                {
                    b = new DialogGUIButton<string>("#ne_Test", onKerbalClicked, availableSubjects[i], true);
                    b.size = new Vector2(60,30);
                    // TODO: Increase label font to match button size
                    l = new DialogGUILabel(availableSubjects[i], true, false);
                    hl = new DialogGUIHorizontalLayout(false, false, 4, noPad, TextAnchor.MiddleCenter, l, b);
                    vl.AddChild(hl);
                }
            }

            // Show already-tested Kerbals
            vl.AddChild(new DialogGUISpace(4));
            l = new DialogGUILabel("#ne_Tested_Kerbals");
            hl = new DialogGUIHorizontalLayout(false, false, 4, noPad, TextAnchor.MiddleCenter, l);
            vl.AddChild(hl);
            if (numTested > 0)
            {
                // TODO: Make this a bigger font / different colour
                for (int i = 0, count = testedSubjects.Count; i < count; i++)
                {
                    l = new DialogGUILabel(testedSubjects[i], true, false);
                    vl.AddChild(l);
                }
            }

            // Add scroll list to window, having it expand to fill all available space both horizontally and vertically
            dialog.Add(new DialogGUIHorizontalLayout(true,true, new DialogGUIScrollList(Vector2.one, false, true, vl)));

            // Window footer - "Needed" info and Close button
            dialog.Add(new DialogGUILabel(Localizer.Format("#ne_Needed_1_of_2", exp.getTestSubjectsNeeded() - numTested, exp.getTestSubjectsNeeded())));
            b = new DialogGUIButton("#ne_Close", onCloseClicked, true);
            b.size = new Vector2(60,30);
            dialog.Add(new DialogGUIHorizontalLayout(
                    new DialogGUIFlexibleSpace(),
                    b,
                    new DialogGUIFlexibleSpace()
                )
            );

            // Position -and- size of dialog
            var rct = new Rect(0.5f, 0.5f, 400, 400);
            var mod = new MultiOptionDialog("", "", "#ne_Choose_Test_Subject", HighLogic.UISkin, rct, dialog.ToArray());
            PopupDialog.SpawnPopupDialog(mod, false, HighLogic.UISkin);
        }

        // Returns a list (possibly empty) of already-tested Kerbal names
        private List<string> getTestedKerbalNames()
        {
            List<string> rv = new List<string>();
            if (exp.getActiveStepIndex() > 0)
            {
                var expSteps = exp.getExperimentSteps();
                for (int i = 0, count = expSteps.Count; i < count; i++)
                {
                    var krs = expSteps[i];
                    if (krs.getSubjectName() != "")
                    {
                        rv.Add(krs.getSubjectName());
                    }
                }
            }
            return rv;
        }

        private void onKerbalClicked(string subject)
        {
            exp.getActiveStep().start(subject, cbMethod);
        }

        private void onCloseClicked()
        {
            cbMethod(false);
        }
    }
}
