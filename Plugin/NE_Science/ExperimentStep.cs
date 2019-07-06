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
    public class ExperimentStep
    {
        public const string CONFIG_NODE_NAME = "NE_ExperimentStep";
        private const string TYPE_VALUE = "Type";
        private const string NAME_VALUE = "Name";
        private const string INDEX_VALUE = "INDEX";

        protected ExperimentData exp;

        private string type = "";
        private string name = "";
        private int index = 0;

        protected ExperimentStep(ExperimentData exp, string type, string name, int index = 0)
        {
            this.exp = exp;
            this.type = type;
            this.name = name;
            this.index = index;
        }

        public virtual bool ready()
        {
            return false;
        }

        public string getName(){
            return name;
        }

        public int getIndex()
        {
            return index;
        }

        public virtual bool isResearchFinished()
        {
            return false;
        }

        public delegate void startCallback(bool started);
        public virtual void start(startCallback cbMethod){
            cbMethod(false);
        }

        public virtual void finishStep()
        {
        }


        public virtual ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(TYPE_VALUE, getType());
            node.AddValue(NAME_VALUE, getName());
            node.AddValue(INDEX_VALUE, getIndex());
            return node;
        }

        public static ExperimentStep getExperimentStepFromConfigNode(ConfigNode node, ExperimentData exp)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getExperimentStepFromConfigNode: invalid Node: " + node.name);
                return new ExperimentStep(exp, "", "");
            }
            int index = NE_Helper.GetValueAsInt(node, INDEX_VALUE);
            string name = node.GetValue(NAME_VALUE);
            ExperimentStep step = createExperimentStep(node.GetValue(TYPE_VALUE), exp, name, index);
            step.load(node);
            return step;

        }

        protected virtual void load(ConfigNode node)
        {
        }

        private static ExperimentStep createExperimentStep(string type, ExperimentData exp, string name, int index)
        {
            switch (type)
            {
                case "ResStep":
                    return new ResourceExperimentStep(exp, name, index);
                case "MEPResStep":
                    return new MEPResourceExperimentStep(exp, name, index);
                case "KerbalResStep":
                    return new KerbalResearchStep(exp, name, index);
                default:
                    return new ExperimentStep(exp, "", name, index);
            }
        }

        protected string getType()
        {
            return type;
        }

        internal virtual bool canStart()
        {
            return exp.state == ExperimentState.INSTALLED && !OMSExperiment.checkBoring(exp.store.getPart().vessel);
        }

        public virtual string getNeededResource()
        {
            return "";
        }

        public virtual float getNeededAmount()
        {
            return 0;
        }

        public virtual EquipmentRacks getNeededEquipment()
        {
            return exp.getEquipmentNeeded();
        }
    }

    public class ResourceExperimentStep : ExperimentStep
    {
        private const string RES_VALUE = "Res";
        private const string AMOUNT_VALUE = "Amount";

        protected string res;
        protected float amount;

        internal ResourceExperimentStep(ExperimentData exp, string name, int index = 0)
            : this(exp, "ResStep", name, index)
        { }

        internal ResourceExperimentStep(ExperimentData exp, string type, string name, int index = 0)
            : base(exp, type, name, index)
        { }

        public ResourceExperimentStep(ExperimentData exp, string res, float amount, string name, int index = 0)
            : base(exp, "ResStep", name, index)
        {
            this.res = res;
            this.amount = amount;
        }

        internal ResourceExperimentStep(ExperimentData exp, string res, float amount, string type, string name, int index = 0)
            : base(exp, type, name, index)
        {
            this.res = res;
            this.amount = amount;
        }

        public override ConfigNode getNode()
        {
            ConfigNode node =  base.getNode();
            node.AddValue(RES_VALUE, res);
            node.AddValue(AMOUNT_VALUE, amount);
            return node;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            res = node.GetValue(RES_VALUE);
            amount = NE_Helper.GetValueAsFloat(node, AMOUNT_VALUE);
        }

        public override bool ready()
        {
            return exp.state == ExperimentState.INSTALLED;
        }

        public override bool isResearchFinished()
        {
            double numTestPoints = ((LabEquipment)exp.store).getResourceAmount(res);
            return Math.Round(numTestPoints, 2) >= Math.Round(amount, 2);
        }

        public override void start(startCallback cbMethod)
        {
            NE_Helper.log("ResExppStep.start()");
            if(canStart()){
                Lab lab = ((LabEquipment)exp.store).getLab();
                if (lab != null && !OMSExperiment.checkBoring(lab.vessel, true))
                {
                    NE_Helper.log("ResExppStep.start(): create Resource");
                    ((LabEquipment)exp.store).createResourceInLab(res, amount);
                    cbMethod(true);
                    return;
                }
                else
                {
                    NE_Helper.logError("ResExppStep.start(): Lab null or boring. Boring: " + OMSExperiment.checkBoring(lab.vessel, true));
                }
            }
            NE_Helper.log("ResExppStep.start(): can NOT start");
            cbMethod(false);
        }

        public override void finishStep()
        {
            if (exp.state == ExperimentState.RUNNING && isResearchFinished())
            {
                ((LabEquipment)exp.store).setResourceMaxAmount(res, 0f);
            }
        }

        public override string getNeededResource()
        {
            return res;
        }

        public override float getNeededAmount()
        {
            return amount;
        }
    }

    public class MEPResourceExperimentStep : ResourceExperimentStep
    {
        internal MEPResourceExperimentStep(ExperimentData exp, string name, int index = 0)
            : base(exp, "MEPResStep", name, index)
        { }

        public MEPResourceExperimentStep(ExperimentData exp, string res, float amount, string name, int index)
            : base(exp, res, amount, "MEPResStep", name, index)
        {
        }

        internal override bool canStart()
        {
            if(base.canStart()){
                return ((MEP_Module)((LabEquipment)exp.store).getLab()).MEPlabState == MEPLabStatus.READY;
            }

            return false; ;
        }
    }
}
