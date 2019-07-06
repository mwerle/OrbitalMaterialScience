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
    class ResourceHelper
    {
        public static PartResource getResource(Part part, string name)
        {
            PartResourceList resourceList = part.Resources;
            return resourceList.Get(name);
        }

        public static double getResourceAmount(Part part, string name)
        {
            PartResource res = getResource(part, name);
            if (res == null)
            {
                return 0;
            }
            return res.amount;
        }

        public static bool setResourceAmount(Part part, string name, double amount)
        {
            PartResource res = getResource(part, name);
            if (res == null)
            {
                return false;
            }
            res.amount = amount;
            return true;
        }

        public static PartResource setResourceMaxAmount(Part part, string name, double max)
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

        public static double getResourceDensity(string name)
        {
            var resDef = PartResourceLibrary.Instance.resourceDefinitions["Bioproducts"];
            if (resDef != null)
                return resDef.density;
            return 0;
        }

        public static double getDemand(Part part, string name)
        {
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            double amount;
            double maxAmount;
            part.vessel.GetConnectedResourceTotals(res_def.id, out amount, out maxAmount, false);

            return amount;
        }

        /** Returns the total amount of available resources connected to the current part */
        public static double getAvailable(Part part, string name)
        {
            var res_def = PartResourceLibrary.Instance.GetDefinition(name);
            if (res_def == null) return 0;
            double amount;
            double maxAmount;
            //part.GetConnectedResourceTotals(res_def.id, res_def.resourceFlowMode, out amount, out maxAmount, false);
            part.vessel.GetConnectedResourceTotals(res_def.id, out amount, out maxAmount, true);

            return amount;
        }

        public static double requestResourcePartial(Part part, string name, double amount)
        {
            if (amount > 0)
            {
                //NE_Helper.log(name + " request: " + amount);
                double taken = part.RequestResource(name, amount);
                //NE_Helper.log(name + " request taken: " + taken);
                if (taken >= amount * .99999)
                    return taken;
                double available = getAvailable(part, name);
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
                double available = getDemand(part, name);
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