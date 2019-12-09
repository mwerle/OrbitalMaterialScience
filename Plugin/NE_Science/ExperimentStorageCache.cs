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
using System.Threading.Tasks;

namespace NE_Science
{
#if true
    /// <summary>
    /// This class caches the ExperimentStorage for a single vessel.
    /// </summary>
    /// The main reason for the cache is to avoid the expensive per-frame check
    /// for whether experiments can be moved or not which relies on a list of
    /// empty ExperimentStorage components.
    ///
    /// The implementation is a Singleton class with a number of static accessor
    /// methods.
    class ExperimentStorageCache
    {
        #region Public Interface

        /// <summary>
        /// Returns the Singleton Instance. Should generally not be used by client code.
        /// </summary>
        public static ExperimentStorageCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExperimentStorageCache();
                }
                return instance;
            }
        }

        /// <summary>
        /// Retrieves all the ExperimentStorage components for the given Vessel.
        /// </summary>
        /// This function is fairly performant and can be called every frame.
        /// <param name="v"></param>
        /// <returns></returns>
        public static List<ExperimentStorage> GetExperimentStorage(Vessel v)
        {
            var cache = Instance.getCacheForVessel(v);
            return cache;
        }

        /// <summary>
        /// Retrieves all the empty ExperimentStorage components for the given Vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static List<ExperimentStorage> GetFreeExperimentStorage(Vessel v)
        {
            var cache = Instance.getEmptyCacheForVessel(v);
            return cache;
            // This is not very performant and generates quite a bit of garbage.
            // TODO: cache the list of empty containers and only update after an
            // experiment has been moved.
            //var cache = Instance.getCacheForVessel(v);
            //return Array.FindAll(cache.ToArray(), p => p.isEmpty());
        }

        public static void NotifyExperimentMoved()
        {
            Instance.OnExperimentMoved();
        }

        #endregion


        #region Private Implementation
        /// <summary>
        /// The static singleton.
        /// </summary>
        private static ExperimentStorageCache instance = null;

        /// <summary>
        /// The Id of the vessel which we are currently caching.
        /// </summary>
        private Guid vesselId;

        /// <summary>
        /// The cached parts of the vessel.
        /// </summary>
        private int vesselPartCount;

        /// <summary>
        /// The actual cache.
        /// </summary>
        private List<ExperimentStorage> cacheEntries;

        /// <summary>
        /// The cache of empty storage containers
        /// </summary>
        private List<ExperimentStorage> emptyCacheEntries;
        private bool isEmptyCacheDirty;

        /// <summary>
        /// Private constructor to enforce Singleton.
        /// </summary>
        private ExperimentStorageCache()
        {
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
            cacheEntries = new List<ExperimentStorage>();
            emptyCacheEntries = new List<ExperimentStorage>();
            Clear();
        }

        ~ExperimentStorageCache()
        {
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
        }

        private void Clear()
        {
            vesselId = Guid.Empty;
            vesselPartCount = 0;
            cacheEntries.Clear();
            emptyCacheEntries.Clear();
            isEmptyCacheDirty = true;
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
            Clear();
        }

        private void OnExperimentMoved()
        {
            isEmptyCacheDirty = true;
        }

        private List<ExperimentStorage> getCacheForVessel(Vessel v)
        {
            UpdateCache(v);
            return cacheEntries;
        }

        private List<ExperimentStorage> getEmptyCacheForVessel(Vessel v)
        {
            if (isEmptyCacheDirty)
            {
                UpdateCache(v);
                emptyCacheEntries.Clear();
                for (var idx = 0; idx < cacheEntries.Count; idx++)
                {
                    if (cacheEntries[idx].isEmpty())
                    {
                        emptyCacheEntries.Add(cacheEntries[idx]);
                    }
                }
                isEmptyCacheDirty = false;
            }
            return emptyCacheEntries;
        }


        private void UpdateCache(Vessel vessel)
        {
            if ((vessel.id != vesselId) || (vessel.Parts.Count != vesselPartCount))
            {
                vesselId = vessel.id;
                vesselPartCount = vessel.Parts.Count;
                //cacheEntries = vessel.GetComponents<ExperimentStorage>();
                cacheEntries = vessel.FindPartModulesImplementing<ExperimentStorage>();
                NE_Helper.log("ExperimentStorage Cache refresh for vessel " + vessel.id);
            }
        }

    #endregion
    } // ExperimentStorageCache

