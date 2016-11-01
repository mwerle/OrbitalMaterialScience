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
    public class MPL_Module : Lab
    {

        private const string MSG_CONFIG_NODE_NAME = "NE_MSG_LabEquipmentSlot";
        private const string USU_CONFIG_NODE_NAME = "NE_USU_LabEquipmentSlot";

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

        private LabEquipmentSlot msgSlot = new LabEquipmentSlot(EquipmentRacks.MSG);
        private LabEquipmentSlot usuSlot = new LabEquipmentSlot(EquipmentRacks.USU);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MPL OnLoad");
            msgSlot = getLabEquipmentSlot(node.GetNode(MSG_CONFIG_NODE_NAME));
            usuSlot = getLabEquipmentSlot(node.GetNode(USU_CONFIG_NODE_NAME));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MPL OnSave");
            node.AddNode(getConfigNodeForSlot(MSG_CONFIG_NODE_NAME, msgSlot));
            node.AddNode(getConfigNodeForSlot(USU_CONFIG_NODE_NAME, usuSlot));

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

        static bool isFirstTime = true;
        private void initERacksActive()
        {
            if (part == null)
            {
                NE_Helper.log("init E Racks - part is null!");
                return;
            }

            Part foo = part.FindChildPart("MPL_IVA(Clone)", true);
            if (foo != null)
            {
                NE_Helper.log("init E Racks - found MPL_IVA!");
                NE_Helper.log("init E Racks - MeshFilter: " + foo.gameObject?.GetComponent<MeshFilter>()?.name);
            }
            else
            {
                NE_Helper.log("init E Racks - MPL_IVA not found!");
            }
            if (part.internalModel != null)
            {

                GameObject labIVA = part.internalModel.gameObject?.transform?.GetChild(0)?.GetChild(0)?.gameObject;
                if (isFirstTime && NE_Helper.debugging())
                {
                    part.internalModel?.gameObject.PrintComponents(5);
                }
                if (labIVA == null)
                {
                    NE_Helper.log("init E Racks - labIVA object not found");
                    return;
                }
                if (labIVA.GetComponent<MeshFilter>() == null)
                {
                    NE_Helper.log("init E Racks - MeshFilter not found");
                    if (isFirstTime)
                    {
                        labIVA.PrintComponents(2);
                        isFirstTime = false;
                    }
                    if (labIVA.transform.GetComponent<MeshFilter>() != null)
                    {
                        NE_Helper.log("init E Racks - but labIVA.transform has MeshFilter: {0}", labIVA.transform.GetComponent<MeshFilter>().name);
                    }
                    if (labIVA.transform.GetChild(0).GetComponent<MeshFilter>() != null)
                    {
                        NE_Helper.log("init E Racks - but labIVA.GetChild(0) has MeshFilter: {0}", labIVA.transform.GetChild(0).GetComponent<MeshFilter>().name);
                    }
                }
                else
                {
                    NE_Helper.log("init E Racks - MeshFilter '" + labIVA.GetComponent<MeshFilter>().name + "'found");
                }
                // MKW DEBUG
                labIVA = labIVA.transform.GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>()?.name == "MPL_IVA")
                {
                    msg = labIVA.transform.FindChild("MSG")?.gameObject;
                    if (msg == null)
                    {
                        NE_Helper.log("init E Racks - MSG object not found");
                        return;
                    }

                    cfe = msg.transform.FindChild("CFE")?.gameObject;
                    if (msg == null)
                    {
                        NE_Helper.log("init E Racks - CFE object not found");
                        return;
                    }
                    usu = labIVA.transform.FindChild("UltraSound")?.gameObject;
                    if (msg == null)
                    {
                        NE_Helper.log("init E Racks - UltraSound object not found");
                        return;
                    }

                    cfe.SetActive(!msgSlot.experimentSlotFree());
                    msg.SetActive(msgSlot.isEquipmentInstalled());
                    usu.SetActive(usuSlot.isEquipmentInstalled());

                    NE_Helper.log("init E Racks successfull");
                }
                else
                {
                    NE_Helper.logError("init E Racks - MPL mesh not found");
                }

            }
            else
            {
                NE_Helper.log("init E Racks - internal model null");
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
                case EquipmentRacks.MSG:
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
                case EquipmentRacks.USU:
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


        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.MSG:
                    msg.SetActive(true);
                    msgSlot.install(le, this);
                    cfe.SetActive(false);
                    break;
                case EquipmentRacks.USU:
                    usu.SetActive(true);
                    usuSlot.install(le, this);
                    break;
            }

            part.mass += le.getMass();
        }

        private void setEquipmentActive(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
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
                case EquipmentRacks.USU:
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

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.isEquipmentInstalled();
                case EquipmentRacks.USU:
                    return usuSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.experimentSlotFree();
                case EquipmentRacks.USU:
                    return usuSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.isEquipmentRunning();
                case EquipmentRacks.USU:
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

            if (msg == null)
            {
                initERacksActive();
            }

            if (!msgSlot.isEquipmentInstalled())
            {
                Events["installMSG"].active = checkForRackModul(EquipmentRacks.MSG);
                Fields["msgStatus"].guiActive = false;
            }
            else
            {
                Events["installMSG"].active = false;
                Events["moveMSGExp"].active = msgSlot.canExperimentMove(part.vessel);
                Fields["msgStatus"].guiActive = true;
                if (Events["moveMSGExp"].active)
                {
                    Events["moveMSGExp"].guiName = "Move " + msgSlot.getExperiment().getAbbreviation();
                }

                if (msgSlot.canActionRun())
                {
                    string cirActionString = msgSlot.getActionString();
                    Events["actionMSGExp"].guiName = cirActionString;
                }
                Events["actionMSGExp"].active = msgSlot.canActionRun();
                if (!msgSlot.experimentSlotFree())
                {
                    msgStatus = msgSlot.getExperiment().getAbbreviation() + ": " + msgSlot.getExperiment().getStateString();
                }
                else
                {
                    msgStatus = "No Experiment";
                }
            }

            if (!usuSlot.isEquipmentInstalled())
            {
                Events["installUSU"].active = checkForRackModul(EquipmentRacks.USU);
                Fields["usuStatus"].guiActive = false;
            }
            else
            {
                Events["installUSU"].active = false;
                Events["moveUSUExp"].active = usuSlot.canExperimentMove(part.vessel);
                Fields["usuStatus"].guiActive = true;
                if (Events["moveUSUExp"].active)
                {
                    Events["moveUSUExp"].guiName = "Move " + usuSlot.getExperiment().getAbbreviation();
                }

                if (usuSlot.canActionRun())
                {
                    string usuActionString = usuSlot.getActionString();
                    Events["actionUSUExp"].guiName = usuActionString;
                }
                Events["actionUSUExp"].active = usuSlot.canActionRun();
                if (!usuSlot.experimentSlotFree())
                {
                    usuStatus = usuSlot.getExperiment().getAbbreviation() + ": " + usuSlot.getExperiment().getStateString();
                }
                else
                {
                    usuStatus = "No Experiment";
                }
            }

        }

        private string getEquipmentString()
        {
            string ret = "";
            if (msgSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "MSG";
            }
            if (usuSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "USU";
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

        [KSPEvent(guiActive = true, guiName = "Install MSG", active = false)]
        public void installMSG()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.MSG);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Module not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move MSG Experiment", active = false)]
        public void moveMSGExp()
        {
            msgSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action MSG Experiment", active = false)]
        public void actionMSGExp()
        {
            msgSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "Install USU", active = false)]
        public void installUSU()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.USU);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Module not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move USU Experiment", active = false)]
        public void moveUSUExp()
        {
            usuSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action USU Experiment", active = false)]
        public void actionUSUExp()
        {
            usuSlot.experimentAction();
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
            mass += usuSlot.getMass();
            mass += msgSlot.getMass();
            return mass;
        }
    }
}
