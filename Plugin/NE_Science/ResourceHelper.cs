/*
    This file was orgininally part of Station Science by ethernet http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models.

    Station Science is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Station Science is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Station Science.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace NE_Science
{
    static class ResourceHelper
    {
        /// <summary>
        /// Returns a named Resource if the Part has it.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PartResource getResource(this Part part, string name)
        {
            PartResourceList resourceList = part.Resources;
            return resourceList.Get(name);
        }

        /// <summary>
        /// Returns the amount of a named Resource if the Part has it.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static double getResourceAmount(this Part part, string name)
        {
            PartResource res = getResource(part, name);
            if (res == null)
            {
                return 0;
            }
            return res.amount;
        }

        /// <summary>
        /// Sets the amount of a Resource in a Part, if the Part has the Resource.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool setResourceAmount(this Part part, string name, double amount)
        {
            PartResource res = getResource(part, name);
            if (res == null)
            {
                return false;
            }
            res.amount = amount;
            return true;
        }

        /// <summary>
        /// Sets the maximum amount of a Resource in a Part, adding the Resource if necessary.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static PartResource setResourceMaxAmount(this Part part, string name, double max)
        {
            PartResource res = getResource(part, name);
            if (res == null && max > 0)
            {
                ConfigNode node = new ConfigNode("RESOURCE");
                node.AddValue("name", name);
                node.AddValue("amount", 0);
                node.AddValue("maxAmount", max);
                res = part.Resources.Add (node);
            }
            else if (res != null && max > 0)
            {
                res.maxAmount = max;
            }
            else if (res != null && max <= 0)
            {
                part.Resources.Remove (res);
            }
            return res;
        }

        /// <summary>
        /// Returns the total Demand of a Resource connected to the current Part.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static double getResourceDemand(this Part part, string name)
        {
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            double amount;
            double maxAmount;
            part.vessel.GetConnectedResourceTotals(res_def.id, out amount, out maxAmount, false);

            return amount;
        }

        /// <summary>
        /// Returns the total amount of available resources connected to the current Part.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static double getResourceAvailable(this Part part, string name)
        {
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            double amount;
            double maxAmount;
            //part.GetConnectedResourceTotals(res_def.id, res_def.resourceFlowMode, out amount, out maxAmount, false);
            part.vessel.GetConnectedResourceTotals(res_def.id, out amount, out maxAmount, true);

            return amount;
        }

        public static double requestResourcePartial(this Part part, string name, double amount)
        {
            if (amount > 0)
            {
                //NE_Helper.log(name + " request: " + amount);
                double taken = part.RequestResource(name, amount);
                //NE_Helper.log(name + " request taken: " + taken);
                if (taken >= amount * .99999)
                    return taken;
                double available = getResourceAvailable(part, name);
                //NE_Helper.log(name + " request available: " + available);
                double new_amount = Math.Min(amount, available) * .99999;
                //NE_Helper.log(name + " request new_amount: " + new_amount);
                if (new_amount > taken)
                    return taken + part.RequestResource(name, new_amount - taken);
                else
                    return taken;
            }
            else if (amount < 0)
            {
                //NE_Helper.log(name + " request: " + amount);
                double taken = part.RequestResource(name, amount);
                //NE_Helper.log(name+" request taken: " + taken);
                if (taken <= amount * .99999)
                    return taken;
                double available = getResourceDemand(part, name);
                //NE_Helper.log(name + " request available: " + available);
                double new_amount = Math.Max(amount, available) * .99999;
                //NE_Helper.log(name + " request new_amount: " + new_amount);
                if (new_amount < taken)
                    return taken + part.RequestResource(name, new_amount - taken);
                else
                    return taken;
            }
            else
                return 0;
        }
    }
}
