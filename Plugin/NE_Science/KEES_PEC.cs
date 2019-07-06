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

        [KSPField(isPersistant = true)]
        public bool decoupled = false;

        [KSPField(isPersistant = false)]
        public double maxGforce = 2.5;

        protected AttachNode node
        {
            get
            {
                AttachNode node = part.FindAttachNode("top");
                if (node == null)
                {
                    NE_Helper.logError("KEES PEC: AttachNode not found");
                    node = part.attachNodes[0];
                }
                return node;
            }
        }

        public Part attachedPart
        {
            get
            {
                return node?.attachedPart;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if(state == StartState.Editor)
            {
                return;
            }

            Events["Decouple"].active = NE_Helper.debugging() && !decoupled;
        }


        /// <summary>
        /// Returns true if the PEC is attached to a Vessel
        /// </summary>
        private bool isVesselShip()
        {
            bool isVesselShip = part.parent != null && vessel != null && !vessel.isEVA;
            return isVesselShip;
        }

        /// <summary>
        /// Check whether we have been coupled or decoupled from a Vessel
        /// </summary>
        /// Decoupling can occur on purpose (eg, Player removes the part from the ship using KIS),
        /// or accidentally if the ship undergoes a high-G maneuvre.
        /// TODO: Figure out whether we can hook into some game or KIS events instead as
        ///       running these checks every frame is a bit expensive.
        public override void OnUpdate()
        {
            base.OnUpdate();
            /* Only perform the max-G check if we are attached to a vessel.
             * During KAS grab, vessel can be itself or a Kerbal, and we may
             * get spurious high G's. */
            if (!decoupled && isVesselShip() && vessel.geeForce > maxGforce)
            {
                // Launch coroutine to ensure we really are decoupling due to max-G's
                // During KIS-grab, we can get spurious high G's
                StartCoroutine(WaitForDecouple(vessel.geeForce));
            }
            
        }

        /// <summary>
        /// Wait for a second before checking whether we've really decoupled due to high-G's
        /// or whether this got triggered due to a KIS grab
        /// </summary>
        private System.Collections.IEnumerator WaitForDecouple(double geeForce)
        {
            yield return new WaitForSeconds(1);
            // If the high G's were due to KIS, we're now decoupled (and probably inactive),
            // otherwise we need to detach now.
            if (!decoupled && isVesselShip() && geeForce > maxGforce)
            {
                NE_Helper.log("KEES PEC over max G, decouple\n" + this.ToString());
                Decouple();
            }
        }

        /// <summary>
        /// Decouples the PEC from its parent vessel.
        /// </summary>
        /// This can occur due to a high-G maneuvre, or if the Player removes the part
        /// from the ship, or due to a debug GUI click.
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5, guiName = "Decouple", active = true)]
        public void Decouple()
        {
            part.decouple();
            OnDetached();
        }

#if (DEBUG)
        [KSPEvent(guiActive = true, guiName = "Debug Dump", active = true)]
        public void DebugDump()
        {
            /* Printed out in Player.log */
            NE_Helper.log(this.ToString ());
        }
#endif

        /// <summary>
        /// Called when the PEC is attached to a Vessel
        /// </summary>
        public void OnAttached()
        {
            attachedPart?.SendMessage("OnPecCoupled", SendMessageOptions.DontRequireReceiver);
            decoupled = false;
            Events["Decouple"].active = NE_Helper.debugging();
        }

        /// <summary>
        /// Called when the PEC is detached from a Vessel
        /// </summary>
        public void OnDetached()
        {
            attachedPart?.SendMessage("OnPecDecoupled", SendMessageOptions.DontRequireReceiver);
            decoupled = true;
            Events["Decouple"].active = false;
        }

        #region KIS Events
        /// <summary>
        /// Process incoming KIS events
        /// </summary>
        /// eventData is a dictionary with the following entries:
        ///     action - name of action; we're interested in the following: AttachEnd, Decouple
        ///     targetPart - part we got attached to; on Decouple it's null
        ///     
        void OnKISAction(Dictionary<string, object> eventData)
        {
            NE_Helper.log("KEESExperiment: received KISAction - " + eventData.ToString());
            try
            {
                switch ((string)eventData["action"])
                {
                    // Called when we've been attached.
                    case "AttachEnd":
                        NE_Helper.log("KEES_Pec - attached using KIS");
                        OnAttached();
                        break;

                    case "Decouple":
                        NE_Helper.log("KEES_Pec - decoupled using KIS");
                        OnDetached();
                        break;
                }
            }
            catch (Exception e)
            {
                NE_Helper.logError("Exception while handling KISAction: " + e.ToString());
            }
        }
        #endregion

        /** Converts the object to a human-readble string suitable for printing.
        /** Converts the object to a human-readable string suitable for printing.
         * Overloads base-class implementation.
         */
        new public String ToString()
        {
            String ret = base.ToString () + "\n";
            ret += "\tnode:               " + node + "\n";
            ret += "\texp:                " + attachedPart + "\n";
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
