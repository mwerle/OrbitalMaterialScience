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
using System.Text;
using KSP;

namespace NE_Science.Contracts.Parameters
{
    /*
     * Strategy for finding the experiment in a recoverd vessel. 
     */
    public class OMSExperimentRecovery
    {
        /** Returns the experiment module name given a part name.
         * Checks the KEES and OMS/KLS experiment registers for a matching experiment part. */
        protected string getExperimentModuleName(string experimentPartName)
        {
            /* First check KEES registry */
            var keesExperiments = KEESExperimentRegister.getExperimentPartNames();
            if(keesExperiments.Contains(experimentPartName))
            {
                return KEESExperimentRegister.getExperimentModuleName();
            }
            /* TODO: check OMS/KLS registry */
            return null;
        }

        protected const string SCIENCE_DATA = "ScienceData";
        protected const string SUBJECT_ID = "subjectID";

        public virtual bool protovesselHasDoneExperiment(ProtoVessel pv, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            for (int i = 0, count = pv.protoPartSnapshots.Count; i < count; i++)
            {
                var part = pv.protoPartSnapshots[i];
                if (part.partName == experiment.name)
                {
                    return experimentFound(part, experiment, targetBody, contractAccepted);
                }
            }
            return false;
        }

        protected bool experimentFound(ProtoPartSnapshot part, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            NE_Helper.log("ProtoVessel recovery: Experiment found");
            string moduleName = getExperimentModuleName(experiment.name);
            for (int i = 0, count = part.modules.Count; i < count; i++)
            {
                var module = part.modules[i];
                NE_Helper.log("ProtoVessel recovery Modulename: " + module.moduleName);
                if (module.moduleName == moduleName)
                {
                    ConfigNode partConf = module.moduleValues;
                    float completed = NE_Helper.GetValueAsFloat(partConf, OMSExperiment.COMPLETED);
                    if (completed >= contractAccepted)
                    {
                        return containsDoneExperimentData(partConf, targetBody);
                    }
                }
            }
            return false;
        }

        protected bool containsDoneExperimentData(ConfigNode partConf, CelestialBody targetBody)
        {
            var nodes = partConf.GetNodes(SCIENCE_DATA);
            for (int idx = 0, count = nodes.Length; idx < count; idx++)
            {
                var scienceData = nodes[idx];
                if (!scienceData.HasValue(SUBJECT_ID))
                    continue;
                string subjectID = scienceData.GetValue(SUBJECT_ID);
                NE_Helper.log("Science on Board SubjectID: " + subjectID);
                if (subjectID.ToLower().Contains("@" + targetBody.name.ToLower() + "inspace"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /*
     * Recovery strategy for KEES experiment. This experiments can be stored in a payload carrier.
     */
    public class KEESExperimentRecovery : OMSExperimentRecovery
    {
        protected const string KIS_CONTAINER = "ModuleKISInventory";

        public override bool protovesselHasDoneExperiment(ProtoVessel pv, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            NE_Helper.log("KEES-Experiement stategy");
            for (int i = 0, count = pv.protoPartSnapshots.Count; i < count; i++)
            {
                var part = pv.protoPartSnapshots[i];
                NE_Helper.log("KEES-Experiement stategy, Part: " + part.partName);
                if (part.partName == experiment.name) {
                    if (experimentFound (part, experiment, targetBody, contractAccepted))
                        return true;
                } else if (payloadCarrierFound (part, experiment, targetBody, contractAccepted)) {
                    return true;
                }
            }
            return false;
        }

        private bool payloadCarrierFound(ProtoPartSnapshot payloadCarrier, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            NE_Helper.log("ProtoVessel recovery: payload carrier found");
            for (int i = 0, count = payloadCarrier.modules.Count; i < count; i++)
            {
                var module = payloadCarrier.modules[i];
                NE_Helper.log("ProtoVessel recovery Modulename: " + module.moduleName);
                if (module.moduleName == KIS_CONTAINER)
                {
                    NE_Helper.log("KIS container found");
                    ConfigNode partConf = findExperimentModulInPC(module, experiment);
                    if (partConf != null)
                    {
                        NE_Helper.log("Experiment module found");
                        float completed = NE_Helper.GetValueAsFloat(partConf, OMSExperiment.COMPLETED);
                        if (completed >= contractAccepted)
                        {
                            return containsDoneExperimentData(partConf, targetBody);
                        }
                    }
                }
            }
            return false;
        }

        private ConfigNode findExperimentModulInPC(ProtoPartModuleSnapshot kisModule, AvailablePart experiment)
        {
            ConfigNode partConf = kisModule.moduleValues;
            var itemNodes = partConf.GetNodes("ITEM");
            for (int itemIdx = 0, itemCount = itemNodes.Length; itemIdx < itemCount; itemIdx++)
            {
                var item = itemNodes[itemIdx];
                NE_Helper.log("ConfigNode ITEM: " + item.GetValue("partName"));
                if (itemNodes[itemIdx].GetValue("partName") != experiment.name)
                {
                    continue;
                }

                var partNodes = item.GetNodes("PART");
                for (int partIdx = 0, partCount = partNodes.Length; partIdx < partCount; partIdx++)
                {
                    var part = partNodes[partIdx];
                    NE_Helper.log("ConfigNode PART: " + part.GetValue("name"));
                    if (part.GetValue("name") != experiment.name)
                    {
                        continue;
                    }

                    var moduleNodes = part.GetNodes("MODULE");
                    var experimentModuleName = getExperimentModuleName(experiment.name);
                    for (int moduleIdx = 0, moduleCount = moduleNodes.Length; moduleIdx < moduleCount; moduleIdx++)
                    {
                        var module = moduleNodes[moduleIdx];
                        // TODO: MKW - if experiment is a custom-defined one, this line will throw an exception!
                        // experiment.name will not be a valid index into the experimentModulname array.
                        if (module.GetValue("name") == experimentModuleName)
                        {
                            return module;
                        }
                    }
                }
            }
            return null;
        }

        /*
         * Following is partial layout of KIS container containing KEES experiments: 
            PART
            {
                name = NE.KEES.PC
                cid = 4293836508
                uid = 2650723247
...
                MODULE
                {
                    name = ModuleKISInventory
                    isEnabled = True
                    invName = 
...
                    ITEM
                    {
                        partName = NE.KEES.PPMD
                        slot = 3
                        quantity = 1
                        equipped = False
                        resourceMass = 0
                        contentMass = 0
                        contentCost = 0
                        PART
                        {
                            name = NE.KEES.PPMD
                            cid = 4294922954
...
                            MODULE
                            {
                                name = KEES_Lab
                                isEnabled = True
                                doResearch = True
...
                            MODULE
                            {
                                name = KEESExperiment
                                isEnabled = True
...
                                ScienceData
                                {
                                    data = 280
                                    subjectID = NE_KEES_PPMD@KerbinInSpaceLow
                                    xmit = 0.2
                                    labBoost = 1
                                    title = Polished Plate Micrometeoroid and Debris while in space near Kerbin
                                }
...
                    ITEM
                    {
                        partName = NE.KEES.POSA2
                        slot = 0
                        quantity = 1
*/
    }
}
