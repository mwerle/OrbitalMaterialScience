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
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    public class MPL_Module : Lab
    {

        private const string MSG_LAB_EQUIPMENT_TYPE = "MSG";
        private const string USU_LAB_EQUIPMENT_TYPE = "USU";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "MSG")]
        public string msgStatus = "";

        [KSPField(isPersistant = false, guiActive = false, guiName = "USU")]
        public string usuStatus = "";

        private GameObject msg;
        private GameObject usu;

        private GameObject cfe;

        public Generator labTimeGenerator;

        private LabEquipmentSlot msgSlot = new LabEquipmentSlot(LabEquipmentType.MSG);
        private LabEquipmentSlot usuSlot = new LabEquipmentSlot(LabEquipmentType.USU);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MPL OnLoad");
            msgSlot = getLabEquipmentSlotByType(node, MSG_LAB_EQUIPMENT_TYPE);
            usuSlot = getLabEquipmentSlotByType(node, USU_LAB_EQUIPMENT_TYPE);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MPL OnSave");
            node.AddNode(msgSlot.getConfigNode());
            node.AddNode(usuSlot.getConfigNode());

        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            initERacksActive();

            labTimeGenerator = createGenerator(Resources.LAB_TIME, LabTimePerHour, Resources.ELECTRIC_CHARGE, ChargePerLabTime);
            generators.Add(labTimeGenerator);
            msgSlot.onStart(this);
            usuSlot.onStart(this);

        }

        private Generator createGenerator(string resToCreate, float creationRate, string useRes, float usePerUnit)
        {
            Generator gen = new Generator(this.part);
            gen.addRate(resToCreate, -creationRate);
            if (usePerUnit > 0)
                gen.addRate(useRes, usePerUnit);
            return gen;
        }

        private void initERacksActive()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>().name == "MPL_IVA")
                {
                    msg = labIVA.transform.GetChild(3).gameObject;

                    cfe = msg.transform.GetChild(2).GetChild(0).gameObject;
                    usu = labIVA.transform.GetChild(4).gameObject;

                    cfe.SetActive(!msgSlot.experimentSlotFree());
                    msg.SetActive(msgSlot.isEquipmentInstalled());
                    usu.SetActive(usuSlot.isEquipmentInstalled());

                    NE_Helper.log("init E Racks successfull");
                }
                else
                {
                    NE_Helper.logError("MPL mesh not found");
                }

            }
            else {
                NE_Helper.log("init E Racks internal model null");
            }
        }

        public override GameObject getExperimentGO(string id)
        {
            switch (id)
            {
                case "NE_CFE":
                case "NE_CCF":
                    return cfe;
                default:
                    return null;

            }
        }

        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case LabEquipmentType.MSG:
                    if (msgSlot.isEquipmentInstalled() && msgSlot.experimentSlotFree())
                    {
                        msgSlot.installExperiment(exp);
                        msgStatus = exp.getAbbreviation();
                        Fields["msgStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + msgSlot.isEquipmentInstalled() + "; free: " + msgSlot.experimentSlotFree());
                    }
                    break;
                case LabEquipmentType.USU:
                    if (usuSlot.isEquipmentInstalled() && usuSlot.experimentSlotFree())
                    {
                        usuSlot.installExperiment(exp);
                        usuStatus = exp.getAbbreviation();
                        Fields["usuStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + usuSlot.isEquipmentInstalled() + "; free: " + usuSlot.experimentSlotFree());
                    }
                    break;
            }
        }

        #region Equipment Management
        private void setEquipmentActive(LabEquipmentType rack)
        {
            switch (rack)
            {
                case LabEquipmentType.MSG:
                    if (msg != null)
                    {
                        msg.SetActive(msgSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (msg != null) msg.SetActive(msgSlot.isEquipmentInstalled());
                    }
                    break;
                case LabEquipmentType.USU:
                    if (usu != null)
                    {
                        usu.SetActive(usuSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (usu != null) usu.SetActive(usuSlot.isEquipmentInstalled());
                    }
                    break;
            }
        }

        public override bool hasEquipmentInstalled(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.MSG:
                    return msgSlot.isEquipmentInstalled();

                case LabEquipmentType.USU:
                    return usuSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public override bool hasEquipmentSlot(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.MSG:
                    // fall through 
                case LabEquipmentType.USU:
                    return true;

                default:
                    return false;
            }
        }

        public override bool hasFreeEquipmentSlot(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.MSG:
                    return !msgSlot.isEquipmentInstalled();

                case LabEquipmentType.USU:
                    return !usuSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public override void installLabEquipment(LabEquipment le)
        {
            switch (le.LabEquipmentType)
            {
                case LabEquipmentType.MSG:
                    if (tryInstallEquipment(le, msgSlot, msg))
                    {
                        cfe.SetActive(false);
                    }
                    else
                    {
                        NE_Helper.logError("installLabEquipment failed: FIR slot already full");
                    }
                    break;
                case LabEquipmentType.USU:
                    if (!tryInstallEquipment(le, usuSlot, usu))
                    {
                        NE_Helper.logError("installLabEquipment failed: CIR slot already full");
                    }
                    break;
                default:
                    NE_Helper.logError($"installLabEquipment failed: unsupported Equipment {le.Name} for Lab {this.name}");
                    throw new ArgumentException($"installLabEquipment failed: unsupported Equipment {le.Name} for Lab {this.name}", "le");
            }
        }
        #endregion


        public bool hasEquipmentFreeExperimentSlot(LabEquipmentType rack)
        {
            switch (rack)
            {
                case LabEquipmentType.MSG:
                    return msgSlot.experimentSlotFree();
                case LabEquipmentType.USU:
                    return usuSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(LabEquipmentType rack)
        {
            switch (rack)
            {
                case LabEquipmentType.MSG:
                    return msgSlot.isEquipmentRunning();
                case LabEquipmentType.USU:
                    return usuSlot.isEquipmentRunning();

                default:
                    return false;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            try {
            labStatus = s;
            Fields["labStatus"].guiActive = true;
            } catch (Exception e) {
                NE_Helper.logError("MPL_Module.displayStatusMessage(): caught exception " + e +"\n" + e.StackTrace);
            }
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;
            msgSlot.updateCheck();
            usuSlot.updateCheck();

            if (msgSlot.isEquipmentRunning() || usuSlot.isEquipmentRunning())
            {
                Events["labAction"].guiName = doResearch? "#ne_Pause_Research" : "#ne_Resume_Research";
                Events["labAction"].active = true;
            }
            else
            {
                Events["labAction"].active = false;
            }

            if (msg == null)
            {
                initERacksActive();
            }

            // MSG UI Buttons
            Fields["msgStatus"].guiActive = msgSlot.isEquipmentInstalled();
            if (Fields["msgStatus"].guiActive)
            {
                Events["actionMSGExp"].active = msgSlot.canActionRun();
                if (Events["actionMSGExp"].active)
                {
                    string cirActionString = msgSlot.getActionString();
                    Events["actionMSGExp"].guiName = cirActionString;
                }
                if (!msgSlot.experimentSlotFree())
                {
                    msgStatus = msgSlot.getExperiment().getAbbreviation() + ": " + msgSlot.getExperiment().stateString();
                    Events["moveMSGExp"].active = msgSlot.canExperimentMove(part.vessel);
                    if (Events["moveMSGExp"].active)
                    {
                        Events["moveMSGExp"].guiName = Localizer.Format("#ne_Move_1", msgSlot.getExperiment().getAbbreviation());
                    }
                }
            }

            // USU UI Buttons
            Fields["usuStatus"].guiActive = usuSlot.isEquipmentInstalled();
            if (Fields["usuStatus"].guiActive)
            {
                Events["actionUSUExp"].active = usuSlot.canActionRun();
                if (Events["actionUSUExp"].active)
                {
                    string usuActionString = usuSlot.getActionString();
                    Events["actionUSUExp"].guiName = usuActionString;
                }
                if (!usuSlot.experimentSlotFree())
                {
                    usuStatus = usuSlot.getExperiment().getAbbreviation() + ": " + usuSlot.getExperiment().stateString();
                    Events["moveUSUExp"].active = usuSlot.canExperimentMove(part.vessel);
                    if (Events["moveUSUExp"].active)
                    {
                        Events["moveUSUExp"].guiName = Localizer.Format("#ne_Move_1", usuSlot.getExperiment().getAbbreviation());
                    }
                }
            }
        }

        public override void OnExperimentWillRemoveFromEquipment(LabEquipment equipment)
        {
            base.OnExperimentWillRemoveFromEquipment(equipment);
            switch (equipment.LabEquipmentType)
            {
                case LabEquipmentType.USU:
                    Fields["usuStatus"].guiActive = false;
                    Events["moveUSUExp"].active = false;
                    usuStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                    break;
                case LabEquipmentType.MSG:
                    Fields["msgStatus"].guiActive = false;
                    Events["moveMSGExp"].active = false;
                    msgStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                    break;
                default:
                    throw new InvalidOperationException($"OnExperimentWilLRemoveFromEquipment() invoked with incompatible equipment {equipment.Abbreviation} for Lab {abbreviation}.");
            }
        }

        protected override bool onLabPaused()
        {
            if(! base.onLabPaused() )
            {
                return false;
            }

            /* Delete all alarms */
            msgSlot?.getExperiment()?.onPaused();
            usuSlot?.getExperiment()?.onPaused();
            return true;
        }

        protected override bool onLabStarted()
        {
            if(! base.onLabStarted() )
            {
                return false;
            }

            /* Create alarms for any running experiments */
            msgSlot?.getExperiment()?.onResumed();
            usuSlot?.getExperiment()?.onResumed();
            return true;
        }

        private string getEquipmentString()
        {
            StringBuilder sb = new StringBuilder();
            if (msgSlot.isEquipmentInstalled())
            {
                sb.Append("MSG");
            }
            if (usuSlot.isEquipmentInstalled())
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append("USU");
            }
            if (sb.Length == 0)
            {
                sb.Append(Localizer.GetStringByTag("#ne_none"));
            }
            return sb.ToString();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_MSG_Experiment", active = false)]
        public void moveMSGExp()
        {
            msgSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_MSG_Experiment", active = false)]
        public void actionMSGExp()
        {
            msgSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_USU_Experiment", active = false)]
        public void moveUSUExp()
        {
            usuSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_USU_Experiment", active = false)]
        public void actionUSUExp()
        {
            usuSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + Localizer.Format("#ne_Lab_Time_per_hour_1", LabTimePerHour);
            ret += "\n";
            ret += Localizer.GetStringByTag("#ne_You_can_install_equipment_racks_in_this_lab_to_run_experiments");
            return ret;
        }

        /// <summary>
        /// Returns the mass of installed equipment and experiments.
        /// </summary>
        /// <returns>The mass.</returns>
        protected override float getMass()
        {
            float mass = 0f;
            mass += usuSlot.getMass();
            mass += msgSlot.getMass();
            return mass;
        }
    }
}
