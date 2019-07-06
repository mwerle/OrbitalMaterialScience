using System;
using System.Collections.Generic;
using KSP.Localization;

namespace NE_Science
{
    public abstract class MEPExperimentData : MultiStepExperimentData<MEPResourceExperimentStep>
    {

        private Guid cachedVesselID;
        private int partCount;
        private MEP_Module[] physicsLabCache = null;

        protected MEPExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost, int numSteps)
            : base(id, type, name, abb, eq, mass, cost, numSteps)
        { }

        /** Sets up the required number of test subjects */
        protected void setExperimentStep(string resourceName, float resourceAmount, string stepName, int index)
        {
            steps[index]= new MEPResourceExperimentStep(this, resourceName, resourceAmount, stepName, index);
        }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            if (physicsLabCache == null || cachedVesselID != vessel.id || partCount != vessel.parts.Count)
            {
                physicsLabCache = UnityFindObjectsOfType(typeof(MEP_Module)) as MEP_Module[];
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            for (int idx = 0, count = physicsLabCache.Length; idx < count; idx++)
            {
                var lab = physicsLabCache[idx];
                if (lab.vessel == vessel && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
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

        /** Returns the total amount of time required to run this experiment. */
        public override float getTimeRequired()
        {
            float time = 0.0f;

            // Gather the time needed for every step.
            for (int idx = 0, count = steps.Length; idx < count; idx++)
            {
                switch(steps[idx].getNeededResource())
                {
                    case Resources.EXPOSURE_TIME:
                        // TODO: Load NE_MEP lab, retrieve the "ExposureTimePerHour" (currently 1.0) from the MEP_Module
                        time += steps[idx].getNeededAmount();
                        break;

                    default:
                        EquipmentRacks rack = steps[idx].getNeededEquipment();
                        LabEquipment le = EquipmentRackRegistry.getLabEquipmentForRack(rack);
                        if (le == null)
                        {
                            NE_Helper.log("Warning: Could not find lab equipemnt for " + rack);
                        }
                        else
                        {
                            time += steps[idx].getNeededAmount() / le.ProductPerHour;
                        }
                        break;
                }
            }
            return time * 60 * 60; /* Convert hours to seconds */
        }

    }

    public class MEE1_ExperimentData : MEPExperimentData
    {
        public MEE1_ExperimentData(float mass, float cost)
            : base("NE_MEE1", "MEE1", "#ne_oms_mee1_title", "MEE1", EquipmentRacks.EXPOSURE, mass, cost, 3)
        {
            setExperimentStep(Resources.LAB_TIME, 1, Localizer.GetStringByTag("#ne_Preparation"), 0);
            setExperimentStep(Resources.EXPOSURE_TIME, 20, Localizer.GetStringByTag("#ne_Exposure"), 1);
            setExperimentStep(Resources.LAB_TIME, 2, Localizer.GetStringByTag("#ne_Store_Samples"), 2);
        }
    }

    public class MEE2_ExperimentData : MEPExperimentData
    {
        public MEE2_ExperimentData(float mass, float cost)
            : base("NE_MEE2", "MEE2", "#ne_oms_mee2_title", "MEE2", EquipmentRacks.EXPOSURE, mass, cost, 3)
        {
            setExperimentStep(Resources.LAB_TIME, 1, Localizer.GetStringByTag("#ne_Preparation"), 0);
            setExperimentStep(Resources.EXPOSURE_TIME, 40, Localizer.GetStringByTag("#ne_Exposure"), 1);
            setExperimentStep(Resources.LAB_TIME, 2, Localizer.GetStringByTag("#ne_Store_Samples"), 2);
        }
    }
}
