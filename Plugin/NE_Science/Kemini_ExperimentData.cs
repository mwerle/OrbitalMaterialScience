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

namespace NE_Science
{
    /*
    * Experiments for the Kemini Research Program
    */
    public class KeminiExperimentData : StepExperimentData
    {
        private Guid cachedVesselID;
        private int partCount;
        private Kemini_Module[] KeminiLabCache = null;

        public KeminiExperimentData(string id, string type, string name, string abb, float mass, float cost, float labTime)
            : base(id, type, name, abb, EquipmentRacks.KEMINI, mass, cost)
        {
            storageType = ExperimentFactory.KEMINI_EXPERIMENTS;
            step = new ResourceExperimentStep(this, Resources.KEMINI_LAB_TIME, labTime, "", 0);
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            // Backwards-compatibility for save games from before KSP1.8
            // TODO: Remove sometime in the future
            if(step.getNeededResource() == Resources.LAB_TIME)
            {
                var stepNode = step.getNode();
                stepNode.SetValue("Res", Resources.KEMINI_LAB_TIME);
                step = ExperimentStep.getExperimentStepFromConfigNode(stepNode, this);
            }
        }         

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            if (KeminiLabCache == null || cachedVesselID != vessel.id || partCount != vessel.parts.Count)
            {
                KeminiLabCache = UnityFindObjectsOfType(typeof(Kemini_Module)) as Kemini_Module[];
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            for (int idx = 0, count = KeminiLabCache.Length; idx < count; idx++)
            {
                var lab = KeminiLabCache[idx];
                if (lab.vessel == vessel && lab.hasEquipmentInstalled(neededEquipment) && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }

        public override bool canMove(Vessel vessel)
        {
            return state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED;
        }

        public override void runLabAction()
        {
            base.runLabAction();
            // UI Optimisation - if lab is in same part as storage, automatically move a finished experiment to storage.
            if (state == ExperimentState.FINISHED)
            {
                ExperimentStorage[] storages = store.getPartGo().GetComponents<ExperimentStorage>();
                for (int idx = 0, count = storages.Length; idx < count; idx++)
                {
                    var es = storages[idx];
                    if (es.isEmpty())
                    {
                        moveTo(es);
                    }
                }
            }
        }

        public override float getTimeRequired()
        {
            // The Kemini "Lab" generates 1 LAB_TIME per hour
            return step.getNeededAmount() * 60 * 60;
        }
    }
}
