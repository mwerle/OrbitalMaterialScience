using System;
using System.Collections.Generic;
using System.Text;

namespace NE_Science
{
    /*
 * Experiments needing MPL-600
 */
    public class MPLExperimentData : StepExperimentData
    {

        private Guid cachedVesselID;
        private int partCount;
        private MPL_Module[] physicsLabCache = null;

        protected MPLExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost)
            : base(id, type, name, abb, eq, mass, cost)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            if (physicsLabCache == null || cachedVesselID != vessel.id || partCount != vessel.parts.Count)
            {
                physicsLabCache = UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[];
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            for (int idx = 0, count = physicsLabCache.Length; idx < count; idx++)
            {
                var lab = physicsLabCache[idx];
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
    }

    public class CCF_ExperimentData : MPLExperimentData
    {
        public CCF_ExperimentData(float mass, float cost)
            : base("NE_CCF", "CCF", "#ne_oms_ccf_title", "CCF", EquipmentRacks.MSG, mass, cost)
        {
            step = new ResourceExperimentStep(this, Resources.MSG_TIME, 22, "", 0);
        }
    }

    public class CFE_ExperimentData : MPLExperimentData
    {
        public CFE_ExperimentData(float mass, float cost)
            : base("NE_CFE", "CFE", "#ne_oms_cfe_title", "CFE", EquipmentRacks.MSG, mass, cost)
        {
            step = new ResourceExperimentStep(this, Resources.MSG_TIME, 40, "", 0);
        }
    }

    public class ADUM_ExperimentData : KerbalResearchExperimentData
    {
        public ADUM_ExperimentData(float mass, float cost)
            : base("NE_ADUM", "ADUM", "#ne_kls_adium_title", "ADUM", EquipmentRacks.USU, mass, cost, 4)
        {
            setExperimentSteps(Resources.ULTRASOUND_GEL, 2.5f);
        }
    }

    public class SpiU_ExperimentData : KerbalResearchExperimentData
    {
        public SpiU_ExperimentData(float mass, float cost)
            : base("NE_SpiU", "SpiU", "#ne_kls_spiu_title", "SpiU", EquipmentRacks.USU, mass, cost, 6)
        {
            setExperimentSteps(Resources.ULTRASOUND_GEL, 3f);
        }
    }
}
