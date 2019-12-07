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
    /// <summary>
    /// The different types of LabEquipment available in NEOS.
    /// </summary>
    /// TODO: Discover dynamically. This is the abbreviation (currently
    /// localised) defined in the LabEquipmentModule in the Part Definition
    /// for each LabEquipment.
    public enum LabEquipmentType
    {
        CIR, FIR, PRINTER, EXPOSURE, MSG, USU, KEMINI, NONE
    }

    /// <summary>
    /// A registry of LabEquipment which can convert between LabEquipmentType and actual LabEquipment parts.
    /// </summary>
    public class LabEquipmentRegistry
    {
        static readonly KeyValuePair<LabEquipmentType, String>[] racks = 
        {
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.PRINTER, "NE.3PR"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.CIR, "NE.CIR"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.FIR, "NE.FIR"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.MSG, "NE.MSG"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.USU, "NE.USU"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.EXPOSURE, "MEP"),
            new KeyValuePair<LabEquipmentType, String>(LabEquipmentType.KEMINI, "NE.KEMINI"),
        };

        /// <summary>
        /// Maps a string to a LabEquipmentType
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static LabEquipmentType getType(string p)
        {
            LabEquipmentType type;
            if (Enum.TryParse(p, true, out type) == false) {
                type = LabEquipmentType.NONE;
            }
            return type;
        }

        /// <summary>
        /// Maps a LabEquipmentType to actual LabEquipment
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public static LabEquipment getLabEquipmentForType(LabEquipmentType er)
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

        /// <summary>
        /// Returns a list of available (researched) LabEquipment
        /// </summary>
        /// <returns></returns>
        public static List<LabEquipment> getAvailableLabEquipment()
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

        /// <summary>
        /// Returns the LabEquipment attached to a KSP Part.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static LabEquipment getLabEquipment(Part part, LabEquipmentType type)
        {
            LabEquipmentModule lem = part.GetComponent<LabEquipmentModule>();
            float mass = part.partInfo.partPrefab.mass;
            float cost = part.partInfo.cost;
            return new LabEquipment(lem.abbreviation, lem.eqName, type, mass, cost, lem.productPerHour, lem.product, lem.reactantPerProduct, lem.reactant);
        }
    }

}
