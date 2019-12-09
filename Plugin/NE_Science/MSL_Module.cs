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
    public class MSL_Module : Lab
    {

        private const string CIR_LAB_EQUIPMENT_TYPE = "CIR";
        private const string FIR_LAB_EQUIPMENT_TYPE = "FIR";
        private const string PRINTER_LAB_EQUIPMENT_TYPE = "PRINTER";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "CIR")]
        public string cirStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "FIR")]
        public string firStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "3PR")]
        public string prStatus = "";

        private GameObject cir;
        private GameObject fir;
        private GameObject printer;

        public Generator labTimeGenerator;

        private LabEquipmentSlot cirSlot = new LabEquipmentSlot(LabEquipmentType.CIR);
        private LabEquipmentSlot firSlot = new LabEquipmentSlot(LabEquipmentType.FIR);
        private LabEquipmentSlot printerSlot = new LabEquipmentSlot(LabEquipmentType.PRINTER);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MSL OnLoad");
            cirSlot = getLabEquipmentSlotByType(node, CIR_LAB_EQUIPMENT_TYPE);
            firSlot = getLabEquipmentSlotByType(node, FIR_LAB_EQUIPMENT_TYPE);
            printerSlot = getLabEquipmentSlotByType(node, PRINTER_LAB_EQUIPMENT_TYPE);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MSL OnSave");
            node.AddNode(cirSlot.getConfigNode());
            node.AddNode(firSlot.getConfigNode());
            node.AddNode(printerSlot.getConfigNode());

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
            cirSlot.onStart(this);
            firSlot.onStart(this);
            printerSlot.onStart(this);

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
                if (labIVA.GetComponent<MeshFilter>().name == "Lab1IVA")
                {
                    printer = labIVA.transform.GetChild(0).gameObject;
                    cir = labIVA.transform.GetChild(1).gameObject;
                    fir = labIVA.transform.GetChild(2).gameObject;

                    if (firSlot.isEquipmentInstalled())
                    {
                        fir.SetActive(true);
                    }
                    else
                    {
                        fir.SetActive(false);
                    }

                    if (cirSlot.isEquipmentInstalled())
                    {
                        cir.SetActive(true);
                    }
                    else
                    {
                        cir.SetActive(false);
                    }

                    if (printerSlot.isEquipmentInstalled())
                    {
                        printer.SetActive(true);
                    }
                    else
                    {
                        printer.SetActive(false);
                    }
                }
                NE_Helper.log("init E Racks successfull");
            }
            else {
                NE_Helper.log("init E Racks internal model null");
            }
        }

        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case LabEquipmentType.CIR:
                    if (cirSlot.isEquipmentInstalled() && cirSlot.experimentSlotFree())
                    {
                        cirSlot.installExperiment(exp);
                        cirStatus = exp.getAbbreviation();
                        Fields["cirStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + cirSlot.isEquipmentInstalled() + "; free: " + cirSlot.experimentSlotFree());
                    }
                    break;
                case LabEquipmentType.FIR:
                    if (firSlot.isEquipmentInstalled() && firSlot.experimentSlotFree())
                    {
                        firSlot.installExperiment(exp);
                        firStatus = exp.getAbbreviation();
                        Fields["firStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + firSlot.isEquipmentInstalled() + "; free: " + firSlot.experimentSlotFree());
                    }
                    break;
                case LabEquipmentType.PRINTER:
                    if (printerSlot.isEquipmentInstalled() && printerSlot.experimentSlotFree())
                    {
                        printerSlot.installExperiment(exp);
                        prStatus = exp.getAbbreviation();
                        Fields["prStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + printerSlot.isEquipmentInstalled() + "; free: " + printerSlot.experimentSlotFree());
                    }
                    break;
            }
        }

        public override void OnExperimentWillRemoveFromEquipment(LabEquipment equipment)
        {
            base.OnExperimentWillRemoveFromEquipment(equipment);
            switch (equipment.LabEquipmentType)
            {
                case LabEquipmentType.CIR:
                    Fields["cirStatus"].guiActive = false;
                    cirStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                    break;

                case LabEquipmentType.FIR:
                    Fields["firStatus"].guiActive = false;
                    firStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                    break;

                case LabEquipmentType.PRINTER:
                    Fields["prStatus"].guiActive = false;
                    prStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                    break;

                default:
                    throw new InvalidOperationException($"OnExperimentWilLRemoveFromEquipment() invoked with incompatible equipment {equipment.Abbreviation} for Lab {abbreviation}.");
            }
        }

        #region Equipment Management
        private void setEquipmentActive(LabEquipmentType rack)
        {
            switch (rack)
            {
                case LabEquipmentType.FIR:
                    if (fir != null)
                    {
                        fir.SetActive(firSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if(fir != null)fir.SetActive(firSlot.isEquipmentInstalled());
                    }
                    break;
                case LabEquipmentType.CIR:
                    if (cir != null)
                    {
                        cir.SetActive(cirSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (cir != null) cir.SetActive(cirSlot.isEquipmentInstalled());
                    }
                    break;
                case LabEquipmentType.PRINTER:
                    if (printer != null)
                    {
                        printer.SetActive(printerSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (printer != null) printer.SetActive(printerSlot.isEquipmentInstalled());
                    }
                    break;
            }
        }

        public override bool hasEquipmentInstalled(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.CIR:
                    return cirSlot.isEquipmentInstalled();

                case LabEquipmentType.FIR:
                    return firSlot.isEquipmentInstalled();

                case LabEquipmentType.PRINTER:
                    return printerSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public override bool hasEquipmentSlot(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.CIR:
                    // fall through 
                case LabEquipmentType.FIR:
                    // fall through 
                case LabEquipmentType.PRINTER:
                    return true;

                default:
                    return false;
            }
        }

        public override bool hasFreeEquipmentSlot(LabEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case LabEquipmentType.CIR:
                    return !cirSlot.isEquipmentInstalled();

                case LabEquipmentType.FIR:
                    return !firSlot.isEquipmentInstalled();

                case LabEquipmentType.PRINTER:
                    return !printerSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public override void installLabEquipment(LabEquipment le)
        {
            switch (le.LabEquipmentType)
            {
                case LabEquipmentType.FIR:
                    if (!tryInstallEquipment(le, firSlot, fir))
                    {
                        NE_Helper.logError("installLabEquipment failed: FIR slot already full");
                    }
                    break;
                case LabEquipmentType.CIR:
                    if (!tryInstallEquipment(le, cirSlot, cir))
                    {
                        NE_Helper.logError("installLabEquipment failed: CIR slot already full");
                    }
                    break;
                case LabEquipmentType.PRINTER:
                    if (!tryInstallEquipment(le, printerSlot, printer))
                    {
                        NE_Helper.logError("installLabEquipment failed: PRINTER slot already full");
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
                case LabEquipmentType.CIR:
                    return cirSlot.experimentSlotFree();

                case LabEquipmentType.FIR:
                    return firSlot.experimentSlotFree();

                case LabEquipmentType.PRINTER:
                    return printerSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(LabEquipmentType rack)
        {
            switch (rack)
            {
                case LabEquipmentType.CIR:
                    return cirSlot.isEquipmentRunning();

                case LabEquipmentType.FIR:
                    return firSlot.isEquipmentRunning();

                case LabEquipmentType.PRINTER:
                    return printerSlot.isEquipmentRunning();

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
                NE_Helper.logError("MSL_Module.displayStatusMessage(): caught exception " + e +"\n" + e.StackTrace);
            }
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;
            #if false
            // This should never happen; let's wait for a nullref during testing
            if (cir == null || fir == null || printer == null)
            {
                initERacksActive();
            }
            #endif
            cirSlot.updateCheck();
            firSlot.updateCheck();
            printerSlot.updateCheck();

            if (cirSlot.isEquipmentRunning() || firSlot.isEquipmentRunning() || printerSlot.isEquipmentRunning())
            {
                Events["labAction"].guiName = doResearch? "#ne_Pause_Research" : "#ne_Resume_Research";
                Events["labAction"].active = true;
            }
            else
            {
                Events["labAction"].active = false;
            }

            // CIR UI buttons
            Fields["cirStatus"].guiActive = cirSlot.isEquipmentInstalled();
            if (Fields["cirStatus"].guiActive)
            {
                Events["actionCIRExp"].active = cirSlot.canActionRun();
                if (Events["actionCIRExp"].active)
                {
                    Events["actionCIRExp"].guiName = cirSlot.getActionString();
                }
                
                if (!cirSlot.experimentSlotFree())
                {
                    cirStatus = cirSlot.getExperiment().getAbbreviation() + ": " + cirSlot.getExperiment().stateString();
                    Events["moveCIRExp"].active = cirSlot.canExperimentMove(part.vessel);
                    if (Events["moveCIRExp"].active)
                    {
                        Events["moveCIRExp"].guiName = Localizer.Format("#ne_Move_1", cirSlot.getExperiment().getAbbreviation());
                    }
                }
            }

            // FIR UI buttons
            Fields["firStatus"].guiActive = firSlot.isEquipmentInstalled();
            if (Fields["firStatus"].guiActive)
            {
                Events["actionFIRExp"].active = firSlot.canActionRun();
                if (Events["actionFIRExp"].active)
                {
                    Events["actionFIRExp"].guiName = firSlot.getActionString();
                }
                if (!firSlot.experimentSlotFree())
                {
                    firStatus = firSlot.getExperiment().getAbbreviation() + ": " + firSlot.getExperiment().stateString();
                    Events["moveFIRExp"].active = firSlot.canExperimentMove(part.vessel);
                    if (Events["moveFIRExp"].active)
                    {
                        Events["moveFIRExp"].guiName = Localizer.Format("#ne_Move_1", firSlot.getExperiment().getAbbreviation());
                    }
                }
            }

            // Printer UI buttons
            Fields["prStatus"].guiActive = printerSlot.isEquipmentInstalled();
            if (Fields["prStatus"].guiActive)
            {
                Events["actionPRExp"].active = printerSlot.canActionRun();
                if (Events["actionPRExp"].active)
                {
                    string prActionString = printerSlot.getActionString();
                    Events["actionPRExp"].guiName = prActionString;
                }
                if (!printerSlot.experimentSlotFree())
                {
                    prStatus = printerSlot.getExperiment().getAbbreviation() + ": " + printerSlot.getExperiment().stateString();
                    Events["movePRExp"].active = printerSlot.canExperimentMove(part.vessel);
                    if (Events["movePRExp"].active)
                    {
                        Events["movePRExp"].guiName = Localizer.Format("#ne_Move_1", printerSlot.getExperiment().getAbbreviation());
                    }
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
            cirSlot?.getExperiment()?.onPaused();
            firSlot?.getExperiment()?.onPaused();
            printerSlot?.getExperiment()?.onPaused();
            return true;
        }

        protected override bool onLabStarted()
        {
            if(! base.onLabStarted() )
            {
                return false;
            }

            /* Create alarms for any running experiments */
            cirSlot?.getExperiment()?.onResumed();
            firSlot?.getExperiment()?.onResumed();
            printerSlot?.getExperiment()?.onResumed();
            return true;
        }

        private string getEquipmentString()
        {
            string ret = "";
            if (firSlot.isEquipmentInstalled())
            {
                ret += "FIR";
            }
            if (cirSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "CIR";
            }
            if (printerSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "3PR";
            }
            if (ret.Length == 0)
            {
                ret = Localizer.GetStringByTag("#ne_none");
            }
            return ret;
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_FIR_Experiment", active = false)]
        public void moveFIRExp()
        {
            firSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_FIR_Experiment", active = false)]
        public void actionFIRExp()
        {
            firSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_CIR_Experiment", active = false)]
        public void moveCIRExp()
        {
            cirSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_CIR_Experiment", active = false)]
        public void actionCIRExp()
        {
            cirSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_3DP_Experiment", active = false)]
        public void movePRExp()
        {
            printerSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_3DP_Experiment", active = false)]
        public void actionPRExp()
        {
            printerSlot.experimentAction();
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
            mass += cirSlot.getMass();
            mass += firSlot.getMass();
            mass += printerSlot.getMass();
            return mass;
        }
    }
}
