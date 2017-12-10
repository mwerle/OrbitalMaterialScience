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
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    public enum ExperimentState
    {
        STORED, INSTALLED, RUNNING, FINISHED, FINALIZED, COMPLETED
    }

    public class ExperimentData
    {
        public const string CONFIG_NODE_NAME = "NE_ExperimentData";
        public const string TYPE_VALUE = "Type";
        public const string STATE_VALUE = "State";
        private const string MASS_VALUE = "Mass";
        private const string COST_VALUE = "Cost";

        private string id;
        private string name;
        private string abb;
        private string type = "";
        private float mass = 0f;
        private float cost = 0f;
        protected EquipmentRacks neededEquipment;
        internal ExperimentState state = ExperimentState.STORED;
        internal ExperimentDataStorage store;
        protected string storageType = ExperimentFactory.OMS_EXPERIMENTS;

        private Guid cachedVesselID;
        private int partCount;
        private ExperimentStorage[] contCache = null;

        /// <summary>
        /// Creates a new Experiment data object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="name">This should be a localisation tag for the "display name".</param>
        /// <param name="abb"></param>
        /// <param name="eq"></param>
        /// <param name="mass"></param>
        /// <param name="cost"></param>
        public ExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost)
        {
            this.id = id;
            this.type = type;
            if (!Localizer.TryGetStringByTag(name, out this.name))
            {
                this.name = name;
            }
            this.abb = abb;
            this.mass = mass;
            this.cost = cost;
            neededEquipment = eq;
        }

        public string getId()
        {
            return id;
        }

        public string getName()
        {
            return name;
        }

        public string getAbbreviation()
        {
            return abb;
        }
        public float getMass()
        {
            return mass;
        }
        public float getCost()
        {
            return cost;
        }

        public EquipmentRacks getEquipmentNeeded()
        {
            return neededEquipment;
        }

        public virtual string getDescription(string linePrefix = "")
        {
            string desc =linePrefix + "<b>" + name + "</b>\n";
            desc += linePrefix + getReqString();
            return desc;
        }

        private string getReqString()
        {
            string reqString = "";// = Localizer.GetStringByTag("#ne_Needs") + ": ";
            switch (getEquipmentNeeded())
            {
                case EquipmentRacks.CIR:
                    reqString = Localizer.Format("#ne_Needs_1_with_2", "MSL-1000", "#ne_oms_eq_cir_title");
                    //reqString += "MSL-1000 with Combustion Integrated Rack (CIR)";
                    break;
                case EquipmentRacks.FIR:
                    reqString = Localizer.Format("#ne_Needs_1_with_2", "MSL-1000", "#ne_oms_eq_fir_title");
                    //reqString += "MSL-1000 with Fluid Integrated Rack (FIR)";
                    break;
                case EquipmentRacks.PRINTER:
                    reqString = Localizer.Format("#ne_Needs_1_with_2", "MSL-1000", "#ne_oms_eq_3dp_title");
                    //reqString += "MSL-1000 with 3D-Printer (3PR)";
                    break;
                case EquipmentRacks.MSG:
                    reqString = Localizer.Format("#ne_Needs_1_with_2", "MPL-600", "#ne_oms_eq_msg_title");
                    //reqString += "MPL-600 with Microgravity Science Glovebox (MSG)";
                    break;
                case EquipmentRacks.USU:
                    reqString = Localizer.Format("#ne_Needs_1_with_2", "MPL-600", "#ne_kls_eq_usu_title");
                    //reqString += "MPL-600 with Ultrasound Unit (USU)";
                    break;
                case EquipmentRacks.KEMINI:
                    reqString = Localizer.Format("#ne_Needs_1", "Command Pod mk1");
                    break;
                case EquipmentRacks.EXPOSURE:
                    reqString = Localizer.Format("#ne_Needs_1_and_2_or_3", "MEP-825", "MPL-600", "MSL-1000");
                    //reqString += "MEP-825 and MPL-600 or MSL-1000";
                    break;
            }

            return reqString;
        }

        public virtual bool canFinalize()
        {
            return state == ExperimentState.FINISHED;
        }

        public string getType()
        {
            return type;
        }

        public virtual float getTimeRemaining()
        {
            return 0;
        }

        protected virtual void load(ConfigNode node)
        {
            state = getState(node.GetValue(STATE_VALUE));
        }

        private ExperimentState getState(string s)
        {
            switch (s)
            {
                case "STORED":
                    return ExperimentState.STORED;
                case "INSTALLED":
                    return ExperimentState.INSTALLED;
                case "RUNNING":
                    return ExperimentState.RUNNING;
                case "FINISHED":
                    return ExperimentState.FINISHED;
                case "FINALIZED":
                    return ExperimentState.FINALIZED;
                default:
                    return ExperimentState.STORED;
            }
        }

        public virtual ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(TYPE_VALUE, getType());
            node.AddValue(STATE_VALUE, state);
            node.AddValue(MASS_VALUE, mass);

            return node;
        }

        public static ExperimentData getExperimentDataFromNode(ConfigNode node)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return getNullObject();
            }
            float mass = NE_Helper.GetValueAsFloat(node, MASS_VALUE);
            float cost = NE_Helper.GetValueAsFloat(node, COST_VALUE);

            ExperimentData exp = ExperimentFactory.getExperiment(node.GetValue(TYPE_VALUE), mass, cost);
            exp.load(node);
            return exp; ;
        }

        private static float stringToFloat(string p)
        {
            if (p != null)
            {
                try
                {
                    return float.Parse(p);
                }catch(FormatException){
                    return 0f;
                }
            }
            return 0f;
        }



        public static ExperimentData getNullObject()
        {
            return new ExperimentData("", "", "null Experiment", "empty", EquipmentRacks.NONE, 0f, 0f);
        }

        public virtual bool canInstall(Vessel vessel)
        {
            return false;
        }

        public virtual bool canMove(Vessel vessel)
        {
            return id != "" && (state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED ) && getFreeExperimentContainers(vessel).Count > 0;
        }

        public List<ExperimentStorage> getFreeExperimentContainers(Vessel vessel)
        {
            List<ExperimentStorage> freeCont = new List<ExperimentStorage>();
            if (contCache == null || cachedVesselID != vessel.id && partCount != vessel.parts.Count)
            {
                contCache = UnityFindObjectsOfType(typeof(ExperimentStorage)) as ExperimentStorage[];
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Storage Cache refresh");
            }

            for (int idx = 0, count = contCache.Length; idx < count; idx++)
            {
                var c = contCache[idx];
                if (c.vessel == vessel && c.isEmpty() && c.type == storageType)
                {
                    freeCont.Add(c);
                }
            }
            return freeCont;
        }

        public virtual List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            return new List<Lab>();
        }

        protected UnityEngine.Object[] UnityFindObjectsOfType(Type type)
        {
            return GameObject.FindObjectsOfType(type);
        }


        internal void installed(LabEquipment rack)
        {
            state = ExperimentState.INSTALLED;
            store = rack;
            rack.getLab().part.mass += getMass();
        }

        internal virtual void onStarted(bool started)
        {
            if(started)
            {
                state = ExperimentState.RUNNING;
                /* TODO: configuration whether to auto-create alarms */
                NE_Helper.AddExperimentAlarm( getTimeRemaining(), "NEOS Alarm", getName(), store.getPart().vessel);
                /* TODO: Save alarm ID so we can modify the alarm if the user pauses or stops the experiment. */
            }
        }

        internal void finalize()
        {
            state = ExperimentState.FINALIZED;
        }

        internal void move(Vessel vessel)
        {
            List<ExperimentStorage> targets = getFreeExperimentContainers(vessel);
            if ((state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED) && targets.Count > 0)
            {
                ChooseMoveTarget t = getMoveGuiComponent();
                t.showDialog(targets, this);
            }
        }

        private ChooseMoveTarget getMoveGuiComponent()
        {
            ChooseMoveTarget t = store.getPartGo().GetComponent<ChooseMoveTarget>();
            if (t == null)
            {
                t = store.getPartGo().AddComponent<ChooseMoveTarget>();
            }
            return t;
        }

        public void moveTo(ExperimentStorage exp)
        {
            if (state == ExperimentState.INSTALLED)
            {
                state = ExperimentState.STORED;
            }
            store.removeExperimentData();
            exp.part.mass += getMass();
            exp.storeExperiment(this);
        }

        internal void setStorage(ExperimentDataStorage storage)
        {
            store = storage;
        }

        internal virtual bool canRunAction()
        {
            return false;
        }

        internal virtual string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return Localizer.Format("#ne_Start_1", getAbbreviation());

                case ExperimentState.FINISHED:
                    return Localizer.Format("#ne_End_1", getAbbreviation());

                default:
                    return "";
            }
        }

        public virtual void runLabAction()
        {

        }

        /** Returns the status of the experiment as an english string. */
        internal virtual string stateString()
        {
            switch (state)
            {
                case ExperimentState.STORED:
                    return "Stored";
                case ExperimentState.INSTALLED:
                    return "Installed";
                case ExperimentState.RUNNING:
                    return "Running";
                case ExperimentState.FINISHED:
                    return "Finished";
                case ExperimentState.FINALIZED:
                    return "Finalized";
                default:
                    return "NullState";
            }
        }


        /** Returns the status of the experiment as a localized string. */
        internal virtual string displayStateString()
        {
            switch (state)
            {
                case ExperimentState.STORED:
                    return Localizer.GetStringByTag("#ne_Stored");
                case ExperimentState.INSTALLED:
                    return Localizer.GetStringByTag("#ne_Installed");
                case ExperimentState.RUNNING:
                    return Localizer.GetStringByTag("#ne_Running");
                case ExperimentState.FINISHED:
                    return Localizer.GetStringByTag("#ne_Finished");
                case ExperimentState.FINALIZED:
                    return Localizer.GetStringByTag("#ne_Finalized");
                default:
                    return Localizer.GetStringByTag("#ne_NullState");
            }
        }

        internal void load(LabEquipment labEquipment)
        {
            store = labEquipment;
        }

        public virtual bool isExposureExperiment(){
            return false;
        }

        internal virtual void updateCheck()
        {
        }

    }

    public class StepExperimentData : ExperimentData
    {
        protected ExperimentStep step;

        protected StepExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost)
            : base(id, type, name, abb, eq, mass, cost)
        {}

        public override ConfigNode getNode()
        {
            ConfigNode baseNode = base.getNode();
            if(step != null){
                baseNode.AddNode(step.getNode());
            }
            return baseNode;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            ConfigNode stepNode = node.GetNode(ExperimentStep.CONFIG_NODE_NAME);

            step = ExperimentStep.getExperimentStepFromConfigNode(stepNode, this);
        }

        internal override bool canRunAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return step.canStart();

                case ExperimentState.RUNNING:
                    return step.isResearchFinished();
                default:
                    return base.canRunAction();
            }
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return Localizer.Format("#ne_Start_1", getAbbreviation());

                case ExperimentState.RUNNING:
                    if (step.isResearchFinished())
                    {
                        return Localizer.Format("#ne_End_1", getAbbreviation());
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        public override void runLabAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    if (step.canStart())
                    {
                        step.start(onStarted);
                    }
                    break;
                case ExperimentState.RUNNING:
                    if (step.isResearchFinished()) {
                        step.finishStep();
                        state = ExperimentState.FINISHED;
                    }
                    break;
            }
        }

        public override bool isExposureExperiment()
        {
            return step.getNeededResource() == Resources.EXPOSURE_TIME;
        }

        /** Returns the amount of time remaining to complete this experiment step in seconds. */
        public override float getTimeRemaining()
        {
            LabEquipment le = store as LabEquipment;

            // Experiment can be finished when resources have accumulated
            // Resources accumulate in the lab
            // Remaining time is resources left / resources per hour

            // 'amount' - amount of resources required
            // 'Lab.getResourceAmount(resource)' - amount of resources acquired
            // 'Lab.ProductPerHour' - resource accumulation per hour
            //
            float amountRemaining = step.getNeededAmount() - (float)le.getResourceAmount(step.getNeededResource());
            float timeRemaining = amountRemaining / le.ProductPerHour;
            return timeRemaining * 60 * 60; /* Convert hours to seconds */
        }
    }

    public abstract class MultiStepExperimentData<T>  : ExperimentData where T : ExperimentStep
    {
        private const string ACTIVE_VALUE = "activeStep";
        protected T[] steps = null; // Implementation Class must ensure this is allocated
        private int activeStep = 0;

        protected MultiStepExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost, int numSteps)
            : base(id, type, name, abb, eq, mass, cost)
        {
            if (numSteps < 1) {
                throw new ArgumentOutOfRangeException ("numSteps", "MultiStepExperimentData must have at least 1 step.");
            }
            steps = new T[numSteps];
        }

        public override ConfigNode getNode()
        {
            ConfigNode baseNode = base.getNode();
            try {
            if (baseNode == null) {
                NE_Helper.logError ("MultiStepExperimentData.getNode() - baseNode is NULL!");
            }
                baseNode.AddValue(ACTIVE_VALUE, activeStep);

                for (int idx = 0, count = steps.Length; idx < count; idx++)
                {
                    var es = steps[idx];
                    if (es == null) {
                        NE_Helper.logError ("MultiStepExperimentData("+getId()+").getNode() - es is NULL!\n"
                            + "    entry "+(idx+1)+" in steps["+steps.Length+"] is NULL\n");
                        continue;
                    }
                    ConfigNode expNode = es.getNode ();
                    if (expNode == null) {
                        NE_Helper.logError ("MultiStepExperimentData.getNode() - expNode is NULL!");
                        continue;
                    }
                    baseNode.AddNode(es.getNode());
                }
            } catch(NullReferenceException nre) {
                NE_Helper.logError ("MultiStepExperimentData.getNode - NullReferenceException:\n"
                                    + nre.StackTrace);
            }
            return baseNode;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);

            activeStep = NE_Helper.GetValueAsInt(node, ACTIVE_VALUE);

            ConfigNode[] stepNodes = node.GetNodes(ExperimentStep.CONFIG_NODE_NAME);
            steps = new T[stepNodes.Length];
            for (int idx = 0, count = stepNodes.Length; idx < count; idx++)
            {
                var stepNode = stepNodes[idx];
                T step = (T)ExperimentStep.getExperimentStepFromConfigNode(stepNode, this);
                steps[step.getIndex()] = step;
            }
        }

        internal override bool canRunAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return steps[activeStep].canStart();

                case ExperimentState.RUNNING:
                    return steps[activeStep].isResearchFinished();
                default:
                    return base.canRunAction();
            }
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return Localizer.Format("#ne_Start_1_step_2", getAbbreviation(), steps[activeStep].getName());


                case ExperimentState.RUNNING:
                    if (steps[activeStep].isResearchFinished())
                    {
                        return Localizer.Format("#ne_End_1_step_2", getAbbreviation(), steps[activeStep].getName());
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        internal T getActiveStep()
        {
            return steps[activeStep];
        }

        internal List<T> getExperimentSteps()
        {
            return new List<T>(steps);
        }

        internal int getActiveStepIndex()
        {
            return activeStep;
        }

        public override void runLabAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    if(steps[activeStep].canStart()){
                        steps[activeStep].start(onStarted);
                    }
                    break;
                case ExperimentState.RUNNING:
                    if (steps[activeStep].isResearchFinished())
                    {
                        steps[activeStep].finishStep();
                        if (isLastStep())
                        {
                            state = ExperimentState.FINISHED;
                        }
                        else
                        {
                            state = ExperimentState.INSTALLED;
                            activeStep++;
                        }
                    }
                    break;
            }
        }

        private bool isLastStep()
        {
            return activeStep == (steps.Length - 1);
        }

        public override bool isExposureExperiment()
        {
            return steps[activeStep].getNeededResource() == Resources.EXPOSURE_TIME;
        }

        /** Returns the amount of time remaining to complete this experiment step in seconds. */
        public override float getTimeRemaining()
        {
            LabEquipment le = store as LabEquipment;

            // Experiment can be finished when resources have accumulated
            // Resources accumulate in the lab
            // Remaining time is resources left / resources per hour

            // 'amount' - amount of resources required
            // 'Lab.getResourceAmount(resource)' - amount of resources acquired
            // 'Lab.ProductPerHour' - resource accumulation per hour
            //
            float amountRemaining = steps[activeStep].getNeededAmount() - (float)le.getResourceAmount(steps[activeStep].getNeededResource());
            float timeRemaining = amountRemaining / le.ProductPerHour;
            return timeRemaining * 60 * 60; /* Convert hours to seconds */
        }


    }

    public class TestExperimentData : KerbalResearchExperimentData
    {
        public TestExperimentData(float mass, float cost)
            : base("NE_Test", "Test", "Test Experiment", "Test", EquipmentRacks.USU, mass, cost, 4)
        {
            steps[0] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 0);
            steps[1] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 1);
            steps[2] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 2);
            steps[3] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 3);
        }

    }
}
