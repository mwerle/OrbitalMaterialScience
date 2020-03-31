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

using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
     /// <summary>
     /// Module used to add Lab Equipment to the Tech tree. 
     /// </summary>
     /// This is the actual PartModule defined in the Part Definition for each
     /// LabEquipment. It is used to display the LabEquipment in the Tech Tree.
    public class LabEquipmentModule : PartModule
    {

        [KSPField(isPersistant = false)]
        public string abbreviation = "";

        [KSPField(isPersistant = false)]
        public string eqName = "";

        [KSPField(isPersistant = true)]
        public float productPerHour = 0;
        [KSPField(isPersistant = false)]
        public string product = "";

        [KSPField(isPersistant = true)]
        public float reactantPerProduct = 0;
        [KSPField(isPersistant = false)]
        public string reactant = "";

    }

     /// <summary>
     /// The actual LabEquipment as used in this mod.
     /// </summary>
     /// LabEquipment is a "virtual Part" in that the user cannot directly manipulate the equipment.
     /// It is either stored in a EquipmentRackContainer or installed in a LabEquipmentSlot inside
     /// a Lab.
     ///
     /// LabEquipment can have experiments (ExperimentData) installed in them in order to run the
     /// experiment.
     /// 
     /// LabEquipment will produce a Product using a Reactants. When running an experiment, Product
     /// is accumulated until the experiment is completed.
    public class LabEquipment : IExperimentDataStorage
    {
        public const string CONFIG_NODE_NAME = "NE_LabEquipment";
        private const string ABB_VALUE = "abb";
        private const string NAME_VALUE = "name";
        private const string TYPE_VALUE = "type";
        private const string MASS_VALUE = "mass";
        private const string COST_VALUE = "cost";
        private const string PRODUCT_VALUE = "product";
        private const string PRODUCT_PER_HOUR_VALUE = "productPerHour";
        private const string REACTANT_VALUE = "reactant";
        private const string REACTANT_PER_PRODUCT_VALUE = "reactantPerProduct";

        private string abb;
        private string name;
        private float mass;
        private float cost;
        private LabEquipmentType type;

        private float productPerHour = 0;
        private string product = "";

        private float reactantPerProduct = 0;
        private string reactant = "";

        private Generator gen;

        private Lab lab;
        private ExperimentData exp;

        public LabEquipment(string abb, string name, LabEquipmentType type, float mass, float cost, float productPerHour, string product, float reactantPerProduct, string reactant)
        {
            this.abb = abb;
            this.name = name;
            this.type = type;
            this.mass = mass;
            this.cost = cost;

            this.product = product;
            this.productPerHour = productPerHour;

            this.reactant = reactant;
            this.reactantPerProduct = reactantPerProduct;
        }

        /// <summary>
        /// The abbreviation for this Equipment.
        /// </summary>
        public string Abbreviation { get { return abb; } }

        /// <summary>
        /// The name for this Equipment.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// The EquipmentType of this Equipment.
        /// </summary>
        public LabEquipmentType LabEquipmentType { get { return type; } }

        /// <summary>
        /// The Product which this Lab produces
        /// </summary>
        public string Product
        {
            get { return product; }
        }

        /// <summary>
        /// How many units of Product the lab generates per hour.
        /// </summary>
        public float ProductPerHour
        {
            get { return productPerHour; }
        }

        /// <summary>
        /// The Reactant which this Lab requires
        /// </summary>
        public string Reactant
        {
            get { return reactant; }
        }

        /// <summary>
        /// How many units of Reactant the lab requires per unit of Product.
        /// </summary>
        public float ReactantPerProduct
        {
            get { return reactantPerProduct; }
        }

        /// <summary>
        /// Gets the mass of the equipment plus installed experiments.
        /// </summary>
        /// <returns>The mass.</returns>
        public float getMass()
        {
            return mass + ((exp != null)? exp.getMass() : 0f);
        }

        /// <summary>
        /// Gets the cost of the equipment plus installed experiments.
        /// </summary>
        /// <returns>The cost.</returns>
        public float getCost()
        {
            return cost + ((exp != null)? exp.getCost() : 0f);
        }

        private static LabEquipment nullObject = null;
        static public LabEquipment getNullObject()
        {
            if (nullObject == null)
            {
                nullObject = new LabEquipment("empty", "empty", LabEquipmentType.NONE, 0f, 0f, 0f, "", 0f, "");
            }
            return nullObject;
        }

        public ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(ABB_VALUE, abb);
            node.AddValue(NAME_VALUE, name);
            node.AddValue(MASS_VALUE, mass);
            node.AddValue(COST_VALUE, cost);
            node.AddValue(TYPE_VALUE, type.ToString());

            node.AddValue(PRODUCT_VALUE, product);
            node.AddValue(PRODUCT_PER_HOUR_VALUE, productPerHour);

            node.AddValue(REACTANT_VALUE, reactant);
            node.AddValue(REACTANT_PER_PRODUCT_VALUE, reactantPerProduct);

            if (exp != null)
            {
                node.AddNode(exp.getNode());
            }

            return node;
        }

        public static LabEquipment getLabEquipmentFromNode(ConfigNode node, Lab lab)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return getNullObject();
            }

            string abb = node.GetValue(ABB_VALUE);
            string name = node.GetValue(NAME_VALUE);
            float mass = node.GetFloat(MASS_VALUE);
            float cost = node.GetFloat(COST_VALUE);

            string product = node.GetValue(PRODUCT_VALUE);
            float productPerHour = node.GetFloat(PRODUCT_PER_HOUR_VALUE);

            string reactant = node.GetValue(REACTANT_VALUE);
            float reactantPerProduct = node.GetFloat(REACTANT_PER_PRODUCT_VALUE);

            LabEquipmentType type = LabEquipmentRegistry.getType(node.GetValue(TYPE_VALUE));

            // Backwards compatibility for save games prior to NEOS 0.9
            // TODO: Remove sometime in the future
            if(type == LabEquipmentType.KEMINI)
            {
                product = Resources.KEMINI_LAB_TIME;
            }

            LabEquipment eq = new LabEquipment(abb, name, type, mass, cost, productPerHour, product, reactantPerProduct, reactant);
            eq.lab = lab;
            ConfigNode expNode = node.GetNode(ExperimentData.CONFIG_NODE_NAME);
            if (expNode != null)
            {
                eq.loadExperiment(ExperimentData.getExperimentDataFromNode(expNode));
            }

            return eq;
        }

        private void loadExperiment(ExperimentData experimentData)
        {
            this.exp = experimentData;
            exp.load(this);
            GameObject ego = lab.getExperimentGO(exp.getId());
            if (ego != null)
            {
                ego.SetActive(true);
            }
        }

        public bool isRunning()
        {
            if (gen != null)
            {
                double last = gen.rates[product].last_produced;
                bool state = (last < -0.0000001);
                return state;
            }
            return false;
        }

        public void install(Lab lab)
        {
            NE_Helper.log("Lab equipment install in " + lab.abbreviation);
            gen = createGenerator(product, productPerHour, reactant, reactantPerProduct, lab);
            lab.addGenerator(gen);
            this.lab = lab;
        }

        private Generator createGenerator(string resToCreate, float creationRate, string useRes, float usePerUnit, Lab lab)
        {
            Generator gen = new Generator(lab.part);
            gen.addRate(resToCreate, -creationRate);
            if (usePerUnit > 0)
                gen.addRate(useRes, usePerUnit);
            return gen;
        }

        internal bool isExperimentSlotFree()
        {
            return exp == null;
        }

        internal void installExperiment(ExperimentData exp)
        {
            this.exp = exp;
            exp.onInstalled(this);
            GameObject ego = lab.getExperimentGO(exp.getId());
            if (ego != null)
            {
                ego.SetActive(true);
            }
        }

        internal ExperimentData getExperiment()
        {
            return exp;
        }

        /// <summary>
        /// Returns whether the Experiment could be moved.
        /// </summary>
        /// Typically this checks the experiment state and whether there are any empty destinations in the Vessel.
        /// <param name="vessel"></param>
        /// <returns></returns>
        internal bool canExperimentMove(Vessel vessel)
        {
            if (exp != null)
            {
                return exp.canMove(vessel);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Action to try to move an Experiment to another ExperimentStorage in the Vessel.
        /// </summary>
        /// At this stage the move may fail and the experiment may remain in the LabEquipment.
        /// <param name="vessel"></param>
        internal void tryMoveExperiment(Vessel vessel)
        {
            if (exp != null)
            {
                exp.move(vessel);
            }
        }

        /// <summary>
        /// Action to remove an Experiment from this LabEquipment.
        /// </summary>
        /// This action actually removes an Experiment from the LabEquipment.
        public void removeExperimentData()
        {
            lab.OnExperimentWillRemoveFromEquipment(this);
            exp = null;
        }

        public GameObject getPartGo()
        {
            return lab.part.gameObject;
        }

        /// <summary>
        /// Returns the Part the LabEquipment is currently in.
        /// </summary>
        /// <returns></returns>
        public Part getPart()
        {
            return lab.part;
        }

        internal void createResourceInLab(string res, float amount)
        {
            lab.setResourceMaxAmount(res, amount);
        }

        internal double getResourceAmount(string res)
        {
            return lab? lab.getResourceAmount(res) : 0.0;
        }

        internal bool canRunExperimentAction()
        {
            if (exp != null)
            {
                return exp.canRunAction();
            }
            else
            {
                return false;
            }
        }

        internal string getActionString()
        {
            if (exp != null)
            {
                return exp.getActionString();
            }
            else
            {
                return "";
            }
        }

        internal void experimentAction()
        {
            if (exp != null)
            {
                exp.runLabAction();
            }
        }

        internal void setResourceMaxAmount(string res, float p)
        {
            NE_Helper.log("Set AmountTo: " + p);
            lab.setResourceMaxAmount(res, p);
        }

        internal Lab getLab()
        {
            return lab;
        }

        internal bool isExposureAction()
        {
            if (exp != null)
            {
                return exp.isExposureExperiment();
            }
            else
            {
                return false;
            };
        }

        internal void updateCheck()
        {
            if (exp != null)
            {
                exp.updateCheck();
            }
        }

        internal string getDescription()
        {
            string desc = "<b>" + name +" (" + abb + ")</b>\n";
            switch (type)
            {
                case LabEquipmentType.CIR:
                case LabEquipmentType.FIR:
                case LabEquipmentType.PRINTER:
                    desc +=  Localizer.Format("#ne_For_1", "MSL-1000");
                    break;
                case LabEquipmentType.MSG:
                case LabEquipmentType.USU:
                    desc +=  Localizer.Format("#ne_For_1", "MPL-600");
                    break;
            }
            return desc;
        }
    }
}
