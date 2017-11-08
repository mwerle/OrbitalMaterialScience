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
    public class KEESExperimentContract : Contract
    {

        public const string TARGET_BODY = "targetBody";
        public const string EXPERIMENT_STRING = "experiment";

        public const string KEES_PC = "NE.KEES.PC";
        public const string KEES_PEC = "NE.KEES.PEC";
        /*
        public const string KEES_PPMD = "NE.KEES.PPMD";
        public const string KEES_ODC = "NE.KEES.OCD";
        public const string KEES_POSAI = "NE.KEES.POSA1";
        public const string KEES_POSAII = "NE.KEES.POSA2";
        */

        // MKW TODO: Localize the experiment names for the contract generator
        private static Experiment[] experimentParts = null;

        CelestialBody targetBody = null;
        Experiment experiment = null;

        private static ConfigNode findExperiment(string experimentId)
        {
            if (experimentId == null || experimentId.Length == 0)
            {
                return null;
            }
            string searchValue = experimentId.Replace('_', '.');
            foreach(ConfigNode ed in GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION"))
            {
                string edId = ed.GetValue("id")?.Replace('_', '.');
                if (edId == searchValue)
                {
                    return ed;
                }
            }
            return null;
        }

        private static Experiment[] ExperimentParts
        {
            get
            {
                if (experimentParts == null )
                {
                    try
                    {
#if false
                        experimentParts = new Experiment[] {
                                      new Experiment(KEES_PPMD, Localizer.GetStringByTag("#ne_kees_ppmd_title"), "KEES PPMD", "PPMD"),
                                      new Experiment(KEES_POSAI, Localizer.GetStringByTag("#ne_kees_posa1_title"), "KEES POSA I", "POSA I"),
                                      new Experiment(KEES_ODC, Localizer.GetStringByTag("#ne_kees_odc_title"), "KEES ODC", "ODC"),
                                      new Experiment(KEES_POSAII, Localizer.GetStringByTag("#ne_kees_posa2_title"), "KEES POSA II", "POSA II")
                                  };
#elif false
                        List<Experiment> el = new List<Experiment>();
                        foreach (AvailablePart p in PartLoader.LoadedPartsList)
                        {
                            ConfigNode ke = p.partConfig?.GetNode("MODULE", "name", "KEESExperiment");
                            if (ke != null)
                            {
                                string experimentId = ke.GetValue("experimentID")?.Replace('_','.');
                                string experimentTitle = findExperiment(experimentId)?.GetValue("title");
                                //string experimentShortName = experimentId?.Substring(3)?.Replace('.', ' ');
                                string experimentShortName = ke.GetValue("shortDisplayName");
                                if (experimentShortName == null)
                                {
                                    experimentShortName = experimentId?.Substring(3)?.Replace('.', ' ');
                                }
                                el.Add(new Experiment(experimentId, experimentTitle, experimentShortName, experimentShortName));
                            }
                        }
#else
                        /* Find all KEES experiments and add them to our registry for generating contracts. */
                        List<Experiment> el = new List<Experiment>();
                        ConfigNode[] experiments = GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION");
                        for( int idx = 0; idx < experiments.Length; idx++)
                        {
                            ConfigNode ed = experiments[idx];
                            string experimentId = ed.GetValue("id");
                            if (experimentId != null && experimentId.StartsWith("NE_KEES"))
                            {
                                string experimentPartName = experimentId.Replace('_','.');
                                if ( PartLoader.getPartInfoByName(experimentPartName) != null )
                                {
                                    string experimentTitle = ed.GetValue("title");
                                    string experimentShortName = ed.GetValue("shortDisplayName");
                                    string experimentAbbreviation = ed.GetValue("abbreviation");
                                    el.Add(new Experiment(experimentPartName, experimentTitle, experimentShortName, experimentAbbreviation));
                                } else {
                                    NE_Helper.logError("KEES Configuration mismatch - experiment " + experimentId + " defined, but no matching part found.");
                                }
                            }
                        }
#endif
                        experimentParts = el.ToArray();
                    } catch ( Exception e )
                    {
                        NE_Helper.logError("Could not initialize list of KEES Experiments for the Contract Engine: " + e.Message );
                        experimentParts = new Experiment[0];
                    }
                }
                return experimentParts;
            }
        }

        protected override bool Generate()
        {
            NE_Helper.log("Generate Contract");
            if (activeContracts() >= getMaxKEESContracts())
            {
                NE_Helper.log("Generate Contract: Max Contracts reached: " + getMaxKEESContracts());
                return false;
            }
            targetBody = getTargetBody();
            if (targetBody == null)
            {
                NE_Helper.log("Generate Contract: Body null set Kerbin as Target");
                targetBody = Planetarium.fetch.Home;
            }
            else
            {
                NE_Helper.log("Generate Contract: Body: " + targetBody.name);
            }

            if (!setTargetExperiment(getTargetExperiment())) return false;

            NE_Helper.log("Generate Contract: Add Parameter");
            AddParameter(new Parameters.KEESDoExperimentParameter(targetBody, getPartForExperiment(experiment)));
            AddParameter(new Parameters.KEESReturnExperimentParameter(targetBody, getPartForExperiment(experiment)));

            NE_Helper.log("Generate Contract: set Values ");
            base.SetExpiry();
            base.SetScience(5f, targetBody);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(80f, 30f, targetBody);
            base.SetFunds(15000f, 30000f, 5000f, targetBody);
            agent = AgentList.Instance.GetAgent("Nehemiah Engineering");
            NE_Helper.log("Generate Contract: done Exp: " + experiment.getAbbreviation() + " Body: " + targetBody.name + " funds Adv: " + this.FundsAdvance);
            return true;
        }

        private int getMaxKEESContracts()
        {
            //int bonus = (int)(3f * ScenarioUpgradeableFacilities.GetFacilityLevel("SpaceCenter/MissionControl"));
            //return 3 + bonus;
            return 2;
        }

        private int activeContracts(String experimentPartName = null, CelestialBody body = null)
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

            var contracts = ContractSystem.Instance.GetCurrentContracts<KEESExperimentContract>();
            for (int idx = 0, count = contracts.Length; idx < count; idx++)
            {
                var keesCon = contracts[idx];
                if ((keesCon.ContractState == Contract.State.Active || keesCon.ContractState == Contract.State.Offered)
                    && (experimentPartName == null || keesCon.experiment != null)
                    && (body == null || keesCon.targetBody != null)
                    && ((experimentPartName == null || experimentPartName == keesCon.experiment.getPartName())
                    && (body == null || body.bodyName == keesCon.targetBody.bodyName)))
                {
                    ret += 1;
                }
            }
            return ret;
        }

        private int activeAndDoneContracts(String experimentPartName = null, CelestialBody body = null)
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
            var contracts = ContractSystem.Instance.GetCurrentContracts<KEESExperimentContract>();
            for (int idx = 0, count = contracts.Length; idx < count; idx++)
            {
                var keesCon = contracts[idx];
                if ((keesCon.ContractState == Contract.State.Active || keesCon.ContractState == Contract.State.Offered || keesCon.ContractState == Contract.State.Completed)
                    && (experimentPartName == null || keesCon.experiment != null)
                    && (body == null || keesCon.targetBody != null)
                    && ((experimentPartName == null || experimentPartName == keesCon.experiment.getPartName())
                    && (body == null || body.bodyName == keesCon.targetBody.bodyName)))
                {
                    ret += 1;
                }
            }
            return ret;
        }

        private Experiment getTargetExperiment()
        {
            List<Experiment> unlockedExperiments = getUnlockedKEESExperiments();
            List<Experiment> unlockedNoContract = new List<Experiment>();
            for (int idx = 0, count = unlockedExperiments.Count; idx < count; idx++)
            {
                var exp = unlockedExperiments[idx];
                if (activeAndDoneContracts(exp.getPartName(), targetBody) == 0)
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

        private List<Experiment> getUnlockedKEESExperiments()
        {
            List<Experiment> unlockedParts = new List<Experiment>();
            for (int idx = 0, count = ExperimentParts.Length; idx < count; idx++)
            {
                var exp = ExperimentParts[idx];
                if (NE_Helper.IsPartTechAvailable(exp.getPartName()))
                {
                    unlockedParts.Add(exp);
                }
            }
            return unlockedParts;
        }

        private bool setTargetExperiment(Experiment exp)
        {
            if (exp == null)
            {
                NE_Helper.log("Generate Contract: Experiment null");
                return false;
            }
            AvailablePart experiment = PartLoader.getPartInfoByName(exp.getPartName());
            if (experiment == null)
            {
                NE_Helper.log("Generate Contract: Experiment Part null");
                return false;
            }
            NE_Helper.log("Generate Contract: Experiment: " + exp.getAbbreviation());
            this.experiment = exp;
            return true;
        }

        private AvailablePart getPartForExperiment(Experiment exp)
        {
            return PartLoader.getPartInfoByName(exp.getPartName());
        }

        private CelestialBody getTargetBody()
        {
            List<CelestialBody> bodies = Contract.GetBodies_Reached(true, false);
            return bodies[UnityEngine.Random.Range(0, bodies.Count)];

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
            return (targetBody.bodyName + experiment.getPartName());
        }
        protected override string GetTitle()
        {
            return Localizer.Format("#ne_Run_experiment_1_in_orbit_around_2_and_return_it_to_3",
                experiment.getShortName(), targetBody.GetDisplayName(), Planetarium.fetch.Home.GetDisplayName()
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
                experiment.getName(), targetBody.GetDisplayName()
            );
        }
        protected override string MessageCompleted()
        {
            return Localizer.Format("#ne_You_have_succesfully_run_the_experiment_1_in_orbit_around_2",
                experiment.getShortName(), targetBody.GetDisplayName()
            );
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = NE_Helper.GetValueAsInt(node, TARGET_BODY);
            for (int idx = 0, count = FlightGlobals.Bodies.Count; idx < count; idx++)
            {
                var body = FlightGlobals.Bodies[idx];
                if (body.flightGlobalsIndex == bodyID)
                {
                    targetBody = body;
                }
            }
            setTargetExperiment(getExperimentByPartName(node.GetValue(EXPERIMENT_STRING)));
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(TARGET_BODY, bodyID);
            node.AddValue(EXPERIMENT_STRING, experiment.getPartName());
        }

        private Experiment getExperimentByPartName(string partName)
        {
            for (int idx = 0, count = ExperimentParts.Length; idx < count; idx++)
            {
                var exp = ExperimentParts[idx];
                if (exp.getPartName() == partName)
                {
                    return exp;
                }
            }
            return new Experiment("", "", "", ""); //Nullobject;
        }

        public override bool MeetRequirements()
        {
            // Must have the "KIS" mod installed (otherwise Kemini is disabled anyway)
            if (!DependancyChecker.HasKIS)
            {
                return false;
            }

            // Must have successfully reached orbit and landed a kerballed craft
            var progress = ProgressTracking.Instance.GetBodyTree(Planetarium.fetch.Home);
            if (progress == null || !progress.returnFromOrbit.IsCompleteManned)
            {
                return false;
            }

            // Must be able to perform EVAs
            if (ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex) < 0.1f)
            {
                return false;
            }

            // And must have unlocked the tech-tiers for the first KEES experiment and the Exposure bracket
            return (NE_Helper.IsPartTechAvailable(KEES_PEC)); // && NE_Helper.IsPartTechAvailable(KEES_PPMD));
        }
    }

    public class Experiment
    {

        private string name;
        private string shortName;
        private string abbrev;
        private string partName;

        public Experiment(string partName, string name, string shortName, string abbrev)
        {
            this.partName = partName;
            this.name = name;
            this.shortName = shortName;
            this.abbrev = abbrev;
        }
        public string getPartName()
        {
            return partName;
        }

        public string getName()
        {
            return name;
        }

        public string getShortName()
        {
            return shortName;
        }

        public string getAbbreviation()
        {
            return abbrev;
        }
    }
}
