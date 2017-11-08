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
    class KEES_PEC : PartModule
    {

        [KSPField(isPersistant = false)]
        public string nodeName = "top";

        [KSPField(isPersistant = false)]
        public double maxGforce = 2.5;

        private AttachNode node = null;
        private KEESExperiment exp = null;

        private int counter = 0;

        [KSPField(isPersistant = true)]
        public bool decoupled = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            node = part.FindAttachNode(nodeName);
            if (node == null)
            {
                NE_Helper.logError("KEES PEC: AttachNode not found");
                node = part.attachNodes[0];
            }
        }

        private void checkForExp()
        {
            if (node != null && node.attachedPart != null)
            {
                KEESExperiment newExp = node.attachedPart.GetComponent<KEESExperiment>();
                if (newExp != null)
                {
                    if (exp == null)
                    {
                        exp = newExp;
                        exp.dockedToPEC(true);
                        NE_Helper.log("New KEES Experiment installed");
                    }
                    else if (exp != newExp)
                    {
                        exp.dockedToPEC(false);
                        exp = newExp;
                        exp.dockedToPEC(true);
                        NE_Helper.log("KEES Experiment switched");
                    }
                }
                else if (exp != null)
                {
                    exp.dockedToPEC(false);
                    NE_Helper.log("KEES Experiment undocked");
                    exp = null;
                }
            }

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            /* Only perform the max-G check if we are attached to a vessel.
             * During KAS grab, vessel can be itself or a Kerbal, and we may
             * get spurious high G's. */
            bool isVesselShip = part.parent != null && vessel != null && !vessel.isEVA;
            if (decoupled && vessel.vesselType != VesselType.Debris)
            {
                NE_Helper.log("Decoupled PEC recoverd");
                decoupled = false;
            }
            if (!decoupled && isVesselShip && vessel.geeForce > maxGforce)
            {
                NE_Helper.log ("KEES PEC over max G, decouple\n" + this.ToString ());
                decouple();
            }
            //Decouple for testing
            if (NE_Helper.debugging() && Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.D))
            {
                decouple();
            }
            if (!decoupled && counter == 0)//don't run this every frame
            {
                checkForExp();
            }
            counter = (++counter) % 6;
        }

        private void decouple()
        {
            decoupled = true;
            part.decouple();
            if (exp != null)
            {
                exp.pecDecoupled();
            }
            exp = null;
        }

        #if (DEBUG)
        [KSPEvent(guiActive = true, guiName = "Debug Dump", active = true)]
        public void DebugDump()
        {
            /* Printed out in Player.log */
            NE_Helper.log(this.ToString ());
        }
        #endif

        /** Converts the object to a human-readble string suitable for printing.
        /** Converts the object to a human-readable string suitable for printing.
         * Overloads base-class implementation.
         */
        new public String ToString()
        {
            String ret = base.ToString () + "\n";
            ret += "\tnode:               " + node + "\n";
            ret += "\texp:                " + exp + "\n";
            ret += "\tdecoupled:          " + decoupled + "\n";
            ret += "\tpart:               " + part + "\n";
            ret += "\tpart.parent:        " + part.parent + "\n";
            ret += "\tvessel:             " + vessel + "\n";
            ret += "\tvessel.isEva:       " + vessel.isEVA + "\n";
            ret += "\tvessel.geeForce:    " + vessel.geeForce + "\n";
            ret += "\tmaxGforce:          " + maxGforce + "\n";
            return ret;
        }
    }
}
