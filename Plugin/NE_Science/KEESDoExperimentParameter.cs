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
    public class KEESDoExperimentParameter : ContractParameter
    {

        private CelestialBody targetBody = null;
        private AvailablePart experiment = null;

        public KEESDoExperimentParameter()
        {
            //this.Enabled = true;
            //this.DisableOnStateChange = false;
        }

        public KEESDoExperimentParameter(CelestialBody target, AvailablePart exp)
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
                return Localizer.Format("#ne_Run_experiment_in_orbit");
            } else {
                return Localizer.Format("#ne_Run_experiment_in_orbit_around_1", targetBody.GetDisplayName());
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
            // MKW TODO: cache all the vessel parts which are KEES experiments to avoid iterating over the entire vessel every update
            if (vessel != null)
                for (int idx = 0, count = vessel.Parts.Count; idx < count; idx++)
                {
                    var part = vessel.Parts[idx];
                    if (part.name == experiment.name)
                    {
                        OMSExperiment e = part.FindModuleImplementing<OMSExperiment>();
                        if (e != null)
                        {
                            if (e.completed >= this.Root.DateAccepted)
                            {
                                ScienceData[] data = e.GetData();
                                for (int dIdx = 0, dCount = data.Length; dIdx < dCount; dIdx++)
                                {
                                    if (data[dIdx].subjectID.ToLower().Contains("@" + targetBody.name.ToLower() + "inspace"))
                                    {
                                        SetComplete();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            SetIncomplete();
        }

        private bool setTargetExperiment(string exp)
        {
            experiment = PartLoader.getPartInfoByName(exp);
            if (experiment == null)
            {
                Debug.LogError("Couldn't find experiment part: " + exp);
                return false;
            }
            return true;
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
            setTargetExperiment(node.GetValue(KEESExperimentContract.EXPERIMENT_STRING));
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(KEESExperimentContract.TARGET_BODY, bodyID);
            node.AddValue(KEESExperimentContract.EXPERIMENT_STRING, experiment.name);
        }
    }
}
