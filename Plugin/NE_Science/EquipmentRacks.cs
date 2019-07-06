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

namespace NE_Science
{
    public class EquipmentRackRegistry
    {
        static readonly KeyValuePair<EquipmentRacks, String>[] racks = 
        {
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.PRINTER, "NE.3PR"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.CIR, "NE.CIR"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.FIR, "NE.FIR"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.MSG, "NE.MSG"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.USU, "NE.USU"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.EXPOSURE, "MEP"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.KEMINI, "NE.KEMINI"),
        };

        public static LabEquipment getLabEquipmentForRack(EquipmentRacks er)
        {
            LabEquipment le = null;

            for (int idx = 0, count = racks.Length; idx < count; idx++)
            {
                if(racks[idx].Key == er)
                {
                    AvailablePart part = PartLoader.getPartInfoByName(racks[idx].Value);
                    if (part != null)
                    {
                        le = getLabEquipment(part.partPrefab, er);
                    }
                    break;
                }
            }

            return le;
        }

        public static List<LabEquipment> getAvailableRacks()
        {
            List<LabEquipment> list = new List<LabEquipment>();
            for (int idx = 0, count = racks.Length; idx < count; idx++)
            {
                var p = racks[idx];
                AvailablePart part = PartLoader.getPartInfoByName(p.Value);
                if (part != null && ResearchAndDevelopment.PartModelPurchased(part))
                {
                    list.Add(getLabEquipment(part.partPrefab, p.Key));
                }
            }
            return list;
        }

        private static LabEquipment getLabEquipment(Part part, EquipmentRacks type)
        {
            LabEquipmentModule lem = part.GetComponent<LabEquipmentModule>();
            float mass = part.partInfo.partPrefab.mass;
            float cost = part.partInfo.cost;
            return new LabEquipment(lem.abbreviation, lem.eqName, type, mass, cost, lem.productPerHour, lem.product, lem.reactantPerProduct, lem.reactant);
        }
    }

    public enum EquipmentRacks
    {
        CIR, FIR, PRINTER, EXPOSURE, MSG, USU, KEMINI, NONE
    }

    public class EquipmentRacksFactory{

        public static EquipmentRacks getType(string p)
        {
            switch (p)
            {
                case "FIR":
                    return EquipmentRacks.FIR;
                case "CIR":
                    return EquipmentRacks.CIR;
                case "PRINTER":
                    return EquipmentRacks.PRINTER;
                case "EXPOSURE":
                    return EquipmentRacks.EXPOSURE;
                case "MSG":
                    return EquipmentRacks.MSG;
                case "USU":
                    return EquipmentRacks.USU;
                case "KEMINI":
                    return EquipmentRacks.KEMINI;
                default:
                    return EquipmentRacks.NONE;
            }
        }
    }
}
