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
    /// <summary>
    /// A cache for all the Kemini Labs in all the Vessels.
    /// </summary>
    public static class KeminiLabCache
    {
        private class CacheEntry
        {
            private Vessel vessel = null;
            private int partCount = 0;
            private Kemini_Module[] labCache = null;

            public Kemini_Module[] LabCache {
                get {
                    UpdateCache();
                    return labCache;
                }
            }

            public CacheEntry(Vessel v)
            {
                vessel = v;
                UpdateCache();
            }

            private void UpdateCache()
            {
                if( vessel.Parts.Count != partCount )
                {
                    partCount = vessel.Parts.Count;
                    labCache = vessel.FindPartModulesImplementing<Kemini_Module>().ToArray();
                    NE_Helper.log("Lab Cache refresh for vessel " + vessel.id);
                }
            }
        }

        /// <summary>
        /// Contains the caches for all the vessels.
        /// </summary>
        private static Dictionary<Guid,CacheEntry> vesselCache = new Dictionary<Guid,CacheEntry>();

        /// <summary>
        /// Gets all the Kemini Labs in the Vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Kemini_Module[] GetLabs(Vessel v)
        {
            CacheEntry ce = getCache(v);

            return ce.LabCache;
        }

        public static void Clear()
        {
            vesselCache.Clear();
        }

        /// <summary>
        /// Creates or updates the cache for the given vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static CacheEntry getCache(Vessel v)
        {
            CacheEntry ce = null;

            if( !vesselCache.TryGetValue(v.id, out ce) )
            {
                ce = new CacheEntry(v);
                vesselCache.Add(v.id, ce);
            }

            return ce;
        }
    }

    /*
    * Experiments for the Kemini Research Program
    */
    public class KeminiExperimentData : StepExperimentData
    {
        public KeminiExperimentData(string id, string type, string name, string abb, float mass, float cost, float labTime)
            : base(id, type, name, abb, LabEquipmentType.KEMINI, mass, cost)
        {
            storageType = ExperimentFactory.KEMINI_EXPERIMENTS;
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, labTime, "", 0);
        }

        public override List<Lab> getFreeLabs(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            foreach(Kemini_Module lab in KeminiLabCache.GetLabs(vessel))
            { 
                if (lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }

        #if false
        public override List<Lab> getFreeLabs(Part p)
        {
            List<Lab> ret = new List<Lab>();

            foreach(Kemini_Module lab in p.GetComponents<Kemini_Module>())
            {
                if(lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }
        #endif

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabs(vessel);
            return labs?.Count > 0 && state == ExperimentState.STORED;
        }

        #if false
        public override bool canMove(Vessel vessel)
        {
            return state == ExperimentState.INSTALLED;
        }
        #endif

        public override void runLabAction()
        {
            base.runLabAction();
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

        #region IMoveable
        public override bool canSourceBeDestination()
        {
            return true;
        }
        #endregion
    }
}
