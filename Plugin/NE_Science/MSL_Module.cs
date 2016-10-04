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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science
{
    public class MSL_Module : Lab
    {

        private const string CIR_CONFIG_NODE_NAME = "NE_CIR_LabEquipmentSlot";
        private const string FIR_CONFIG_NODE_NAME = "NE_FIR_LabEquipmentSlot";
        private const string DPR_CONFIG_NODE_NAME = "NE_DPR_LabEquipmentSlot";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "CIR")]
        public string cirStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "FIR")]
        public string ffrStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "3PR")]
        public string prStatus = "";

        private GameObject cir;
        private GameObject fir;
        private GameObject printer;

        public Generator labTimeGenerator;

        private LabEquipmentSlot cirSlot = new LabEquipmentSlot(EquipmentRacks.CIR);
        private LabEquipmentSlot firSlot = new LabEquipmentSlot(EquipmentRacks.FIR);
        private LabEquipmentSlot printerSlot = new LabEquipmentSlot(EquipmentRacks.PRINTER);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MSL OnLoad");

            LabEquipmentSlot les = getLabEquipmentSlot(node);
            foreach (var les in )


            switch (les.getRackType())
            {
                case EquipmentRacks.CIR:
                    cirSlot = les;
                    break;
                case EquipmentRacks.FIR:
                    firSlot = les;
                    break;
            case EquipmentRacks.PRINTER:
                    printerSlot = les;
                    break;
                default:
                    NE_Helper.logError("MSL OnLoad - invalid equipment rack specified in save file.");
            }


        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MSL OnSave");
            node.AddNode(getConfigNodeForSlot(CIR_CONFIG_NODE_NAME, cirSlot));
            node.AddNode(getConfigNodeForSlot(FIR_CONFIG_NODE_NAME, firSlot));
            node.AddNode(getConfigNodeForSlot(DPR_CONFIG_NODE_NAME, printerSlot));

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
                case EquipmentRacks.CIR:
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
                case EquipmentRacks.FIR:
                    if (firSlot.isEquipmentInstalled() && firSlot.experimentSlotFree())
                    {
                        firSlot.installExperiment(exp);
                        ffrStatus = exp.getAbbreviation();
                        Fields["ffrStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + firSlot.isEquipmentInstalled() + "; free: " + firSlot.experimentSlotFree());
                    }
                    break;
                case EquipmentRacks.PRINTER:
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


        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.FIR:
                    fir.SetActive(true);
                    firSlot.install(le, this);
                    break;
                case EquipmentRacks.CIR:
                    cir.SetActive(true);
                    cirSlot.install(le, this);
                    break;
                case EquipmentRacks.PRINTER:
                    printer.SetActive(true);
                    printerSlot.install(le, this);
                    break;
            }
            part.mass += le.getMass();
        }

        private void setEquipmentActive(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.FIR:
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
                case EquipmentRacks.CIR:
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
                case EquipmentRacks.PRINTER:
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

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.isEquipmentInstalled();

                case EquipmentRacks.FIR:
                    return firSlot.isEquipmentInstalled();

                case EquipmentRacks.PRINTER:
                    return printerSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.experimentSlotFree();

                case EquipmentRacks.FIR:
                    return firSlot.experimentSlotFree();

                case EquipmentRacks.PRINTER:
                    return printerSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.isEquipmentRunning();

                case EquipmentRacks.FIR:
                    return firSlot.isEquipmentRunning();

                case EquipmentRacks.PRINTER:
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
            if (cir == null || fir == null || printer == null)
            {
                initERacksActive();
            }

            if (cirSlot.isEquipmentRunning() || firSlot.isEquipmentRunning() || printerSlot.isEquipmentRunning())
            {
                Events["stopResearch"].active = doResearch;
                Events["startResearch"].active = !doResearch;
            }
            else
            {
                if (doResearch)
                {
                    Events["stopResearch"].active = false;
                    Events["startResearch"].active = false;
                }
                else
                {
                    Events["stopResearch"].active = doResearch;
                    Events["startResearch"].active = !doResearch;
                }
            }

            if (!cirSlot.isEquipmentInstalled())
            {
                Events["installCIR"].active = checkForRackModul(EquipmentRacks.CIR);
                Fields["cirStatus"].guiActive = false;
            }
            else
            {
                Events["installCIR"].active = false;
                Events["moveCIRExp"].active = cirSlot.canExperimentMove(part.vessel);
                Fields["cirStatus"].guiActive = true;
                if (Events["moveCIRExp"].active)
                {
                    Events["moveCIRExp"].guiName = "Move " + cirSlot.getExperiment().getAbbreviation();
                }
                
                if (cirSlot.canActionRun())
                {
                    string cirActionString = cirSlot.getActionString();
                    Events["actionCIRExp"].guiName = cirActionString;
                }
                Events["actionCIRExp"].active = cirSlot.canActionRun();
                if (!cirSlot.experimentSlotFree())
                {
                    cirStatus = cirSlot.getExperiment().getAbbreviation() + ": " + cirSlot.getExperiment().getStateString(); 
                }
                else
                {
                    cirStatus = "No Experiment";
                }
            }
            if (!firSlot.isEquipmentInstalled())
            {
                Events["installFIR"].active = checkForRackModul(EquipmentRacks.FIR);
                Fields["ffrStatus"].guiActive = false;
            }
            else
            {
                Events["installFIR"].active = false;
                Events["moveFIRExp"].active = firSlot.canExperimentMove(part.vessel);
                Fields["ffrStatus"].guiActive = true;
                if (Events["moveFIRExp"].active)
                {
                    Events["moveFIRExp"].guiName = "Move " + firSlot.getExperiment().getAbbreviation();
                }
                if (firSlot.canActionRun())
                {
                    string ffrActionString = firSlot.getActionString();
                    Events["actionFIRExp"].guiName = ffrActionString;
                }
                Events["actionFIRExp"].active = firSlot.canActionRun();
                if (!firSlot.experimentSlotFree())
                {
                    ffrStatus = firSlot.getExperiment().getAbbreviation() + ": " + firSlot.getExperiment().getStateString();
                    
                }
                else
                {
                    ffrStatus = "No Experiment";
                }
            }
            if (!printerSlot.isEquipmentInstalled())
            {
                Events["installPrinter"].active = checkForRackModul(EquipmentRacks.PRINTER);
                Fields["prStatus"].guiActive = false;
            }
            else
            {
                Events["installPrinter"].active = false;
                Events["movePRExp"].active = printerSlot.canExperimentMove(part.vessel);
                Fields["prStatus"].guiActive = true;
                if (Events["movePRExp"].active)
                {
                    Events["movePRExp"].guiName = "Move " + printerSlot.getExperiment().getAbbreviation();
                }
                
                if (printerSlot.canActionRun())
                {
                    string prActionString = printerSlot.getActionString();
                    Events["actionPRExp"].guiName = prActionString;
                }
                Events["actionPRExp"].active = printerSlot.canActionRun();
                if (!printerSlot.experimentSlotFree())
                {
                    prStatus = printerSlot.getExperiment().getAbbreviation() + ": " + printerSlot.getExperiment().getStateString();    
                }
                else
                {
                    prStatus = "No Experiment";
                }
            }

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
                ret = "none";
            }
            return ret;
        }

        private bool checkForRackModul(EquipmentRacks equipmentRack)
        {
            List<EquipmentRackContainer> moduls = new List<EquipmentRackContainer>(GameObject.FindObjectsOfType(typeof(EquipmentRackContainer)) as EquipmentRackContainer[]);
            foreach (EquipmentRackContainer modul in moduls)
            {
                if (modul.vessel == this.vessel && modul.getRackType() == equipmentRack)
                {
                    return true;
                }
            }

            return false;
        }

        private EquipmentRackContainer getRackModul(EquipmentRacks equipmentRack)
        {
            List<EquipmentRackContainer> moduls = new List<EquipmentRackContainer>(GameObject.FindObjectsOfType(typeof(EquipmentRackContainer)) as EquipmentRackContainer[]);

            foreach (EquipmentRackContainer modul in moduls)
            {
                if (modul.vessel == this.vessel && modul.getRackType() == equipmentRack)
                {
                    return modul;
                }
            }

            return null;
        }

        [KSPEvent(guiActive = true, guiName = "Install CIR", active = false)]
        public void installCIR()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.CIR);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Module not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Install FIR", active = false)]
        public void installFIR()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.FIR);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Module not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Install 3D-Printer", active = false)]
        public void installPrinter()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.PRINTER);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Module not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move FIR Experiment", active = false)]
        public void moveFIRExp()
        {
            firSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action FIR Experiment", active = false)]
        public void actionFIRExp()
        {
            firSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "Move CIR Experiment", active = false)]
        public void moveCIRExp()
        {
            cirSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action CIR Experiment", active = false)]
        public void actionCIRExp()
        {
            cirSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "Move Printer Experiment", active = false)]
        public void movePRExp()
        {
            printerSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action Printer Experiment", active = false)]
        public void actionPRExp()
        {
            printerSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + "Lab Time per hour: " + LabTimePerHour;
            ret += "\nYou can install equipment racks in this lab to run experiments.";
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
