/*
 *   This file is part of Orbital Material Science.
 *
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    public abstract class Lab : PartModule, IPartMassModifier
    {
        #region KSPFields
        /// A KSPField is a class member which can be automatically persisted in the save file
        /// or displayed as a Tweakable.

        ///
        /// Minimum crew required to operate this Lab.
        ///
        [KSPField(isPersistant = false)]
        public int minimumCrew = 0;

        /// <summary>
        /// Current status whether the lab is active / performing research.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool doResearch = true;

        /// <summary>
        /// Time when the lab was last active.
        /// </summary>
        [KSPField(isPersistant = true)]
        public string last_active = "0";

        /// <summary>
        /// Abbreviation used in various places.
        /// </summary>
        [KSPField(isPersistant = false)]
        public string abbreviation = "";

        /// <summary>
        /// GUI field to display the current lab status.
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false, guiName = "#ne_Lab_Status")]
        public string labStatus = "";

        ///
        /// Last count of crew in Part.
        /// This can be different to part.protoModuleCrew.Count if a crew member has just entered or left the Part.
        ///
        [KSPField(isPersistant = false)]
        private int m_cCrew = 0;
        #endregion

        public double LastActive
        {
            get
            {
                try
                {
                    return Double.Parse(last_active);
                }
                catch (FormatException)
                {
                    last_active = "0";
                    return 0;
                }
            }

            set
            {
                last_active = value.ToString();
            }
        }

        public virtual void installExperiment(ExperimentData exp)
        {

        }

        protected LabEquipmentSlot getLabEquipmentSlotByType(ConfigNode configNode, string type)
        {
            LabEquipmentSlot rv = null;

            if (configNode == null)
            {
                NE_Helper.logError("Lab getLabEquipmentSlotByType: parent node null");
                goto done;
            }

            // Find a NE_LabEquipmentSlot of the correct type
            ConfigNode cn = configNode.GetNode(LabEquipmentSlot.CONFIG_NODE_NAME, "type", type);
            if (cn == null)
            {
                // Pre-Kemini-0.3 savegames have another level of nesting of ConfigNodes so let's recursively look into the child-nodes
                foreach(ConfigNode child in configNode.nodes)
                {
                    rv = getLabEquipmentSlotByType(child, type);
                    if (rv != null)
                    {
                        goto done;
                    }
                }

                // Not found, so let's raise an error
                NE_Helper.logError("Lab getLabEquipmentSlotByType: node " + configNode.name
                    + " does not contain a " + LabEquipmentSlot.CONFIG_NODE_NAME
                    + " node of type " + type);
                goto done;
            }
            rv = LabEquipmentSlot.getLabEquipmentSlotFromConfigNode(cn, this);

        done:
            return rv != null? rv : new LabEquipmentSlot(EquipmentRacks.NONE);
        }

        protected LabEquipmentSlot getLabEquipmentSlot(ConfigNode configNode)
        {
            if (configNode != null)
            {
                return LabEquipmentSlot.getLabEquipmentSlotFromConfigNode(configNode.GetNode(LabEquipmentSlot.CONFIG_NODE_NAME), this);
            }
            else
            {
                NE_Helper.logError("Lab GetLabEquipmentSlotFromNode: LabEquipmentSlotNode null");
                return new LabEquipmentSlot(EquipmentRacks.NONE);
            }
        }

        protected ConfigNode getConfigNodeForSlot(string nodeName, LabEquipmentSlot slot)
        {
            ConfigNode node = new ConfigNode(nodeName);
            node.AddNode(slot.getConfigNode());
            return node;
        }

        public virtual GameObject getExperimentGO(String id)
        {
            return null;
        }
        public double getResourceAmount(string name)
        {
            return ResourceHelper.getResourceAmount(part, name);
        }

        public PartResource setResourceMaxAmount(string name, double max)
        {
            return ResourceHelper.setResourceMaxAmount(part, name, max);
        }

        protected List<Generator> generators;

        public void addGenerator(Generator g)
        {
            generators.Add(g);
        }

        protected virtual bool isActive()
        {
            return doResearch && part.protoModuleCrew.Count >= minimumCrew && !OMSExperiment.checkBoring(vessel, false);
        }

        protected virtual void displayStatusMessage(string s)
        {
            labStatus = s;
        }

        protected V getOrDefault<K, V>(Dictionary<K, V> dict, K key)
        {
            try
            {
                return dict[key];
            }
            catch (KeyNotFoundException)
            {
                return default(V);
            }
        }

        public bool isUnderstaffed()
        {
            return part.protoModuleCrew.Count < minimumCrew;
        }

        /** This function is called whenever the display of the status messages should change. */
        private void updateStatus()
        {
            if (!doResearch)
            {
                if(OMSExperiment.checkBoring(vessel, false))
                {
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Go_to_space"));
                }
                else if(isUnderstaffed())
                {
                    displayStatusMessage(Localizer.Format("#ne_Understaffed_1_of_2", part.protoModuleCrew.Count, minimumCrew));
                }
                else
                {
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Paused"));
                }
            }
            else
            {
                updateLabStatus();
            }
        }

        /// <summary>
        /// This function is called regularly and should be used to update any state
        /// or displays which cannot be updated via events.
        /// </summary>
        /// The primary state which must be polled is to detect when an experiment has finished.
        protected virtual void updateLabStatus()
        {
            /* Default implementation : no-op */
        }

        /** Called whenever the state of the lab changes to stopped, such as when understaffed or paused */
        protected virtual bool onLabPaused()
        {
            if (!canPerformLabActions())
            {
                return false;
            }

            doResearch = false;
            Events["labAction"].guiName = "#ne_Resume_Research";
            return true;
        }

        /// <summary>
        /// Called whenever crew enters or leaves this part.
        /// </summary>
        protected virtual void onCrewMoved()
        {
            /* Crew left and we dropped below the minimum crew requirement. */
            if( m_cCrew >= minimumCrew && part.protoModuleCrew.Count < minimumCrew )
            {
                onLabPaused();
            }
            /* Crew entered and we went above the minimum crew requirement. */
            if( m_cCrew < minimumCrew && part.protoModuleCrew.Count >= minimumCrew )
            {
                updateStatus();
                /* Lab is not automatically restarted, player must press button. */
            }
        }

        /// <summary>
        /// Query whether we can perform lab actions such as starting or pausing the lab.
        /// </summary>
        /// <returns></returns>
        protected virtual bool canPerformLabActions()
        {
            return !isUnderstaffed();
        }

        /// <summary>
        /// Called when the state of the lab changes to started.
        /// </summary>
        /// This can occur when crew members enter the Part, or the user clicks on resume.
        /// It is always called  before updateLabStatus()
        /// <returns>True if the lab was started</returns>
        protected virtual bool onLabStarted()
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            if (OMSExperiment.checkBoring(vessel, true))
            {
                return false;
            }

            doResearch = true;
            Events["labAction"].guiName = "#ne_Pause_Research";
            updateStatus();
            return true;
        }

        double owed_time = 0;

        /// <summary>
        /// Called once after the Part is initialized (ie, after OnAwake) but before the first Update.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            this.part.force_activate();
            generators = new List<Generator>();

            m_cCrew = part.protoModuleCrew.Count;

            if (LastActive > 0)
            {
                owed_time = Planetarium.GetUniversalTime() - LastActive;
            }
            Events["labAction"].guiName = doResearch? "#ne_Pause_Research" : "#ne_Resume_Research";
            Fields["labStatus"].guiActive = true;
            StartCoroutine(updateState());
        }

        /// <summary>
        /// Coroutine running while the Part is active.
        /// </summary>
        /// This is more efficient than running OnUpdate.
        /// <returns></returns>
        public System.Collections.IEnumerator updateState()
        {
            while (true)
            {
                /* Unfortunately there doesn't appear to be an Event for this, so we need to poll
                 * to Check if crew has entered or left this part. */
                if( m_cCrew != part.protoModuleCrew.Count )
                {
                    onCrewMoved();
                    m_cCrew = part.protoModuleCrew.Count;
                }
                else
                {
                    updateStatus();
                }
                yield return new UnityEngine.WaitForSeconds(1f);
            }
        }

        public void startResearch()
        {
            if( !onLabStarted() )
            {
                return;
            }
        }

        public void stopResearch()
        {
            if( !onLabPaused() )
            {
                return;
            }
        }

        #region KSPEvents
        // KSPEvents can display a gui button in the part action menu

        [KSPEvent(guiActive = true, guiName = "#ne_Resume_Research", active = true)]
        public void labAction()
        {
            if(!doResearch)
            {
                startResearch();
            }
            else
            {
                stopResearch();
            }
        }
        #endregion

        #region KSPActions
        // KSPActions can be bound to action keys in the Editor
        [KSPAction("#ne_Resume_Research")]
        public void startResearchingAction(KSPActionParam param)
        {
            startResearch();
        }

        [KSPAction("#ne_Pause_Research")]
        public void stopGeneratingAction(KSPActionParam param)
        {
            stopResearch();
        }

        [KSPAction("#ne_Toggle_Research")]
        public void toggleResearchAction(KSPActionParam param)
        {
            if (doResearch)
                stopResearch();
            else
                startResearch();
        }
        #endregion

        /// <summary>
        /// Called by Unity on every physics tic.
        /// </summary>
        /// This should be used for any physics calculations.
        public override void OnFixedUpdate()
        {
            if (isActive())
            {
                for (int idx = 0, count = generators.Count; idx < count; idx++)
                {
                    generators[idx].doTimeStep(TimeWarp.fixedDeltaTime + owed_time);
                }
                owed_time = 0;
                LastActive = Planetarium.GetUniversalTime();
            }
            else
            {
                LastActive = 0;
            }
        }

        public override string GetInfo()
        {
            string ret = "";
            if (minimumCrew > 0)
                ret += Localizer.Format("#ne_Researchers_required_1", minimumCrew);

            return ret;
        }


        /// <summary>
        /// To be implemented by derived class; this method should return the mass of all installed equipment and experiments
        /// </summary>
        protected virtual float getMass()
        {
            return 0f;
        }

        /// <summary>Refresh cost and mass</summary>
        public void RefreshMassAndCost()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        #region IPartMassModifier overrides
        /// <summary>Overridden from IPartMassModifier</summary>
        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        /// <summary>Overridden from IPartMassModifier</summary>
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return getMass();
        }
        #endregion
    }

    public class Generator
    {
        public class Rate
        {
            public Generator owner;
            public readonly string resource;

            public double ratePerHour;

            public double ratePerDay
            {
                get { return ratePerHour * 24; }
                set { ratePerHour = value / 24; }
            }
            public double ratePerMinute
            {
                get { return ratePerHour / 60; }
                set { ratePerHour = value * 60; }
            }
            public double ratePerSecond
            {
                get { return ratePerHour / 3600; }
                set { ratePerHour = value * 3600; }
            }
            public double ratePerFixedDelta
            {
                get { return (ratePerHour * TimeWarp.fixedDeltaTime) / 3600; }
            }

            public double last_produced = 0, last_available = 0, last_ratePerHour = 0;

            public Rate(Generator _owner, string _resource, double _ratePerHour = 0)
            {
                this.owner = _owner;
                this.resource = _resource;
                this.ratePerHour = _ratePerHour;
                isScience = (resource.Length >= SCIENCE_PREFIX.Length && resource.Substring(0, SCIENCE_PREFIX.Length) == "__SCIENCE__");
                if (isScience)
                    subjectString = resource.Substring(SCIENCE_PREFIX.Length);
            }

            public static string SCIENCE_PREFIX = "__SCIENCE__";

            public readonly bool isScience;

            public readonly string subjectString = "";

            public ScienceSubject getSubject()
            {
                return ScienceHelper.getScienceSubject(subjectString, owner.part.vessel);
            }

            public double getAvailable()
            {
                if (isScience)
                    return ResearchAndDevelopment.Instance.Science;
                else
                    return ResourceHelper.getAvailable(owner.part, this.resource);
            }

            public double getDemand()
            {
                if (isScience)
                    return Double.PositiveInfinity;
                else
                    return ResourceHelper.getDemand(owner.part, this.resource);
            }

            public double getMaxStep()
            {
                double available;
                if (ratePerHour > 0)
                    available = getAvailable();
                else
                    available = -getDemand();
                last_available = available;
                return available / ratePerSecond;
            }

            private double owed = 0;
            private double subjOwed = 0;
            private string subjOwedId = "";
            public double rateMultiplier = 1;

            public double requestResource(double amount)
            {
                if (isScience)
                {
                    if (amount < 0)
                    {
                        ScienceSubject subject = getSubject();
                        if (subject != null)
                        {
                            double cap = subject.scienceCap;
                            double curve_base = (cap - 1) / (cap);
                            double old_science = subject.science;
                            double precurve_amount = -amount * subject.subjectValue;
                            double new_science = (1 - (1 - (old_science / cap)) * Math.Pow(curve_base, precurve_amount)) * cap;
                            rateMultiplier = subject.subjectValue * ((new_science - old_science) / precurve_amount);
                            if (subject.id != subjOwedId)
                                subjOwed = 0;
                            subject.science = (float)(new_science + subjOwed);
                            subjOwed = -(((double)subject.science) - new_science - subjOwed);
                            //print("[" + old_science + "/" + cap + "] (+) " + precurve_amount + " == " + new_science + " [" + subject.science + " + " + subjOwed + "]");
                            subjOwedId = subject.id;
                            amount = -(subject.science - old_science);
                        }
                        else
                        {
                            rateMultiplier = ScienceHelper.getScienceMultiplier(owner.part.vessel);
                            amount *= rateMultiplier;
                        }
                    }
                    else
                        rateMultiplier = 1;
                    double sci_before = ResearchAndDevelopment.Instance.Science;
                    /* MKW - Which TransactionReason should we use?? */
                    ResearchAndDevelopment.Instance.AddScience((float)(-amount + owed), TransactionReasons.ScienceTransmission);
                    owed = (-amount + owed + sci_before) - ((double)ResearchAndDevelopment.Instance.Science);
                    return sci_before - ResearchAndDevelopment.Instance.Science;
                }
                else
                {
                    rateMultiplier = 1;
                    double produced = owner.part.RequestResource(resource, amount + owed);
                    owed = (amount + owed) - produced;
                    return produced;
                }
            }

            public void produce(double timeStep)
            {
                if (timeStep == 0)
                {
                    last_produced = 0;
                }
                else
                {
                    last_produced = ratePerSecond * timeStep;
                    //print(resource + " to_produce: " + ratePerSecond + "*" + timeStep + "=" + last_produced);
                    double actual = requestResource(last_produced);
                    //print(resource + " actual: " + actual);
                }
            }
        }

        public Dictionary<string, Rate> rates = new Dictionary<string, Rate>();
        public Part part = null;
        public double last_time_step = 0;

        public double getMaxStep()
        {
            //return rates.Values.Max(rate => rate.getMaxStep());
            double ret = Double.PositiveInfinity;
            foreach (Rate rate in rates.Values)
            {
                double step = rate.getMaxStep();
                if (step < ret)
                {
                    ret = step;
                }
            }
            return ret;
        }

        public void doTimeStep(double seconds)
        {
            //print("doTimeStep: " + seconds);
            last_time_step = Math.Min(seconds, getMaxStep());
            //print("last_time_step: " + last_time_step);
            foreach (Rate rate in rates.Values)
            {
                rate.produce(last_time_step);
            }
        }

        public Rate addRate(string resource, double ratePerHour = 0)
        {
            Rate ret = new Rate(this, resource, ratePerHour);
            rates.Add(resource, ret);
            return ret;
        }

        public Generator(Part _part)
        {
            this.part = _part;
        }
    }
}
