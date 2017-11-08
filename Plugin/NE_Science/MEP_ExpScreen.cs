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
    class MEP_ExpScreen : InternalModule
    {
        private double lastUpdate = 0;

        [KSPField(isPersistant = false)]
        public string folder = "NehemiahInc/OMS/Props/MEP_StatusScreen/";

        [KSPField(isPersistant = false)]
        public string noExpTexture = "";

        [KSPField(isPersistant = false)]
        public string mee1Texture = "";

        [KSPField(isPersistant = false)]
        public string mee2Texture = "";

        [KSPField(isPersistant = false)]
        public float refreshInterval = 2;

        private GameDatabase.TextureInfo noExp;
        private GameDatabase.TextureInfo mee1;
        private GameDatabase.TextureInfo mee2;

        private Material screenMat = null;

        private string lastExpName = "empty";

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
                MEP_Module lab = part.GetComponent<MEP_Module>();

                string currentExperiment = lab.getExposureSlot().getExperiment().getAbbreviation();
                if (currentExperiment != lastExpName)
                {
                    GameDatabase.TextureInfo newTexture = getTextureForState(currentExperiment);
                    if (newTexture != null)
                    {
                        changeTexture(newTexture);
                        
                    }
                    else
                    {
                        NE_Helper.logError("New Texture null, Exp Name: " + currentExperiment);
                    }
                    lastExpName = currentExperiment;
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
                NE_Helper.logError("Transform NOT found: " + "MEP IVA Screen");
            }
        }

        private Material getScreenMaterial()
        {
            if (screenMat == null)
            {
                Transform t = internalProp.FindModelTransform("MEP IVA Screen");
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

        private GameDatabase.TextureInfo getTextureForState(string name)
        {
            NE_Helper.log("Experiment Name: " + name);
            switch (name)
            {
                case "empty":
                     if (noExp == null) noExp = getTexture(folder, noExpTexture);
                    return noExp;
                case "MEE1":
                    if (mee1 == null) mee1 = getTexture(folder, mee1Texture);
                    return mee1;
                case "MEE2":
                    if (mee2 == null) mee2 = getTexture(folder, mee2Texture);
                    return mee2;
                default:
                    return null;
            }
        }

        private GameDatabase.TextureInfo getTexture(string folder, string textureName)
        {
            return GameDatabase.Instance.GetTextureInfoIn(folder, textureName);
        }
    }
}
