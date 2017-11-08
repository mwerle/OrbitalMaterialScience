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
using UnityEngine;
using KSP;
using Contracts;
using Contracts.Parameters;
using KSP.Localization;

namespace NE_Science.Contracts.Parameters
{
    public class KEESReturnExperimentParameter : ContractParameter
    {        
        private CelestialBody targetBody = null;
        private AvailablePart experiment = null;

        

        public KEESReturnExperimentParameter()
        {
        }

        public KEESReturnExperimentParameter(CelestialBody target, AvailablePart exp)
            : base()
        {
            targetBody = target;
            experiment = exp;
        }

        protected override string GetHashString()
        {
            return "return and recover experiment " + this.GetHashCode();
        }
        protected override string GetTitle()
        {
            return Localizer.Format("#ne_Return_and_recover_experiment_at_1", Planetarium.fetch.Home.GetDisplayName());
        }

        protected override void OnRegister()
        {
            NE_Helper.log("On Register");
            GameEvents.onVesselRecovered.Add(OnRecovered);
            if (StageRecoveryWrapper.StageRecoveryAvailable)
            {
                StageRecoveryWrapper.AddRecoverySuccessEvent(StageRecoverySuccessEvent);
            }
        }

        protected override void OnUnregister()
        {
            GameEvents.onVesselRecovered.Remove(OnRecovered);
            if (StageRecoveryWrapper.StageRecoveryAvailable)
            {
                StageRecoveryWrapper.RemoveRecoverySuccessEvent(StageRecoverySuccessEvent);
            }
        }

        private void StageRecoverySuccessEvent(Vessel v, float[] infoArray, string reason)
        {
            if (reason == "SUCCESS")
            {
                // MKW TODO: Check if we have quick recovery
                OnRecovered(v.protoVessel, false);
            }
        }

        private void OnRecovered(ProtoVessel pv, bool quick)
        {
            NE_Helper.log("Recovery ProtoVessel");
            if (targetBody != null && experiment != null)
            {
                NE_Helper.log("Looking up strategy for " + experiment.name);
                OMSExperimentRecovery strategy = new KEESExperimentRecovery();
                if(strategy.protovesselHasDoneExperiment(pv, experiment,targetBody, this.Root.DateAccepted)){
                    SetComplete();
                }
            }
            
        }

        private bool setTargetExperiment(string exp)
        {
            experiment = PartLoader.getPartInfoByName(exp);
            if (experiment == null)
            {
                NE_Helper.logError("Couldn't find experiment part: " + exp);
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
