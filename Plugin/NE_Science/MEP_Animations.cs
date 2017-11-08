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
    class MEP_Animations : PartModule
    {

        private MEP_Module lab;

        private Light warnLight;
        private Light warnPointLight;

        private bool error = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                return;
            }

            lab = gameObject.GetComponent<MEP_Module>();
            if (lab == null)
            {
                NE_Helper.logError("MEP_Animation: MEP_Module not found!");
                return;
            }

            var lights = gameObject.GetComponentsInChildren(typeof(Light)) as Light[];
            if (lights == null)
            {
                NE_Helper.logError("MEP_Animation: No lights found in MEP_Module!");
                return;
            }

            for (int idx = 0, count = lights.Length; idx < count; idx++)
            {
                var light = lights[idx];
                if (light.name == "rotationLight")
                {
                    warnLight = light;
                }
                else if (light.name == "WarnPointLlight")
                {
                    warnPointLight = light;
                }
            }
        }

        public override void OnUpdate()
        {
            if (lab.MEPlabState == MEPLabStatus.ERROR_ON_START || lab.MEPlabState == MEPLabStatus.ERROR_ON_STOP)
            {
                if (!error)
                {
                    switchLightsOn();
                    error = true;
                }
                warnLight.transform.Rotate(Time.deltaTime * 180, 0, 0);
            }
            else
            {
                if (error)
                {
                    switchLightsOff();
                    error = false;
                }
            }
        }

        private void switchLightsOff()
        {
            if (warnLight != null)
            {
                warnLight.intensity = 0f;
            }
            else
            {
                NE_Helper.logError("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.0f;
            }
            else
            {
                NE_Helper.logError("WarnPointLight null");
            }
        }

        private void switchLightsOn()
        {
            if (warnLight != null)
            {
                warnLight.intensity = 6f;
            }
            else
            {
                NE_Helper.logError("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.5f;
            }
            else
            {
                NE_Helper.logError("WarnPointLight null");
            }
        }
    }
}
