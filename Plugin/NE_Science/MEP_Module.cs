/*
 *   This file is part of Orbital Material Science.
 *
 *   Part of the code may originate from Station Science by ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    class MEP_Module : Lab
    {
        private const string MEP_STATE_VALUE = "Mep_State";
        private const string EXPOSURE_LAB_EQUIPMENT_TYPE = "EXPOSURE";

        public MEPLabStatus MEPlabState = MEPLabStatus.NOT_READY;

        [KSPField(isPersistant = false)]
        public float ExposureTimePerHour = 0;

        [KSPField(isPersistant = false)]
        public bool failures = true;

        [KSPField(isPersistant = false)]
        public int failurePercentage = 1;

        [KSPField(isPersistant = true)]
        public int armOps = 0;

        [KSPField(isPersistant = true, guiActive = true, guiName = "#ne_Experiment")]
        public string experimentName = "No Experiment";

        private string deployAnimName = "Deploy";
        private string startExpAnimName = "StartExperiment";
        private string errorOnStartAnimName = "ErrorOnStart";
        private string errorOnStopAnimName = "ErrorOnStop";
        public delegate void OnAnimationFinished(Animation a);

        private LabEquipmentSlot exposureSlot = new LabEquipmentSlot(EquipmentRacks.EXPOSURE);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            string stateString = node.GetValue(MEP_STATE_VALUE);
            if (stateString != null)
            {
                MEPlabState = MEPLabStatusFactory.getType(stateString);
            }
            exposureSlot = getLabEquipmentSlotByType(node, EXPOSURE_LAB_EQUIPMENT_TYPE);
        }


        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MEP OnSave");
            node.AddValue(MEP_STATE_VALUE, MEPlabState);
            node.AddNode(exposureSlot.getConfigNode());
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            NE_Helper.log("MEP_Module OnStart");
            exposureSlot.onStart(this);
            if (!exposureSlot.isEquipmentInstalled())
            {
                exposureSlot.install(new LabEquipment("MEP", "MEP", EquipmentRacks.EXPOSURE, 0f, 0f, ExposureTimePerHour, Resources.EXPOSURE_TIME, 0, ""), this); ;
            }

            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    Events["DeployPlatform"].guiName = Localizer.GetStringByTag("#ne_Deploy_Platform");
                    playAnimation(deployAnimName, -1f, 0f, onAnimationOnStartFinished);
                    break;
                case MEPLabStatus.READY:
                    Events["DeployPlatform"].guiName = Localizer.GetStringByTag("#ne_Retract_Platform");
                    playAnimation(startExpAnimName, -1f, 0f, onAnimationOnStartFinished);
                    break;

                case MEPLabStatus.RUNNING:
                    playAnimation(startExpAnimName, 1f, 1f, onAnimationOnStartFinished);
                    break;

                case MEPLabStatus.ERROR_ON_START:
                    playAnimation(errorOnStartAnimName, 1f, 1f, onAnimationOnStartFinished);
                    Events["DeployPlatform"].active = false;
                    Events["labAction"].active = false;
                    Events["FixArm"].active = true;
                    break;

                case MEPLabStatus.ERROR_ON_STOP:
                    playAnimation(errorOnStopAnimName, -1f, 0f, onAnimationOnStartFinished);
                    Events["DeployPlatform"].active = false;
                    Events["labAction"].active = false;
                    Events["FixArm"].active = true;
                    break;
            }
        }

        #region Right-click menu buttons (KSPEvent)
        /// <summary>
        /// Event to allow moving an experiment.
        /// </summary>
        [KSPEvent(guiActive = true, guiName = "#ne_Move_Experiment")]
        public void moveExp()
        {
            exposureSlot.moveExperiment(part.vessel);
        }

        /// <summary>
        /// Event to control the experiment.
        /// </summary>
        [KSPEvent(guiActive = true, guiName = "#ne_Action_Experiment")]
        public void actionExp()
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (exposureSlot.isExposureAction())
            {
                bool shouldArmMove = canArmMove();
                if(shouldArmMove)
                {
                    exposureSlot.experimentAction();
                }
                animateRobotArm(shouldArmMove);
            }
            else
            {
                exposureSlot.experimentAction();
            }
        }

        /// <summary>
        /// Event to fix the robot arm.
        /// </summary>
        /// This event should only be usable from EVA.
        /// TODO: Make the event only usable with an Engineer.
        [KSPEvent(active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.0f, guiName = "#ne_Fix_robotic_arm")]
        public void FixArm()
        {
            Events["FixArm"].active = false;
            armOps = 0;
            switch (MEPlabState)
            {
                case MEPLabStatus.ERROR_ON_START:
                    ScreenMessages.PostScreenMessage("#ne_Robotic_arm_fixed_Experiment_will_start_soon", 2, ScreenMessageStyle.UPPER_CENTER);
                    playAnimation(errorOnStartAnimName, -1f, 1f, onAnimStartFixFinished);
                    break;
                case MEPLabStatus.ERROR_ON_STOP:
                    ScreenMessages.PostScreenMessage("#ne_Robotic_arm_fixed", 2, ScreenMessageStyle.UPPER_CENTER);
                    playAnimation(errorOnStopAnimName, 1f, 0f, onAnimStopFixFinished);
                    break;
            }
        }

        /// <summary>
        /// Event to open/close the exposure platform.
        /// </summary>
        /// TODO: Ensure it can only be controlled if a Kerbal is in the Part.
        [KSPEvent(guiActive = true, guiName = "#ne_Deploy_Platform")]
        public void DeployPlatform()
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            Events["labAction"].active = false;
            Events["DeployPlatform"].active = false;
            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    playAnimation(deployAnimName, 1f, 0, onAnimationDeployPlatformFinished);
                    break;
                case MEPLabStatus.READY:
                    playAnimation(deployAnimName, -1f, 1, onAnimationRetractPlatformFinished);
                    break;
            }
        }
        #endregion

        #region KSPActions
        // KSPActions can be bound to action keys in the Editor
        [KSPAction("#ne_Deploy_Platform")]
        public void deployExposurePlatform(KSPActionParam param)
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if(MEPlabState != MEPLabStatus.NOT_READY)
            {
                return;
            }
            Events["labAction"].active = false;
            Events["DeployPlatform"].active = false;
            playAnimation(deployAnimName, 1f, 0, onAnimationDeployPlatformFinished);
        }

        [KSPAction("#ne_Retract_Platform")]
        public void retractExposurePlatform(KSPActionParam param)
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if(MEPlabState != MEPLabStatus.READY)
            {
                return;
            }
            Events["labAction"].active = false;
            Events["DeployPlatform"].active = false;
            playAnimation(deployAnimName, -1f, 1, onAnimationDeployPlatformFinished);
        }

        #endregion

        public override void installExperiment(ExperimentData exp)
        {
            if (exp.getEquipmentNeeded() == EquipmentRacks.EXPOSURE && exposureSlot.experimentSlotFree())
            {
                exposureSlot.installExperiment(exp);
                experimentName = exp.getAbbreviation() + ": " + exp.stateString();
                
            }
        }

        public LabEquipmentSlot getExposureSlot()
        {
            return exposureSlot;
        }

        public bool isRunning()
        {
            return MEPlabState == MEPLabStatus.RUNNING;
        }

        public bool isReady()
        {
            return MEPlabState == MEPLabStatus.READY;
        }

        public bool hasError()
        {
            return MEPlabState == MEPLabStatus.ERROR_ON_START || MEPlabState == MEPLabStatus.ERROR_ON_STOP;
        }

        protected override bool isActive()
        {
            return doResearch && isRunning() && part.protoModuleCrew.Count >= minimumCrew && !OMSExperiment.checkBoring(vessel, false);
        }

        protected override void updateLabStatus()
        {
            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Platform_retracted"));
                    break;
                case MEPLabStatus.READY:
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Idle"));
                    break;
                case MEPLabStatus.RUNNING:
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Running"));
                    break;
                case MEPLabStatus.ERROR_ON_START:
                case MEPLabStatus.ERROR_ON_STOP:
                    displayStatusMessage(Localizer.GetStringByTag("#ne_Robotic_Arm_Failure"));
                    break;
            }

            // TODO: Need callbacks on experiment installed and removed.
            Fields["experimentName"].guiActive = !exposureSlot.experimentSlotFree();
            if( Fields["experimentName"].guiActive )
            {
                experimentName = exposureSlot.getExperiment().getAbbreviation() + ": " + exposureSlot.getExperiment().stateString();

                Events["moveExp"].active = exposureSlot.canExperimentMove(part.vessel);
                if (Events["moveExp"].active)
                {
                    Events["moveExp"].guiName = Localizer.Format("#ne_Move_1", exposureSlot.getExperiment().getAbbreviation());
                }
                Events["actionExp"].active = exposureSlot.canActionRun();
                if (Events["actionExp"].active)
                {
                    Events["actionExp"].guiName = exposureSlot.getActionString();
                }
            }
        }

        protected override bool canPerformLabActions()
        {
            // The MEP doesn't usually require crew but does when performing an action.
            return (part.protoModuleCrew.Count >= 1) && base.canPerformLabActions();
        }

        protected override bool onLabPaused()
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            if( !base.onLabPaused() )
            {
                return false;
            }
            ExperimentData e = exposureSlot?.getExperiment();

            if( e != null )
            {
                /* And if the experiment is running let's animate closing it. */
                if( exposureSlot.isExposureAction() && (e.state == ExperimentState.RUNNING) )
                {
                    animateRobotArm(canArmMove());
                }
                /* Notify the experiment that the lab has paused */
                e.onPaused();
            }
            return true;
        }

        protected override bool onLabStarted()
        {
            if (!canPerformLabActions())
            {
                ScreenMessages.PostScreenMessage("#ne_Not_enough_crew_in_this_module", 6, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            if( !base.onLabStarted() )
            {
                return false;
            }

            ExperimentData e = exposureSlot?.getExperiment();
            if( e != null )
            {
                /* And if the experiment is running let's animate closing it. */
                if( exposureSlot.isExposureAction() && (e.state == ExperimentState.RUNNING) )
                {
                    animateRobotArm(canArmMove());
                }
                /* Notify the experiment that the lab has resumed */
                e.onResumed();
            }
            return true;
        }

        /// <summary>
        /// Calculates chance of robot arm failure.
        /// </summary>
        /// If robot arm failures are enabled, this function calculates whether the robot arm failed. The chance
        /// of failure increases every time the arm is used.
        /// <returns>True if robot arm did not fail.</returns>
        private bool canArmMove()
        {
            if (failures)
            {
                if (failurePercentage > 100)
                {
                    failurePercentage = 100;
                }
                if (failurePercentage < 1)
                {
                    failurePercentage = 1;
                }
                int actFailurePerc = failurePercentage + (int)(armOps * 1.5f);
                int i = new System.Random().Next(1, (100 / actFailurePerc + 1));
                NE_Helper.log("ExpLab is successfull: " + !(i == 1) + " ; " + i + " ; percentage: " + failurePercentage + "% armOps: " + armOps + " actual percentage: " + actFailurePerc + "%:");
                ++armOps;
                return !(i == 1);
            }
            else
            {
                return true;
            }
        }

        private void onAnimationOnStartFinished(Animation anim)
        {
            updateLabStatus();
        }

        private void onAnimationDeployPlatformFinished(Animation anim)
        {
            MEPlabState = MEPLabStatus.READY;
            Events["DeployPlatform"].guiName = Localizer.GetStringByTag("#ne_Retract_Platform");
            Events["DeployPlatform"].active = true;
            Events["labAction"].active = true;
            Events["actionExp"].active = exposureSlot.canActionRun();
        }

        private void onAnimationRetractPlatformFinished(Animation anim)
        {
            MEPlabState = MEPLabStatus.NOT_READY;
            Events["DeployPlatform"].guiName = Localizer.GetStringByTag("#ne_Deploy_Platform");
            Events["DeployPlatform"].active = true;
            Events["labAction"].active = true;
        }

        private void onAnimExpStartFinished(Animation anim)
        {
            MEPlabState = MEPLabStatus.RUNNING;
            Events["labAction"].active = true;
        }

        private void onAnimExpStopFinished(Animation anim)
        {
            MEPlabState = MEPLabStatus.READY;
            Events["labAction"].active = true;
        }

        private void onAnimExpStartErrorFinished(Animation anim)
        {
            ScreenMessages.PostScreenMessage("#ne_Warning_robotic_arm_failure", 6, ScreenMessageStyle.UPPER_CENTER);
            MEPlabState = MEPLabStatus.ERROR_ON_START;
            Events["FixArm"].active = true;
        }

        private void onAnimExpStopErrorFinished(Animation anim)
        {
            ScreenMessages.PostScreenMessage("#ne_Warning_robotic_arm_failure", 6, ScreenMessageStyle.UPPER_CENTER);
            MEPlabState = MEPLabStatus.ERROR_ON_STOP;
            Events["FixArm"].active = true;
        }

        private void onAnimStartFixFinished(Animation anim)
        {
            playAnimation(startExpAnimName, 1f, 0, onAnimExpStartFinished);
        }

        private void onAnimStopFixFinished(Animation anim)
        {
            onAnimExpStopFinished(anim);
        }

        /// <summary>
        /// Coroutine which waits for the animation to finish playing before running the specified delegate.
        /// Code shamelessly lifted from  AnimatedDecoupler/DecoupleAnimator.cs
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator runOnAnimationFinished(Animation anim, OnAnimationFinished onFinished = null)
        {
            while (anim.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            onFinished?.Invoke(anim);
        }

        private void playAnimation(string animName, float speed, float time, OnAnimationFinished onFinished = null)
        {
            // MKW TODO - remove "FirstOrDefault" Linq statement
            Animation anim = part.FindModelAnimators(animName).FirstOrDefault();
            if (anim != null)
            {
                anim[animName].speed = speed;
                anim[animName].normalizedTime = time;
                anim.Play(animName);
                if(onFinished != null)
                {
                    StartCoroutine(runOnAnimationFinished(anim, onFinished));
                }
            }
            else
            {
                NE_Helper.logError("no Animation; Name: " + animName);
            }
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + Localizer.Format("#ne_Exposure_Time_per_hour_1", ExposureTimePerHour);
            return ret;
        }

        internal bool hasEquipmentFreeExperimentSlot(EquipmentRacks neededEquipment)
        {
            return exposureSlot.experimentSlotFree();
        }

        /// <summary>
        /// Runs the relevant robot arm animation.
        /// </summary>
        internal void animateRobotArm(bool canArmMove)
        {
            Events["labAction"].active = false;
            Events["DeployPlatform"].active = false;
            switch (MEPlabState)
            {
                case MEPLabStatus.READY:
                    if(canArmMove)
                    {
                        playAnimation(startExpAnimName, 1f, 0f, onAnimExpStartFinished);
                    }
                    else
                    {
                        playAnimation(errorOnStartAnimName, 1f, 0f, onAnimExpStartErrorFinished);
                    }
                    break;
                case MEPLabStatus.RUNNING:
                    if(canArmMove)
                    {
                        playAnimation(startExpAnimName, -1f, 1f, onAnimExpStopFinished);
                    }
                    else
                    {
                        playAnimation(errorOnStopAnimName, -1f, 1, onAnimExpStopErrorFinished);
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns the mass of installed equipment and experiments.
        /// </summary>
        /// <returns>The mass.</returns>
        protected override float getMass()
        {
            float mass = 0f;
            mass += exposureSlot.getMass();
            return mass;
        }
    }

    public enum MEPLabStatus
    {
        NOT_READY, READY, RUNNING, ERROR_ON_START, ERROR_ON_STOP, NONE
    }

    public class MEPLabStatusFactory
    {
        public static MEPLabStatus getType(string p)
        {
            switch (p)
            {
                case "NOT_READY":
                    return MEPLabStatus.NOT_READY;
                case "READY":
                    return MEPLabStatus.READY;
                case "RUNNING":
                    return MEPLabStatus.RUNNING;
                case "ERROR_ON_START":
                    return MEPLabStatus.ERROR_ON_START;
                case "ERROR_ON_STOP":
                    return MEPLabStatus.ERROR_ON_STOP;
                case "NONE":
                    return MEPLabStatus.NONE;
                default:
                    return MEPLabStatus.NOT_READY;
            }
        }
    }
}
