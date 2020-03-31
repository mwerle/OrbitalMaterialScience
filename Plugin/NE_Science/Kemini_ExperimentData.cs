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
    public class KeminiLabCache
    {
        #region Public Interface
        public static KeminiLabCache Instance
        {
            get {
                if (instance == null)
                {
                    instance = new KeminiLabCache();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets all the Kemini Labs in the Vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Kemini_Module[] GetLabs(Vessel v)
        {
            CacheEntry ce = Instance.getCache(v);

            return ce.LabCache;
        }

        public static void Clear()
        {
            Instance.vesselCache.Clear();
        }

        #endregion // public 


        #region Private Implementation
        private static KeminiLabCache instance = null;

        private KeminiLabCache()
        {
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
        }

        ~KeminiLabCache()
        {
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
        }

        /// <summary>
        /// Called when the player switches scenes.
        /// </summary>
        /// <param name="newScene"></param>
        private void OnLevelWasLoaded(GameScenes newScene)
        {
            switch(newScene)
            {
                // Ignore when flying
                case GameScenes.SPACECENTER:
                case GameScenes.FLIGHT:
                case GameScenes.TRACKSTATION:
                    break;

                // Clear cache for all other scenes
                default:
                    Clear();
                    break;
            }
        }

        /// <summary>
        /// Called when a vessel is about to be destroyed.
        /// </summary>
        /// <param name="v"></param>
        private void OnVesselDestroyed(Vessel v)
        {
            Instance.vesselCache.Remove(v.id);
        }

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
        private Dictionary<Guid,CacheEntry> vesselCache = new Dictionary<Guid,CacheEntry>();

        /// <summary>
        /// Game time when the cache was last accessed
        /// </summary>
        private double lastTime = 0;

        /// <summary>
        /// Creates or updates the cache for the given vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private CacheEntry getCache(Vessel v)
        {
            CacheEntry ce = null;

            // If time is earlier, the player reverted, so clear the cache.
            if (HighLogic.CurrentGame.UniversalTime < lastTime)
            {
                vesselCache.Clear();
            }

            if( !vesselCache.TryGetValue(v.id, out ce) )
            {
                ce = new CacheEntry(v);
                vesselCache.Add(v.id, ce);
            }

            // Update last accessed time
            lastTime = HighLogic.CurrentGame.UniversalTime;

            return ce;
        } // class CacheEntry
        #endregion // private
    } // class KeminiLabCache

    /*
    * Experiments for the Kemini Research Program
    */
    public class KeminiExperimentData : StepExperimentData
    {
        public KeminiExperimentData(string id, string type, string name, string abb, float mass, float cost, float labTime)
            : base(id, type, name, abb, LabEquipmentType.KEMINI, mass, cost)
        {
            storageType = ExperimentFactory.KEMINI_EXPERIMENTS;
            step = new ResourceExperimentStep(this, Resources.KEMINI_LAB_TIME, labTime, "", 0);
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
                #if false
                ExperimentStorage[] storages = store.getPartGo().GetComponents<ExperimentStorage>();
                for (int idx = 0, count = storages.Length; idx < count; idx++)
                {
                    var es = storages[idx];
                    if (es.isEmpty())
                    {
                        moveTo(es);
                    }
                }
                #endif
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
