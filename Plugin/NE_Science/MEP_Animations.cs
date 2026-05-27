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

        private Light warnRotationLight;
        private Light warnPointLight;

        private bool error = false;

        // MKW DEBUG
        public static bool isFirstTime = true;

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

            var lights = gameObject.GetComponentsInChildren<Light>();
            if (lights == null)
            {
                // Try to find inactive components
                lights = gameObject.GetComponentsInChildren<Light>(true);
            }
            if (lights == null || lights.Length == 0)
            {
                NE_Helper.logError("MEP_Animation: No lights found in MEP_Module!");
                if (NE_Helper.debugging() && isFirstTime)
                {
                    gameObject.PrintComponents(5);
                    isFirstTime = false;
                }
                return;
            }

            for (int idx = 0, count = lights.Length; idx < count; idx++)
            {
                // NOTE: The light names are the names of the corresponding
                // light objects in the Unity project
                var light = lights[idx];
                if (light.name == "WarnRotationLight")
                {
                    warnRotationLight = light;
                }
                else if (light.name == "WarnPointLight")
                {
                    warnPointLight = light;
                }
            }
        }

        public override void OnUpdate()
        {
            if (lab.hasError())
            {
                if (!error)
                {
                    switchLightsOn();
                    error = true;
                }
                warnRotationLight?.transform.Rotate(Time.deltaTime * 180, 0, 0);
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
            if (warnRotationLight != null)
            {
                warnRotationLight.intensity = 0f;
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
            if (warnRotationLight != null)
            {
                warnRotationLight.intensity = 6f;
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
