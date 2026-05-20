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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using Contracts.Agents;
using KSP;
using KSPAchievements;
using KSP.Localization;

namespace NE_Science.Contracts
{
    public class KeminiExperimentContract : Contract
    {

        public const string TARGET_BODY = "targetBody";
        public const string EXPERIMENT_STRING = "experiment";

        CelestialBody targetBody = null;
        ExperimentData experiment = null;

        /** Callback used to generate a contract at a particular prestige level.
         *  this.Prestige contains the required prestige level of contract to generate.
         *  Add any other checks and guards in here to prevent too many contracts being generated.
         *  Return TRUE if a contract could be created, FALSE if not. */
        protected override bool Generate()
        {
            if (Prestige != ContractPrestige.Trivial) {
                return false;
            }

            if (ExperimentFactory.getAvailableExperiments(ExperimentFactory.KEMINI_EXPERIMENTS, true).Count == 0) {
                return false;
            }

            NE_Helper.log("Generate Contract");
            if (activeContracts() >= getMaxContracts())
            {
                NE_Helper.log("Generate Contract: Max Contracts reached: " + getMaxContracts());
                return false;
            }
            targetBody = getTargetBody();
            // Assert: targetBody != null
            NE_Helper.log("Generate Contract: Body: " + targetBody.name);

            if (!setTargetExperiment(getTargetExperiment())) return false;

            NE_Helper.log("Generate Contract: Add Parameter");
            AddParameter(new Parameters.ExperimentDataDoExperimentParameter(targetBody, experiment));
            AddParameter(new Parameters.ExperimentDataReturnExperimentParameter(targetBody, experiment));

            NE_Helper.log("Generate Contract: set Values ");
            base.SetExpiry();
            base.SetScience(5f, targetBody);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(5f, 4f, targetBody);
            base.SetFunds(10000f, 20000f, 10000f, targetBody);
            agent = AgentList.Instance.GetAgent("Nehemiah Engineering");
            NE_Helper.log("Generate Contract: done Exp: " + experiment.getAbbreviation() + " Body: " + targetBody.name + " funds Adv: " + this.FundsAdvance);
            return true;
        }

        private int getMaxContracts()
        {
            return 2;
        }

        /** Returns a count of offered and active Kemini Contracts.
         *  NB: The original version had optional filters per type and body, but those were never used. */
        private int activeContracts()
        {
            int ret = 0;
            if (ContractSystem.Instance == null)
            {
                return 0;
            }
            if (ContractSystem.Instance.Contracts == null)
            {
                return 0;
            }
            var contracts = ContractSystem.Instance.GetCurrentContracts<KeminiExperimentContract>();
            for (int idx = 0, count = contracts.Length; idx < count; idx++)
            {
                var kcon = contracts[idx];
                if (kcon.ContractState == Contract.State.Active || kcon.ContractState == Contract.State.Offered)
                {
                    ret++;
                }
            }
            return ret;
        }

        private int activeAndDoneContracts(ExperimentData expData = null, CelestialBody body = null)
        {
            int ret = 0;
            if (ContractSystem.Instance == null)
            {
                return 0;
            }
            if (ContractSystem.Instance.Contracts == null)
            {
                return 0;
            }
            var contracts = ContractSystem.Instance.Contracts;
            for (int idx = 0, count = contracts.Count; idx < count; idx++)
            {
                var con = contracts[idx];
                KeminiExperimentContract keminiCon = con as KeminiExperimentContract;
                if (keminiCon != null && (keminiCon.ContractState == Contract.State.Active ||
                    keminiCon.ContractState == Contract.State.Offered ||
                    keminiCon.ContractState == Contract.State.Completed) &&
                  (expData == null || keminiCon.experiment != null) &&
                  (body == null || keminiCon.targetBody != null) &&
                  ((expData == null || expData.getId() == keminiCon.experiment.getId()) &&
                   (body == null || body.bodyName == keminiCon.targetBody.bodyName)))
                    ret += 1;
            }
            return ret;
        }

        private ExperimentData getTargetExperiment()
        {
            List<ExperimentData> unlockedExperiments = ExperimentFactory.getAvailableExperiments(ExperimentFactory.KEMINI_EXPERIMENTS, true);
            List<ExperimentData> unlockedNoContract = new List<ExperimentData>();

            for (int idx = 0, count = unlockedExperiments.Count; idx < count; idx++)
            {
                var exp = unlockedExperiments[idx];
                if (activeAndDoneContracts(exp, targetBody) == 0)
                {
                    unlockedNoContract.Add(exp);
                }
            }
            if (unlockedNoContract.Count == 0)
            {
                return null;
            }
            else
            {
                return unlockedNoContract[UnityEngine.Random.Range(0, unlockedNoContract.Count)];
            }
        }

