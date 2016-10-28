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
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science
{
    class MSL_Printer_Animation : InternalModule
    {
        private bool isUserInIVA = false;

        [KSPField]
        public string prMovingSound = "NehemiahInc/OMS/Sounds/3DPmove";

        [KSPField]
        public string prBaseChgDirSound = "NehemiahInc/OMS/Sounds/3DPchgDir";

        [KSPField]
        public string prHeadChgDirSound = "NehemiahInc/OMS/Sounds/3DPheadChgDir";

        private const float BASE_SPEED = 0.01f;
        private const float HEAD_SPEED = 0.02f;

        private const float HEAD_MAX = 0.54f;
        private const float HEAD_MIN = -0.54f;

        private const float BASE_MAX = 0.35f;
        private const float BASE_MIN = -0.33f;

        private const float DOPPLER_LEVEL = 0f;
        private const float MIN_DIST = 0.003f;
        private const float MAX_DIST = 0.004f;


        private Transform headBase;
        private Transform head;

        private AudioSource prAs;
        private AudioSource prBaseChgDirAs;
        private AudioSource prHeadChgDirAs;

        private int count = 0;

        private int baseDirection = 1;
        private int headDirection = 1;

        public override void OnAwake()
        {
            base.OnAwake();
            GameEvents.OnCameraChange.Add(OnCameraChange);
        }

        private void OnCameraChange(CameraManager.CameraMode newMode)
        {
            isUserInIVA = NE_Helper.IsUserInIVA(part);

            // If we leave IVA, stop all sounds
            if (!isUserInIVA)
            {
                stopSoundFX();
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnUpdate();
            if (count == 0)
            {
                if (headBase == null || head == null)
                {
                    initPartObjects();
                }
                MSL_Module lab = part.GetComponent<MSL_Module>();
                if (lab.isEquipmentRunning(EquipmentRacks.PRINTER) && isUserInIVA)
                {
                    moveBase();
                    moveHead();
                    playSoundFX();
                }
                else
                {
                    stopSoundFX();
                }
            }
            count = (count + 1) % 2;
        }

        private void stopSoundFX()
        {
            if (prAs.isPlaying)
            {
                prAs.Stop();
            }
        }

        private void playSoundFX()
        {
            if (!prAs.isPlaying)
            {
                prAs.Play();
            }
        }

        private void moveHead()
        {
            float pos = head.localPosition.y;
            pos += HEAD_SPEED * -headDirection; //I dont understand why it has to be -headDirection to work
            if (pos > HEAD_MAX || pos < HEAD_MIN)
            {
               headDirection = headDirection * -1;
                if (prAs.isPlaying)
                {
                    prHeadChgDirAs.Play();
                }
            }
            else
            {
                float movment = HEAD_SPEED * headDirection;
                head.Translate(0, movment, 0, Space.Self);
            }
        }

        private void moveBase()
        {
            float pos = headBase.localPosition.x;
            pos += BASE_SPEED * baseDirection;
            if (pos > BASE_MAX || pos < BASE_MIN)
            {
                baseDirection = baseDirection * -1;
                if (prAs.isPlaying)
                {
                    prBaseChgDirAs.Play();
                }
            }
            else
            {
                headBase.Translate(BASE_SPEED * baseDirection, 0, 0);
            }
        }

        private void initPartObjects()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>().name == "Lab1IVA")
                {
                    NE_Helper.log("set printer transforms");
                    GameObject printer = labIVA.transform.GetChild(0).gameObject;
                    //GameObject cir = labIVA.transform.GetChild(1).gameObject;
                    headBase = printer.transform.GetChild(1).GetChild(0);
                    if (headBase != null)
                    {
                        prAs = part.gameObject.AddComponent<AudioSource>();// using gameobjects from the internal model does not work AS would stay in the place it was added.
                        AudioClip clip = GameDatabase.Instance.GetAudioClip(prMovingSound);
                        prAs.clip = clip;
                        prAs.dopplerLevel = DOPPLER_LEVEL;
                        prAs.rolloffMode = AudioRolloffMode.Logarithmic;
                        prAs.Stop();
                        prAs.loop = true;
                        prAs.minDistance = MIN_DIST;
                        prAs.maxDistance = MAX_DIST;
                        prAs.volume = 1f;

                        prBaseChgDirAs = part.gameObject.AddComponent<AudioSource>();// using gameobjects from the internal model does not work AS would stay in the place it was added.
                        prBaseChgDirAs.clip = GameDatabase.Instance.GetAudioClip(prBaseChgDirSound);
                        prBaseChgDirAs.dopplerLevel = DOPPLER_LEVEL;
                        prBaseChgDirAs.rolloffMode = AudioRolloffMode.Logarithmic;
                        prBaseChgDirAs.Stop();
                        prBaseChgDirAs.loop = false;
                        prBaseChgDirAs.minDistance = MIN_DIST;
                        prBaseChgDirAs.maxDistance = MAX_DIST;
                        prBaseChgDirAs.volume = 0.4f;

                        prHeadChgDirAs = part.gameObject.AddComponent<AudioSource>();// using gameobjects from the internal model does not work AS would stay in the place it was added.
                        prHeadChgDirAs.clip = GameDatabase.Instance.GetAudioClip(prHeadChgDirSound);
                        prHeadChgDirAs.dopplerLevel = DOPPLER_LEVEL;
                        prHeadChgDirAs.rolloffMode = AudioRolloffMode.Logarithmic;
                        prHeadChgDirAs.Stop();
                        prHeadChgDirAs.loop = false;
                        prHeadChgDirAs.minDistance = MIN_DIST;
                        prHeadChgDirAs.maxDistance = MAX_DIST;
                        prHeadChgDirAs.volume = 1f;
                    }
                    head = headBase.GetChild(0);
                }
            }
        }
    }
}
