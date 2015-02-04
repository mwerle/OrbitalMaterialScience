﻿/*
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
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class LabEquipmentSlot
    {
        public const string CONFIG_NODE_NAME = "NE_LabEquipmentSlot";
        private const string TYPE_VALUE = "type";

        private EquipmentRacks type;
        private LabEquipment equ;

        public LabEquipmentSlot(EquipmentRacks t, LabEquipment e = null)
        {
            type = t;
            if (e != null && type == e.getType())
            {
                equ = e;
            }
        }

        public bool isEquipmentInstalled()
        {
            return equ != null;
        }

        public bool isEquipmentRunning()
        {
            if (isEquipmentInstalled())
            {

                return equ.isRunning();
            }
            else
            {
                return false;
            }
        }

        public void install(LabEquipment eq, Lab lab)
        {
            if (eq != null && type == eq.getType())
            {
                equ = eq;
                eq.install(lab);
            }
            else
            {
                NE_Helper.logError("LabEquipmentSlot.install: Type doesn't macht");
            }
        }

        internal void onStart(Lab lab)
        {
            if (equ != null)
            {
                equ.install(lab);
            }
        }

        public ConfigNode getConfigNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(TYPE_VALUE, type);
            if (equ != null)
            {
                node.AddNode(equ.getNode());
            }

            return node;
        }

        public static LabEquipmentSlot getLabEquipmentSlotFromConfigNode(ConfigNode node)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return new LabEquipmentSlot(EquipmentRacks.NONE);
            }
            EquipmentRacks type = EquipmentRacksFactory.getType(node.GetValue(TYPE_VALUE));
            LabEquipment le = null;
            ConfigNode leNode = node.GetNode(LabEquipment.CONFIG_NODE_NAME);
            if (leNode != null)
            {
                le = LabEquipment.getLabEquipmentFromNode(leNode);
            }
            return new LabEquipmentSlot(type, le);
        }
    }
}
