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
    class MEP_IVA_Alarm_Animation : InternalModule
    {

        [KSPField]
        public float maxIntensity = 2.5f;

        [KSPField]
        public float intensityStep = 0.1f;

        [KSPField]
        public string alarmSound = "NehemiahInc/Sounds/alarm";

        private const string EMISSIVE_COLOR = "_EmissiveCollor";

        private const float DOPPLER_LEVEL = 0f;
        private const float MIN_DIST = 1f;
        private const float MAX_DIST = 2f;

        private Light alarmLight;
        private Material lightMat;

        private AudioSource alarmAs;

        private int count = 0;

        private int lightDir = 1;
        private float curIntensity = 0f;

        public override void OnFixedUpdate()
        {
            if (count == 0)
            {
                if (alarmLight == null)
                {
                    initPartObjects();
                }
                MEP_Module lab = part.GetComponent<MEP_Module>();
                if (lab.MEPlabState == MEPLabStatus.ERROR_ON_START || lab.MEPlabState == MEPLabStatus.ERROR_ON_STOP)
                {
                    animateAlarmLight();
                    playSoundFX();
                }
                else
                {
                    if (curIntensity > 0.01f)
                    {
                        curIntensity = 0f;
                        alarmLight.intensity = curIntensity;
                        lightMat.SetColor(EMISSIVE_COLOR, new Color(0, 0, 0, 1));
                    }
                    stopSoundFX();
                }

            }
            count = (count + 1) % 2;
        }

        private void animateAlarmLight()
        {
            float newIntesity = curIntensity + (intensityStep * (float)lightDir);
            if (newIntesity > maxIntensity || newIntesity < 0.01f)
            {
                lightDir = lightDir * -1;
            }
            curIntensity = curIntensity + (intensityStep * (float)lightDir);
            alarmLight.intensity = curIntensity;

            float r = (1f / maxIntensity * curIntensity);

            Color newColor = new Color(r, 0, 0, 1);
            lightMat.SetColor(EMISSIVE_COLOR, newColor);
        }

        private void stopSoundFX()
        {
            if (!alarmAs.isPlaying)
            {
                alarmAs.Stop();
            }
        }

        private void playSoundFX()
        {
            if (!alarmAs.isPlaying)
            {
                alarmAs.Play();
            }
        }



        private void initPartObjects()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;

                if (labIVA.GetComponent<MeshFilter>().name == "MEP IVA")
                {
                    NE_Helper.log("set alarm light");

                    GameObject light = labIVA.transform.GetChild(3).GetChild(0).gameObject;
                    alarmLight = light.transform.GetChild(0).gameObject.GetComponent<Light>();

                    lightMat = light.GetComponent<Renderer>().material;
                    alarmAs = part.gameObject.GetComponent<AudioSource>();
                    if (alarmAs == null)
                    {
                        alarmAs = part.gameObject.AddComponent<AudioSource>(); // using gameobjects from the internal model does not work AS would stay in the place it was added.
                    }
                    AudioClip clip = GameDatabase.Instance.GetAudioClip(alarmSound);
                    alarmAs.clip = clip;
                    alarmAs.dopplerLevel = DOPPLER_LEVEL;
                    alarmAs.rolloffMode = AudioRolloffMode.Linear;
                    alarmAs.Stop();
                    alarmAs.loop = true;
                    alarmAs.minDistance = MIN_DIST;
                    alarmAs.maxDistance = MAX_DIST;
                    alarmAs.volume = 0.6f;
                }
                else
                {
                    NE_Helper.logError("MEP IVA not found");
                }
            }
        }
    }
}
