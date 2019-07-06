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
    class MSL_FIR_Animation : InternalModule
    {
        private bool isUserInIVA = false;

        [KSPField]
        public string pumpSound = "NehemiahInc/OMS/Sounds/pump";

        private const float PUMP1_SPEED = 10;
        private const float PUMP2_SPEED = 15;

        private const float DOPPLER_LEVEL = 0f;
        private const float MIN_DIST = 0.003f;
        private const float MAX_DIST = 0.004f;

        private Transform pump1;
        private Transform pump2;

        private AudioSource pumpAs;

        private int count = 0;

        /// <summary>
        /// Called every time object is activated.
        /// </summary>
        /// Use this instead of OnAwake so that we only listen to the GameEvents when we really have to.
        public void OnEnable()
        {
            GameEvents.OnCameraChange.Add(OnCameraChange);
            GameEvents.OnIVACameraKerbalChange.Add(OnIVACameraChange);
        }

        /// <summary>
        /// Called every time object is deactivated.
        /// </summary>
        /// Use this instead of OnDestroy so that we only listen to the GameEvents when we really have to.
        public void OnDisable()
        {
            GameEvents.OnCameraChange.Remove(OnCameraChange);
            GameEvents.OnIVACameraKerbalChange.Remove(OnIVACameraChange);
        }

        /// <summary>
        /// Called whenever the camera changes.
        /// </summary>
        /// WARNING: the first time this is called, the part may not be fully initialized yet
        /// so we must make sure all possible code-paths can handle nulls.
        /// <param name="newMode"></param>
        private void OnCameraChange(CameraManager.CameraMode newMode)
        {
            onCameraChanged();
        }

        /// <summary>
        /// Called whenever the IVA camera changes to a different Kerbal.
        /// </summary>
        /// <param name="newKerbal"></param>
        private void OnIVACameraChange(Kerbal newKerbal)
        {
            onCameraChanged();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (count == 0)
            {
                if (pump1 == null || pump2 == null)
                {
                    initPartObjects();
                }
                MSL_Module lab = part.GetComponent<MSL_Module>();
                if (lab.isEquipmentRunning(EquipmentRacks.FIR) && isUserInIVA)
                {
                    if (pump1 != null)
                    {
                        pump1.Rotate(PUMP1_SPEED, 0, 0);
                    }
                    if (pump2 != null)
                    {
                        pump2.Rotate(PUMP2_SPEED, 0, 0);
                    }
                    playSoundFX();
                }
                else
                {
                    stopSoundFX();
                }
            }
            count = (count + 1) % 2;
        }

        private void onCameraChanged()
        {
            isUserInIVA = NE_Helper.IsUserInIVA(part);
            if(!isUserInIVA)
            {
                // Need to call this since the OnFixedUpdate() is only called while in IVA.
                stopSoundFX();
            }
        }

        private void stopSoundFX()
        {
            if (pumpAs != null && pumpAs.isPlaying)
            {
                pumpAs.Stop();
            }
        }

        private void playSoundFX()
        {
            if (pumpAs != null && !pumpAs.isPlaying)
            {
                pumpAs.Play();
            }
        }

        private void initPartObjects()
        {
            if (part.internalModel == null)
            {
                return;
            }

            GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
            if (labIVA.GetComponent<MeshFilter>().name != "MSL_IVA")
            {
                return;
            }
                
            GameObject fir = labIVA.transform.FindChild("FIR").gameObject;
            pump1 = fir.transform.FindChild("Pump_1");
            pump2 = fir.transform.FindChild("Pump_2");

            pumpAs = part.gameObject.AddComponent<AudioSource>(); // using gameobjects from the internal model does not work AS would stay in the place it was added.
            AudioClip clip = GameDatabase.Instance.GetAudioClip(pumpSound);
            pumpAs.clip = clip;
            pumpAs.dopplerLevel = DOPPLER_LEVEL;
            pumpAs.rolloffMode = AudioRolloffMode.Logarithmic;
            pumpAs.Stop();
            pumpAs.loop = true;
            pumpAs.minDistance = MIN_DIST;
            pumpAs.maxDistance = MAX_DIST;
            pumpAs.volume = 1f;
        }
    }
}
