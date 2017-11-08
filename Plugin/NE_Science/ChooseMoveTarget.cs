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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    class ChooseMoveTarget : MonoBehaviour
    {
        private List<ExperimentStorage> targets = null;
        private ExperimentData exp = null;
        private ScreenMessage smInfo = null;
        private ScreenMessage smError = null;
        private Part currentPart = null;
        private Part sourcePart = null;
        private List<Part> destinationParts = new List<Part>();

        private static Color dullOrange = new Color (0.8f, 0.4f, 0.2f);
        private static Color orange = new Color (1.0f, 0.9f, 0.4f);
        private static Color dullCyan = new Color (0f, 0.5f, 0.5f);

        void Awake()
        {
            // Disable updates until we're actually called
            this.enabled = false;
        }

        internal void showDialog(List<ExperimentStorage> targets, ExperimentData experimentData)
        {
            NE_Helper.log("start");
            this.targets = targets;
            exp = experimentData;
            NE_Helper.log("init done");
            if(exp == null || targets == null || targets.Count == 0)
            {
                this.enabled = false;
                return;
            }
            // Highlight source part
            sourcePart = exp.store.getPart();
            sourcePart.SetHighlightColor(dullOrange);
            sourcePart.SetHighlightType(Part.HighlightType.AlwaysOn);
            sourcePart.SetHighlight(true, false);

            // Create a list of destination parts and highlight them
            for (int i = 0, count = targets.Count; i < count; i++)
            {
                Part p = targets[i].part;
                if (p == sourcePart || destinationParts.Contains(p))
                {
                    continue;
                }
                destinationParts.Add(p);
                p.SetHighlightColor(dullCyan);
                p.SetHighlightType(Part.HighlightType.AlwaysOn);
                p.SetHighlight(true, false);
            }

            smInfo = ScreenMessages.PostScreenMessage(Localizer.Format("#ne_Select_a_part_to_transfer_1_to_ESC_to_cancel", exp.getAbbreviation()),
                15, ScreenMessageStyle.UPPER_CENTER);
            smInfo.color = Color.cyan;
            this.enabled = true;
            NE_Helper.LockUI();
        }

        void Update()
        {
            updatePartHover();
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                closeGui();
            }
            if (Input.GetMouseButtonDown(1))
            {
                //closeGui();
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (currentPart != null)
                {
                    onMouseClick(currentPart);
                }
            }
        }

        private void closeGui()
        {
            this.enabled = false;
            resetHighlight();
            if (smInfo != null)
            {
                ScreenMessages.RemoveMessage(smInfo);
                smInfo = null;
            }
            if (smError != null)
            {
                ScreenMessages.RemoveMessage(smError);
                smError = null;
            }
            destinationParts.Clear();
            // Delay unlocking UI to end of frame to prevent KSP from handling ESC key
            NE_Helper.RunOnEndOfFrame(this, NE_Helper.UnlockUI);
        }

        private void updatePartHover()
        {
            Part p = NE_Helper.GetPartUnderCursor();
            if( p == currentPart) return;

            if (currentPart != null)
            {
                onMouseHoverExit(currentPart);
            }
            if (p != null)
            {
                onMouseHoverEnter(p);
            }
            currentPart = p;
        }

        private void onMouseHoverExit(Part p)
        {
            if (p == sourcePart)
            {
                p.SetHighlightColor(dullOrange);
            }
            if (destinationParts.Contains(p))
            {
                p.SetHighlightColor(dullCyan);
            }
        }

        private void onMouseHoverEnter(Part p)
        {
            if (p == sourcePart)
            {
                p.SetHighlightColor(orange);
            }
            if (destinationParts.Contains(p))
            {
                p.SetHighlightColor(Color.cyan);
            }
        }

        private void onMouseClick(Part p)
        {
            if(destinationParts.Contains(p))
            {
                ExperimentStorage es = getTargetForPart(p);
                if (es != null)
                {
                    exp.moveTo(es);
                    closeGui();
                }
            }
            else if (p == sourcePart)
            {
                showError("#ne_This_is_the_source_part");
            }
            else
            {
                showError("#ne_This_is_an_invalid_part");
            }
        }

        private void showError(string msg)
        {
            if (smError != null)
            {
                ScreenMessages.RemoveMessage(smError);
            }
            smError = ScreenMessages.PostScreenMessage(msg, 5, ScreenMessageStyle.UPPER_CENTER);
            smError.color = orange;
        }

        private ExperimentStorage getTargetForPart(Part p)
        {
            for(int i = 0, count = targets.Count; i < count; i++)
            {
                if( targets[i].part == p)
                    return targets[i];
            }
            return null;
        }

        private void resetHighlight()
        {
            sourcePart.SetHighlightDefault();
            for(int i = 0, count = destinationParts.Count; i < count; i++)
            {
                destinationParts[i].SetHighlightDefault();
            }
        }
    }
}
