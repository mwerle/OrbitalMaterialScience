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
using KSP.Localization;

namespace NE_Science
{
    public class Kemini_Module : Lab, IScienceResultHelperClient
    {

        private const string KEMINI_LAB_EQUIPMENT_TYPE = "KEMINI";

        private LabEquipmentSlot keminiSlot = new LabEquipmentSlot(EquipmentRacks.KEMINI);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("KL OnLoad");
            keminiSlot = getLabEquipmentSlotByType(node, KEMINI_LAB_EQUIPMENT_TYPE);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("KL OnSave");
            node.AddNode(keminiSlot.getConfigNode());
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            keminiSlot.onStart(this);
            if (!keminiSlot.isEquipmentInstalled())
            {
                LabEquipment keminiLab = new LabEquipment("KL", "Kemini Lab", EquipmentRacks.KEMINI, 0f, 0f, 1f, Resources.LAB_TIME, 10f, Resources.ELECTRIC_CHARGE);
                keminiSlot.install(keminiLab, this);
            }
            Fields["labStatus"].guiName = "#ne_Kemini_Lab";
        }


        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case EquipmentRacks.KEMINI:
                    if (keminiSlot.isEquipmentInstalled() && keminiSlot.experimentSlotFree())
                    {
                        keminiSlot.installExperiment(exp);
                        displayStatusMessage( keminiSlot.getExperiment().displayStateString() );
                        keminiSlot.experimentAction();

                        ScienceResultHelper.Instance.Register(this);
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + keminiSlot.isEquipmentInstalled() + "; free: " + keminiSlot.experimentSlotFree());
                    }
                    break;
            }
        }

        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.KEMINI:

                    keminiSlot.install(le, this);
                    break;
            }
        }

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.experimentSlotFree();
                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.isEquipmentRunning();

                default:
                    return false;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            if (keminiSlot.experimentSlotFree())
            {
                labStatus = s;
            }
            else
            {
                labStatus = keminiSlot.getExperiment().getAbbreviation() + ": " + s;
            }
            Fields["labStatus"].guiActive = true;
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;

            if (keminiSlot.isEquipmentRunning())
            {
                Events["labAction"].guiName = doResearch? "#ne_Pause_Research" : "#ne_Resume_Research";
                Events["labAction"].active = true;
            }
            else
            {
                Events["labAction"].active = false;
            }

            if (keminiSlot.isEquipmentInstalled())
            {
                Events["moveKeminiExp"].active = keminiSlot.canExperimentMove(part.vessel);
                if (Events["moveKeminiExp"].active)
                {
                    Events["moveKeminiExp"].guiName = Localizer.Format("#ne_Move_1", keminiSlot.getExperiment().getAbbreviation());
                }

                if (keminiSlot.canActionRun())
                {
                    string keminiActionString = keminiSlot.getActionString();
                    Events["actionKeminiExp"].guiName = keminiActionString;
                }
                Events["actionKeminiExp"].active = keminiSlot.canActionRun();
                if (!keminiSlot.experimentSlotFree())
                {
                    //displayStatusMessage( keminiSlot.getExperiment().getAbbreviation() + ": " + keminiSlot.getExperiment().displayStateString() );
                    displayStatusMessage( keminiSlot.getExperiment().displayStateString() );
                }
            }
        }

        protected override bool onLabPaused()
        {
            if(! base.onLabPaused() )
            {
                return false;
            }

            /* Delete all alarms */
            keminiSlot?.getExperiment()?.onPaused();
            return true;
        }

        protected override bool onLabStarted()
        {
            if(! base.onLabStarted() )
            {
                return false;
            }

            /* Create alarms for any running experiments */
            keminiSlot?.getExperiment()?.onResumed();
            return true;
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_Kemini_Experiment", active = false)]
        public void moveKeminiExp()
        {
            keminiSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_Kemini_Experiment", active = false)]
        public void actionKeminiExp()
        {
            keminiSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            return ret;
        }


        #region ExperimentResultDialog interface and callbacks
        /// <summary>
        /// ExperimentResultHelperClient interface
        /// </summary>
        public Part getPart()
        {
            return part;
        }
        public void OnExperimentResultDialogResetClicked()
        {
            NE_Helper.log("Kemini_Module: OnExperimentResultDialogResetClicked()");
        }
        public void OnExperimentResultDialogOpened()
        {
            NE_Helper.log("Kemini_Module: OnExperimentResultDialogOpened()");
            ScienceResultHelper.Instance.DisableButton(ScienceResultHelper.ExperimentResultDialogButton.ButtonReset);
        }
        public void OnExperimentResultDialogClosed ()
        {
            NE_Helper.log("Kemini_Module: OnExperimentResultDialogClosed()");
        }
        #endregion
    }
}