#else

    /// <summary>
    /// This class implements a single cache of ExperimentStorage across all Vessels.
    /// </summary>
    /// The cache is indexed by Vessel. Accessor methods exist to retrieve all ExperimentStorage
    /// or a subset (all the full or empty ones).
    class ExperimentStorageCache
    {
        /// <summary>
        /// Returns the Singleton Instance.
        /// </summary>
        public static ExperimentStorageCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExperimentStorageCache();
                }
                return instance;
            }
        }

        public static bool IsDirty(Vessel v)
        {
            var cache = Instance.getCacheForVessel(v);
            return cache.IsDirty;
        }

        /// <summary>
        /// Clears the entire cache.
        /// </summary>
        public static void Clear()
        {
            Instance.cache.Clear();
        }

        /// <summary>
        /// Retrieves all the ExperimentStorage components for the given Vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static ExperimentStorage[] GetExperimentStorage(Vessel v)
        {
            var cache = Instance.getCacheForVessel(v);
            return cache.Entries;
        }

        /// <summary>
        /// Retrieves all the empty ExperimentStorage components for the given Vessel.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static ExperimentStorage[] GetFreeExperimentStorage(Vessel v)
        {
            var cache = Instance.getCacheForVessel(v);
            return Array.FindAll(cache.Entries, p => p.isEmpty());
        }

        #if false

        #region CacheEntry
        /// <summary>
        /// A cache entry for a particular vessel.
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// For fuck's sake, I have no idea what's wrong, but as soon as "vessel" is set, it also sets
            /// vesselId and vesselPartCount, rendering the tests on whether or not the cache needs to be
            /// regenerated invalid.
            /// </summary>
            
            // I've got no idea if the Guid for a Vessel can change if it docks/undocks
            private Guid myId;
            // Staging, explosions, docking, ..
            private int myCount;
            private ExperimentStorage[] storageCache;
            private Vessel vessel;

            public CacheEntry(Vessel v)
            {
                vessel = v;
                myId = vessel.id;
                myCount = 0;
                UpdateCache();
            }

            public Guid VesselId { get { return myId; } }

            public bool IsDirty { get { return myCount != vessel.Parts.Count; } }

            public ExperimentStorage[] Entries
            {
                get
                {
                    UpdateCache();
                    return storageCache;
                }
            }

            private void UpdateCache()
            {
                if ((vessel.id != myId) || (vessel.Parts.Count != myCount))
                {
                    myId = vessel.id;
                    myCount = vessel.Parts.Count;
                    storageCache = vessel.GetComponents<ExperimentStorage>();
                    NE_Helper.log("ExperimentStorage Cache refresh for vessel " + vessel.id);
                }
            }
        }
        #endregion

        #region Private Implementation

        private static ExperimentStorageCache instance = null;

        private Dictionary<Guid,CacheEntry> cache = null;

        /// <summary>
        /// Private constructor to enforce Singleton.
        /// </summary>
        private ExperimentStorageCache()
        {
            cache = new Dictionary<Guid,CacheEntry>();

            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
        }

        /// <summary>
        /// Called when the player switches scenes.
        /// </summary>
        /// <param name="newScene"></param>
        void OnLevelWasLoaded(GameScenes newScene)
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
                    cache.Clear();
                    break;
            }
        }

        /// <summary>
        /// Called when a vessel is about to be destroyed.
        /// </summary>
        /// <param name="v"></param>
        void OnVesselDestroyed(Vessel v)
        {
            cache.Remove(v.id);
        }

        private CacheEntry getCacheForVessel(Vessel v)
        {
            CacheEntry ce = null;
            if( !cache.TryGetValue(v.id, out ce) )
            {
                ce = new CacheEntry(v);
                cache.Add(v.id, ce);
            }
            if (ce.VesselId != v.id)
            {
                throw new Exception("ARGH! Cache inconsistency detected; vessel entry doesn't match cache!");
            }
            return ce;
        }
        #endregion
        #endif
    }

#endif
}