        private bool setTargetExperiment(ExperimentData exp)
        {
            if (exp == null) {
                NE_Helper.log("Generate Contract: Experiment null");
                return false; }
            NE_Helper.log("Generate Contract: Experiment: " + exp.getAbbreviation());
            this.experiment = exp;
            return true;
        }

        /** Return either Kerbin, or a random one out of a list of bodies we have reached. */
        private CelestialBody getTargetBody()
        {
            CelestialBody ret = null;
            List<CelestialBody> bodies = Contract.GetBodies_Reached(true, false);
            if (bodies != null && bodies.Count > 0) {
                ret = bodies [UnityEngine.Random.Range (0, bodies.Count - 1)];
            } else {
                ret = Planetarium.fetch.Home;
            }
            // ASSERT: ret == Kerbin, or a random planet which we have reached
            return ret;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return (targetBody.bodyName + experiment.getId());
        }

        protected override string GetTitle()
        {
            return Localizer.Format("#ne_Run_experiment_1_in_orbit_around_2_and_return_it_to_3",
                experiment.getAbbreviation(), targetBody.GetDisplayName(), Planetarium.fetch.Home.GetDisplayName()
            );
        }

        protected override string GetDescription()
        {
            // Topic and Subject are unused in the standard StoryDefs.cfg for "CollectScience" missions
            return TextGen.GenerateBackStories("CollectScience", Agent.Name, "", "", MissionSeed, true, true, true);
        }

        protected override string GetSynopsys()
        {
            return Localizer.Format("#ne_Run_experiment_1_in_orbit_around_2",
                experiment.getAbbreviation(), targetBody.GetDisplayName()
            );
        }

        protected override string MessageCompleted()
        {
            return Localizer.Format("#ne_You_have_succesfully_run_the_experiment_1_in_orbit_around_2",
                experiment.getAbbreviation(), targetBody.GetDisplayName()
            );
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
            setTargetExperiment((KeminiExperimentData)KeminiExperimentData.getExperimentDataFromNode(node.GetNode(ExperimentData.CONFIG_NODE_NAME)));
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(TARGET_BODY, bodyID);

            node.AddNode(experiment.getNode());
        }

        /** Return TRUE if requirements for experiment are met, FALSE otherwise.
         * NOTE: This is called at least once per frame, so should be performant.
         */
        public override bool MeetRequirements()
        {
            // Must have "ModuleManager" mod installed (otherwise Kemini is disabled)
            if (!DependancyChecker.HasModuleManager)
            {
                return false;
            }

            // Must have successfully reached orbit and landed a kerballed craft
            var progress = ProgressTracking.Instance.GetBodyTree(Planetarium.fetch.Home);
            return progress != null &&  progress.returnFromOrbit.IsCompleteManned;
        }

        private void addExperimentalParts()
        {
            AvailablePart ap = ExperimentFactory.getPartForExperiment(ExperimentFactory.KEMINI_EXPERIMENTS, experiment);
            if (ap != null && !ResearchAndDevelopment.PartModelPurchased(ap))
            {
                NE_Helper.log("Adding experimental part: " + ap.name);
                ResearchAndDevelopment.AddExperimentalPart(ap);
            }
        }

        /** Removes experimental part.
         *  TODO: Check if there is another active contract using this part in which case we have to keep it! */
        private void removeExperimentalParts()
        {
            AvailablePart ap = ExperimentFactory.getPartForExperiment(ExperimentFactory.KEMINI_EXPERIMENTS, experiment);
            if (ap != null && ResearchAndDevelopment.IsExperimentalPart(ap))
            {
                NE_Helper.log("Removing experimental part: " + ap.name);
                ResearchAndDevelopment.RemoveExperimentalPart(ap);
            }
        }

        protected override void OnAccepted()
        {
            base.OnAccepted();
            addExperimentalParts();
        }

        protected override void OnFinished()
        {
            base.OnFinished();
            NE_Helper.log("Finished experiment " + experiment.getAbbreviation());
            removeExperimentalParts();
        }

        protected override void OnCancelled()
        {
            base.OnCancelled();
            NE_Helper.log("Cancelled experiment " + experiment.getAbbreviation());
            removeExperimentalParts();
        }

        protected override void OnFailed()
        {
            base.OnFailed();
            NE_Helper.log("Failed experiment " + experiment.getAbbreviation());
            removeExperimentalParts();
        }
    }
}
