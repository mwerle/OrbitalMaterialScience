/*
 *   This file is part of Orbital Material Science.
 *
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
    public class KEESExperiment : OMSExperiment
    {
        /* Overload from OMSExperiment */
        new public string notReadyStatus = Localizer.GetStringByTag("#ne_Not_installed");
        new public string readyStatus = Localizer.GetStringByTag("#ne_Ready");
        new public string errorStatus = Localizer.GetStringByTag("#ne_Experiment_Ruined");

        [KSPField(isPersistant = false)]
        public int exposureTimeRequired;

        [KSPField(isPersistant = true)]
        public bool docked = false;

        private const string deployAnimation = "Deploy";

        public PartResource getResource(string name)
        {
            return ResourceHelper.getResource(part, name);
        }

        public double getResourceAmount(string name)
        {
            return ResourceHelper.getResourceAmount(part, name);
        }

        public PartResource setResourceMaxAmount(string name, double max)
        {
            return ResourceHelper.setResourceMaxAmount(part, name, max);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor) { return; }

            #if (DEBUG)
            Events["DebugDump"].active = true;
            #endif
            this.part.force_activate();
            switch(this.state){
                case READY:
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    break;
                case FINISHED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = true;
                    break;
                case RUNNING:
                    playAnimation(deployAnimation, 1f, 1f);
                    break;
            }
            setEVAconfigForStart(true, 3);
            StartCoroutine(updateStatus());
        }

        #if (DEBUG)
        [KSPEvent(guiActive = true, guiName = "Debug Dump", active = true)]
        public void DebugDump()
        {
            /* Printed out in Player.log */
            NE_Helper.log(this.ToString ());
        }
        #endif

        /** Converts the object to a human-readble string suitable for printing.
         * Overloads base-class implementation.
         */
        new public String ToString()
        {
            String ret = base.ToString () + "\n";
            ret += "\tstate:              " + this.state + "\n";
            ret += "\tresource Amount:    " + getResourceAmount(Resources.EXPOSURE_TIME) + "\n";
            ret += "\tScience Amount:     " + GetScienceCount() + "\n";
            ret += "\tdataIsCollectable:  " + dataIsCollectable + "\n";
            ret += "\tresettable:         " + resettable + "\n";
            ret += "\tresourceToReset:    " + resourceToReset + "\n";
            ret += "\trerunnable:         " + rerunnable + "\n";
            return ret;
        }

        public System.Collections.IEnumerator updateStatus()
        {
            while (true)
            {
                updateState();
                yield return new UnityEngine.WaitForSeconds(1f);
            }
        }

        private void setEVAconfigForStart(bool unfocused, float distance)
        {
            Events["StartExperiment"].guiActiveUnfocused = unfocused;
            Events["StartExperiment"].unfocusedRange = distance;
        }


        [KSPEvent(guiActive = true, guiName = "#ne_Start_Experiment", active = false)]
        public void StartExperiment()
        {
            if (GetScienceCount() > 0)
            {
                ScreenMessages.PostScreenMessage("#ne_Experiment_already_finalized", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (checkBoring(vessel, true)) return;

            if (experimentStarted())
            {
                createResources();
                ScreenMessages.PostScreenMessage("#ne_Started_experiment", 6, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void createResources()
        {
            PartResource exposureTime = setResourceMaxAmount(Resources.EXPOSURE_TIME, exposureTimeRequired);
        }


        public bool deployChecks(bool msg = true)
        {
            if (checkBoring(vessel, true)) return false;
            if (state == FINISHED)
            {
                return true;
            }
            else
            {
                if (msg)
                {
                    ScreenMessages.PostScreenMessage("#ne_Experiment_not_finished_yet", 6, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            return false;
        }

        new public void DeployExperiment()
        {
            if (deployChecks())
            {
                base.DeployExperiment();
                finalized();
            }
        }

        new public void ResetExperiment()
        {
            base.ResetExperiment();
            resetExp();
        }

        new public void ResetExperimentExternal()
        {
            base.ResetExperimentExternal();
            resetExp();
        }

        new public void ResetAction(KSPActionParam p)
        {
            base.ResetAction(p);
            resetExp();
        }

        public void stopResearch(string resName)
        {
            setResourceMaxAmount(resName, 0);
        }

        public void stopResearch()
        {
            stopResearch(Resources.EXPOSURE_TIME);
        }

        public void updateState()
        {
            switch (state)
            {
                case NOT_READY:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = notReadyStatus;
                    checkForLabs(false);
                    break;
                case READY:
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    expStatus = readyStatus;
                    checkForLabs(true);
                    break;
                case RUNNING:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = runningStatus;
                    checkBiomeChange();
                    checkUndocked();
                    checkFinished();
                    break;
                case FINISHED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = deployChecks(false);
                    expStatus = finishedStatus;
                    break;
                case FINALIZED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = finalizedStatus;
                    break;
                case ERROR:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = errorStatus;
                    break;
            }
        }

        public virtual void checkFinished()
        {

            if(isFinished())
            {
                finished();
            }
        }

        public bool isFinished()
        {
            double numExposureTime = getResourceAmount(Resources.EXPOSURE_TIME);
            return Math.Round(numExposureTime, 2) >= exposureTimeRequired;
        }


        public void checkUndocked()
        {

            if (!docked)
            {
                undockedRunningExp();
            }
        }

        public void checkBiomeChange()
        {
            double numExposureTime = getResourceAmount("ExposureTime");
            //int sciCount = GetScienceCount(); // MKW - what was this for?

            var subject = ScienceHelper.getScienceSubject(experimentID, vessel);
            string subjectId = ((subject == null) ? "" : subject.id);
            if (subjectId != "" && last_subjectId != "" && last_subjectId != subjectId &&
                (numExposureTime > 0))
            {
                biomeChanged();
            }
            last_subjectId = subjectId;
        }

        public void checkForLabs(bool ready)
        {
            if (!ready)
            {
                if (docked)
                {
                    this.labFound();
                }
                else
                {
                    notReadyStatus = Localizer.GetStringByTag("#ne_Not_installed_on_a_PEC");
                }
            }
            else
            {
                if (!docked)
                {
                    labLost();
                    notReadyStatus = Localizer.GetStringByTag("#ne_Not_installed_on_a_PEC");
                }
            }
        }

        public virtual void labLost()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            state = NOT_READY;
        }

        public virtual void biomeChanged()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage(Localizer.Format("#ne_Location_changed_mid_experiment_1_ruined", part.partInfo.title), 6, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = ERROR;
        }

        public virtual void undockedRunningExp()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage(Localizer.Format("#ne_Warning_1_has_detached_from_station_without_being_finalized",part.partInfo.title), 2, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = NOT_READY;
        }

        public virtual void labFound()
        {
            Events["StartExperiment"].active = true;
            Events["DeployExperiment"].active = false;
            state = READY;
        }

        public virtual bool experimentStarted()
        {
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = false;
                state = RUNNING;
                playAnimation(deployAnimation, 1, 0);
                return true;
        }

        public virtual void finished()
        {
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = deployChecks(false);
                state = FINISHED;
                completed = (float)Planetarium.GetUniversalTime();
        }

        public virtual void finalized()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = FINALIZED;
            playAnimation(deployAnimation, -1, 1);
        }

        public virtual void resetExp()
        {
            ScreenMessages.PostScreenMessage(Localizer.Format("#ne_Resetting_Experiment_1", part.partInfo.title), 2, ScreenMessageStyle.UPPER_CENTER);
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = NOT_READY;
        }

        public override string GetInfo()
        {
            String ret = Localizer.Format("#ne_Exposure_time_required_1", exposureTimeRequired);
            ret += "\n";
            ret += Localizer.GetStringByTag("#ne_You_need_to_install_the_experiment_on_a_KEES_PEC");

            return ret;
        }

        public UnityEngine.Object[] UnityFindObjectsOfType(Type type)
        {
            return GameObject.FindObjectsOfType(type);
        }


        public void dockedToPEC(bool docked)
        {
            this.docked = docked;
        }

        private void playAnimation(string animName, float speed, float time)
        {
            // MKW TODO - remove "FirstOrDefault" Linq statement
            Animation anim = part.FindModelAnimators(animName).FirstOrDefault();
            if (anim != null)
            {
                anim[animName].speed = speed;
                anim[animName].normalizedTime = time;

                anim.Play(animName);
            }
            else
            {
                NE_Helper.logError("no Animation; Name: " + animName);
            }
        }


        internal void pecDecoupled()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage(Localizer.Format("#ne_Warning_1_has_detached_from_the_vessel", part.partInfo.title), 2, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = ERROR;
            docked = false;
        }
    }
}
