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
    /*
     *Module used to add Lab Equipment to the Tech tree. 
     */
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

    /*
     * Class used to add LabEquipment to Containers
     */
    public class LabEquipment : ExperimentDataStorage
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
        private EquipmentRacks type;

        private float productPerHour = 0;
        private string product = "";

        private float reactantPerProduct = 0;
        private string reactant = "";

        private Generator gen;

        private Lab lab;
        private ExperimentData exp;

        public LabEquipment(string abb, string name, EquipmentRacks type, float mass, float cost, float productPerHour, string product, float reactantPerProduct, string reactant)
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

        public string getAbbreviation()
        {
            return abb;
        }

        public string getName()
        {
            return name;
        }

        public EquipmentRacks getType()
        {
            return type;
        }

        /** How many units of Product the lab generates per hour. */
        public float ProductPerHour
        {
            get { return productPerHour; }
        }

        /** How many units of Reactant the lab requires per unit of Product. */
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

        static public LabEquipment getNullObject()
        {
             return new LabEquipment("empty", "empty", EquipmentRacks.NONE, 0f, 0f, 0f, "", 0f, "");
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
            float mass = NE_Helper.GetValueAsFloat(node, MASS_VALUE);
            float cost = NE_Helper.GetValueAsFloat(node, COST_VALUE);

            string product = node.GetValue(PRODUCT_VALUE);
            float productPerHour = NE_Helper.GetValueAsFloat(node, PRODUCT_PER_HOUR_VALUE);

            string reactant = node.GetValue(REACTANT_VALUE);
            float reactantPerProduct = NE_Helper.GetValueAsFloat(node, REACTANT_PER_PRODUCT_VALUE);

            EquipmentRacks type = EquipmentRacksFactory.getType(node.GetValue(TYPE_VALUE));

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
            exp.installed(this);
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

        internal void moveExperiment(Vessel vessel)
        {
            if (exp != null)
            {
                exp.move(vessel);
            }
            GameObject ego = lab.getExperimentGO(exp.getId());
            if (ego != null)
            {
                ego.SetActive(false);
            }
        }

        public void removeExperimentData()
        {
            exp = null;
        }

        public GameObject getPartGo()
        {
            return lab.part.gameObject;
        }

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
            string desc = "<b>" + getName() +" (" + getAbbreviation()+ ")</b>\n";
            switch (type)
            {
                case EquipmentRacks.CIR:
                case EquipmentRacks.FIR:
                case EquipmentRacks.PRINTER:
                    desc +=  Localizer.Format("#ne_For_1", "MSL-1000");
                    break;
                case EquipmentRacks.MSG:
                case EquipmentRacks.USU:
                    desc +=  Localizer.Format("#ne_For_1", "MPL-600");
                    break;
            }
            return desc;
        }
    }
}
