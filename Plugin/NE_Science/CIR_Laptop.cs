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
    class CIR_Laptop : InternalModule
    {
        private double lastUpdate = 0;
        private const string SCREEN_PART_NAME = "LaptopScreen";

        [KSPField(isPersistant = false)]
        public string folder = "NehemiahInc/OMS/Props/CIR_Laptop/";

        [KSPField(isPersistant = false)]
        public string noExpTexture = "";

        [KSPField(isPersistant = false)]
        public string expRunningTexture = "";

        [KSPField(isPersistant = false)]
        public float refreshInterval = 2;

        private GameDatabase.TextureInfo noExp;
        private GameDatabase.TextureInfo expRunning;

        private Material screenMat = null;
        private bool running = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (lastUpdate + refreshInterval < Time.time)
            {
                lastUpdate = Time.time;
                MSL_Module lab = part.GetComponent<MSL_Module>();
                if (gameObject.activeSelf != lab.hasEquipmentInstalled(EquipmentRacks.CIR))
                {
                    gameObject.SetActive(lab.hasEquipmentInstalled(EquipmentRacks.CIR));
                }
                if (lab.hasEquipmentInstalled(EquipmentRacks.CIR))
                {
                    if (running != lab.isEquipmentRunning(EquipmentRacks.CIR))
                    {
                        running = lab.isEquipmentRunning(EquipmentRacks.CIR);
                        changeTexture(getTextureForState(running));
                    }
                }

            }
        }

        private void changeTexture(GameDatabase.TextureInfo newTexture)
        {
            Material mat = getScreenMaterial();
            if (mat != null)
            {
                mat.mainTexture = newTexture.texture;
            }
            else
            {
                NE_Helper.logError("Transform NOT found: " + SCREEN_PART_NAME);
            }
        }

        private Material getScreenMaterial()
        {
            if (screenMat == null)
            {
                Transform t = internalProp.FindModelTransform(SCREEN_PART_NAME);
                if (t != null)
                {
                    screenMat = t.GetComponent<Renderer>().material;
                    return screenMat;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return screenMat;
            }
        }

        private GameDatabase.TextureInfo getTextureForState(bool running)
        {
            if(running){
                if (expRunning == null) expRunning = getTexture(folder, expRunningTexture);
                return expRunning;
            }else
            {
                if (noExp == null) noExp = getTexture(folder, noExpTexture);
                return noExp;
            }
        }

        private GameDatabase.TextureInfo getTexture(string folder, string textureName)
        {
            return GameDatabase.Instance.GetTextureInfoIn(folder, textureName);
        }
    }
}
