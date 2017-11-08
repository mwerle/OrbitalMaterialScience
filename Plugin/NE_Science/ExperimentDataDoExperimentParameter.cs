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
using KSP;
using Contracts;
using Contracts.Parameters;
using KSP.Localization;

namespace NE_Science.Contracts.Parameters
{
    public class ExperimentDataDoExperimentParameter : ContractParameter
    {

        private CelestialBody targetBody = null;
        private ExperimentData experiment = null;

        // Cache experiment storage aboard vessel
        private Guid vesselId = Guid.Empty;
        private int vesselPartCount = 0;
        private ExperimentStorage[] expStorage = null;

        public ExperimentDataDoExperimentParameter()
        {
            //this.Enabled = true;
            //this.DisableOnStateChange = false;
        }

        public ExperimentDataDoExperimentParameter(CelestialBody target, ExperimentData exp)
            : base()
        {
            targetBody = target;
            experiment = exp;
        }

        protected override string GetHashString()
        {
            return "do experiment " + this.GetHashCode();
        }

        protected override string GetTitle()
        {
            if (targetBody == null)
            {
                return Localizer.Format("#ne_Run_experiment_1_in_orbit", experiment.getAbbreviation());
            } else {
                return Localizer.Format("#ne_Run_experiment_1_in_orbit_around_2", experiment.getAbbreviation(), targetBody.GetDisplayName());
            }
        }

        private float lastUpdate = 0;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (lastUpdate > UnityEngine.Time.realtimeSinceStartup + 1)
                return;

            if (targetBody == null || experiment == null)
            {
                NE_Helper.log("targetBody or experimentType is null");
                return;
            }
            lastUpdate = UnityEngine.Time.realtimeSinceStartup;
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
                ExperimentStorage[] ess = getVesselExperimentStorage(vessel);
                if (ess.Length > 0)
                {
                    for (int idx = 0, count = ess.Length; idx < count; idx++)
                    {
                        var es = ess[idx];
                        ScienceData[] data = es.GetData();
                        for (int dataIdx = 0, dataCount = data.Length; dataIdx < dataCount; dataIdx++)
                        {
                            var datum = data[dataIdx];
                            if (datum.subjectID.ToLower().Contains(experiment.getId().ToLower()+"@" + targetBody.name.ToLower() + "inspace"))
                            {
                                SetComplete();
                                return;
                            }
                        }
                    }
                }
            }

            /* MKW - Commenting this out and leaving it here because there's partially unimplemented logic which may
            ** need to be added later; not sure what Nehemia was trying to do there (see below).
                foreach (Part part in vessel.Parts)
                {
                    ExperimentStorage[] ess = part.GetComponents<ExperimentStorage>();
                    if (ess.Length > 0)
                    {
                        foreach (ExperimentStorage es in ess)
                        {
                            ScienceData[] data = es.GetData();
                            foreach (ScienceData datum in data)
                            {
                                if (datum.subjectID.ToLower().Contains(experiment.getId().ToLower()+"@" + targetBody.name.ToLower() + "inspace"))
                                {
                                    SetComplete();
                                    return;
                                }
                            }
                        }
                        //
                        // MKW - What was the code below supposed to achieve? As it stands it does nothing..
                        // 
                        OMSExperiment e = part.FindModuleImplementing<OMSExperiment>();
                        if (e != null)
                        {
                            if (e.completed >= this.Root.DateAccepted)
                            {
                                
                            }
                        }
                    }
                }
            */
            SetIncomplete();
        }

        private ExperimentStorage[] getVesselExperimentStorage(Vessel vessel)
        {
            var parts = vessel.parts;
            if (expStorage == null || vessel.id != vesselId || parts.Count != vesselPartCount)
            {
                // Create/Refresh cache
                List<ExperimentStorage> l = new List<ExperimentStorage>();
                for (int idx = 0, count = parts.Count; idx < count; idx++)
                {
                    ExperimentStorage[] ess = vessel.parts[idx].GetComponents<ExperimentStorage>();
                    l.AddRange(ess);
                }
                expStorage = l.ToArray();
                vesselId = vessel.id;
                vesselPartCount = parts.Count;
            }
            return expStorage;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = NE_Helper.GetValueAsInt(node, KEESExperimentContract.TARGET_BODY);
            for (int idx = 0, count = FlightGlobals.Bodies.Count; idx < count; idx++)
            {
                var body = FlightGlobals.Bodies[idx];
                if (body.flightGlobalsIndex == bodyID)
                {
                    targetBody = body;
                }
            }
            experiment = ExperimentData.getExperimentDataFromNode(node.GetNode(ExperimentData.CONFIG_NODE_NAME));
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(KeminiExperimentContract.TARGET_BODY, bodyID);
            node.AddNode(experiment.getNode());
        }
    }
}
