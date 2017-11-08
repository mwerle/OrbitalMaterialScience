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
    class MEP_StatusScreen : InternalModule
    {
        private double lastUpdate = 0;

        [KSPField(isPersistant = false)]
        public string folder = "NehemiahInc/OMS/Props/MEP_StatusScreen/";

        [KSPField(isPersistant = false)]
        public string notReadyTexture = "";

        [KSPField(isPersistant = false)]
        public string readyTexture = "";

        [KSPField(isPersistant = false)]
        public string runningTexture = "";

        [KSPField(isPersistant = false)]
        public string errorTexture = "";

        [KSPField(isPersistant = false)]
        public float refreshInterval = 2;

        private GameDatabase.TextureInfo notReady;
        private GameDatabase.TextureInfo ready;
        private GameDatabase.TextureInfo running;
        private GameDatabase.TextureInfo error;
        private Material screenMat = null;

        private MEPLabStatus lastLabStatus = MEPLabStatus.NONE;

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

                if (lab.MEPlabState != lastLabStatus)
                {
                    GameDatabase.TextureInfo newTexture = getTextureForState(lab.MEPlabState);
                    if (newTexture != null)
                    {
                        changeTexture(newTexture);
                        
                    }
                    else
                    {
                        NE_Helper.logError("New Texture null");
                    }
                    lastLabStatus = lab.MEPlabState;
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

        private GameDatabase.TextureInfo getTextureForState(MEPLabStatus p)
        {
            switch (p)
            {
                case MEPLabStatus.NOT_READY:
                    if (notReady == null) notReady = getTexture(folder, notReadyTexture);
                    return notReady;

                case MEPLabStatus.READY:
                    if (ready == null) ready = getTexture(folder, readyTexture);
                    return ready;

                case MEPLabStatus.RUNNING:
                    if (running == null) running = getTexture(folder, runningTexture);
                    return running;

                case MEPLabStatus.ERROR_ON_START:
                case MEPLabStatus.ERROR_ON_STOP:
                    if (error == null) error = getTexture(folder, errorTexture);
                    return error;

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
