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

        private Light _alarmLight;
        private Material _lightMat;
        private AudioSource _alarmAs;

        private int count = 0;

        private int lightDir = 1;
        private float curIntensity = 0f;

        public override void OnFixedUpdate()
        {
            if (count == 0)
            {
                /*
                if (alarmLight == null)
                {
                    initPartObjects();
                }
                */

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
            if (alarmLight == null) return;

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
            if (alarmAs != null && alarmAs.isPlaying)
            {
                alarmAs.Stop();
            }
        }

        private void playSoundFX()
        {
            if (alarmAs != null && !alarmAs.isPlaying)
            {
                alarmAs.Play();
            }
        }

        public Light alarmLight
        {
            get
            {
                if (_alarmLight == null)
                {
                    GameObject labIVA = part.internalModel?.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                    if (labIVA == null)
                    {
                        NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - IVA not found");
                        goto done;
                    }

                    var lights = labIVA.GetComponentsInChildren<Light>();
                    if (lights == null || lights.Length == 0)
                    {
                        NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - no lights found in IVA");
                        goto done;
                    }

                    for (int idx = 0; idx < lights.Length; idx++)
                    {
                        var light = lights[idx];
                        if (light.name == "AlarmLight")
                        {
                            NE_Helper.log("Found alarm light");
                            _alarmLight = light;
                            _lightMat = light.GetComponent<Renderer>().material;
                            break;
                        }
                    }
                }
            done:
                return _alarmLight;
            }
        }

        public Material lightMat
        {
            get
            {
                if (_lightMat == null)
                {
                    _lightMat = alarmLight.GetComponent<Renderer>().material;
                }
                return _lightMat;
            }
        }

        public AudioSource alarmAs
        {
            get
            {
                if (_alarmAs == null)
                {
                    _alarmAs = part.gameObject.AddComponent<AudioSource>(); // using gameobjects from the internal model does not work AS would stay in the place it was added.
                    AudioClip clip = GameDatabase.Instance.GetAudioClip(alarmSound);
                    _alarmAs.clip = clip;
                    _alarmAs.dopplerLevel = DOPPLER_LEVEL;
                    _alarmAs.rolloffMode = AudioRolloffMode.Linear;
                    _alarmAs.Stop();
                    _alarmAs.loop = true;
                    _alarmAs.minDistance = MIN_DIST;
                    _alarmAs.maxDistance = MAX_DIST;
                    _alarmAs.volume = 0.6f;
                }
                return _alarmAs;
            }
        }

        /*
        // MKW DEBUG
        static bool isFirstTime = true;

        private void initPartObjects()
        {
#if true
            GameObject labIVA = part.internalModel?.gameObject.transform.GetChild(0).GetChild(0).gameObject;

            if (labIVA == null)
            {
                NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - IVA not found");
                return;
            }

            var lights = labIVA.GetComponentsInChildren<Light>();
            if (lights == null || lights.Length == 0)
            {
                NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - no lights found in IVA");
                return;
            }

            for (int idx = 0; idx < lights.Length; idx++)
            {
                var light = lights[idx];
                if (light.name == "AlarmLight")
                {
                    NE_Helper.log("Found alarm light");
                    alarmLight = light;
                    lightMat = light.GetComponent<Renderer>().material;

                    alarmAs = part.gameObject.AddComponent<AudioSource>(); // using gameobjects from the internal model does not work AS would stay in the place it was added.
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
            }

#else
            // MKW DEBUG
            if (isFirstTime && NE_Helper.debugging())
            {
                part.internalModel?.gameObject.PrintComponents(5);
            }
            var mf = labIVA?.GetComponent<MeshFilter>();
            if (mf == null)
            {
                NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - MeshFilter not found");
                if (isFirstTime && NE_Helper.debugging())
                {
                    labIVA?.PrintComponents(2);
                }
                if (labIVA?.transform?.GetComponent<MeshFilter>() != null)
                {
                    NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - but labIVA.transform has MeshFilter: {0}", labIVA.transform.GetComponent<MeshFilter>().name);
                    mf = labIVA.transform.GetComponent<MeshFilter>();
                }
                if (labIVA?.transform?.GetChild(0)?.GetComponent<MeshFilter>() != null)
                {
                    NE_Helper.log("MEP_IVA_AlarmAnimation.initPartObjects - but labIVA.transform.GetChild(0) has MeshFilter: {0}", labIVA.transform.GetChild(0).GetComponent<MeshFilter>().name);
                    mf = labIVA.transform.GetChild(0).GetComponent<MeshFilter>();
                }
            }

            if (mf?.name == "MEP IVA")
            {
                NE_Helper.log("set alarm light");

                GameObject light = labIVA.transform.GetChild(3).GetChild(0).gameObject;
                alarmLight = light.transform.GetChild(0).gameObject.GetComponent<Light>();

                lightMat = light.GetComponent<Renderer>().material;
            }
            else
            {
                NE_Helper.logError("MEP IVA not found; could not configure light");
            }

            alarmAs = part.gameObject.GetComponent<AudioSource>();
            if (alarmAs == null)
            {
                NE_Helper.log("set alarm audio");
                alarmAs = part.gameObject.AddComponent<AudioSource>(); // using gameobjects from the internal model does not work AS would stay in the place it was added.
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

            isFirstTime = false;
#endif
        }
        */
    }
}
