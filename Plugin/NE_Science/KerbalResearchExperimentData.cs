using System;
using System.Collections.Generic;
using KSP.Localization;

namespace NE_Science
{
    public abstract class KerbalResearchExperimentData : MultiStepExperimentData<KerbalResearchStep>
    {
        private const string TEST_SUBJECTS_NEEDED = "SubjectsNeeded";

        private int testSubjectsNeeded;

        private Guid cachedVesselID;
        private int partCount;
        private MPL_Module[] physicsLabCache = null;

        protected KerbalResearchExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, float cost, int testSubjectsNeeded)
            : base(id, type, name, abb, eq, mass, cost, testSubjectsNeeded)
        {
            this.testSubjectsNeeded = testSubjectsNeeded;
        }

        /** Sets up the required number of test subjects */
        protected void setExperimentSteps(string resourceName, float resourceAmount)
        {
            for (int idx = 0; idx < steps.Length; idx++) {
                steps [idx] = new KerbalResearchStep(this, resourceName, resourceAmount, idx);
            }
        }

        public override ConfigNode getNode()
        {
            ConfigNode node =  base.getNode();

            node.AddValue(TEST_SUBJECTS_NEEDED, testSubjectsNeeded);
            return node;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            testSubjectsNeeded = NE_Helper.GetValueAsInt(node, TEST_SUBJECTS_NEEDED);
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }

        public override string getDescription(string linePrefix = "")
        {
            string desc = base.getDescription(linePrefix);
            desc += "\n" + linePrefix + Localizer.Format("#ne_Test_subjects_needed_1", getTestSubjectsNeeded());
            return desc;
        }

        public int getTestSubjectsNeeded()
        {
            return testSubjectsNeeded;
        }

        internal override string stateString()
        {
            string s = base.stateString();
            switch(state){
                case ExperimentState.RUNNING:
                    s += " " + getActiveStep().getSubjectName();
                    break;
                case ExperimentState.INSTALLED:
                    if (getAvailableLabCrewMembers().Count == 0)
                    {
                        s += " " + Localizer.GetStringByTag("#ne_No_test_subjects");
                    }
                    break;
            }

            return s;
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    if (isTestSubjectAvailable())
                    {
                        return Localizer.Format("#ne_Start_1", getAbbreviation());
                    }
                    else
                    {
                        return Localizer.Format("#ne_Show_1_Status", getAbbreviation());
                    }

                case ExperimentState.RUNNING:
                    if (getActiveStep().isResearchFinished())
                    {
                        return Localizer.Format("#ne_End_1_step_2", getAbbreviation(), getActiveStep().getSubjectName());
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        internal override void updateCheck()
        {
            base.updateCheck();
            if (state == ExperimentState.RUNNING)
            {
                List<string> crewMembers = getAllLabCrewMembers();
                if (!crewMembers.Contains(getActiveStep().getSubjectName()))
                {
                    #if DEBUG // Avoid expensive string operations in non-Debug builds
                    string debug = "";
                        foreach(string member in crewMembers){
                            debug += member + ", ";
                        }
                    NE_Helper.log("Aborting Crew Members: " + debug + "; Subject; " + getActiveStep().getSubjectName());
                    #endif
                    getActiveStep().abortStep();
                    state = ExperimentState.INSTALLED;
                }
            }
        }

        internal ChooseTestSubject getTestSubjectGuiComponent()
        {
            ChooseTestSubject t = store.getPartGo().GetComponent<ChooseTestSubject>();
            if (t == null)
            {
                t = store.getPartGo().AddComponent<ChooseTestSubject>();
            }
            return t;
        }

        internal bool isTestSubjectAvailable()
        {
            List<string> members = getAvailableLabCrewMembers();
            return members.Count > 0;
        }

        /// <summary>
        /// Returns lab crew members which are not currently part of an experiment.
        /// </summary>
        /// <returns>A (possibly empty) list of available lab crew members.</returns>
        internal List<string> getAvailableLabCrewMembers()
        {
            List<string> members = new List<string>();
            if( steps == null ) {
                NE_Helper.logError("getAvailableLabCrewMembers(): steps is null");
            }
            else if (state == ExperimentState.INSTALLED)
            {
                List<string> labCrew = getAllLabCrewMembers();
                for (int crewIdx = 0, crewCount = labCrew.Count; crewIdx < crewCount; crewIdx++)
                {
                    var crewMember = labCrew[crewIdx];
                    bool foundInStep = false;
                    for (int rsIdx = 0, rsCount = steps.Length; rsIdx < rsCount; rsIdx++)
                    {
                        var s = steps[rsIdx];
                        if( s == null )
                        {
                            NE_Helper.logError("getAvailableLabCrewMembers(): s is null");
                            continue;
                        }
                        if (s.getSubjectName() == crewMember)
                        {
                            if( s.getSubjectName() == null ) {
                                NE_Helper.logError("getAvailableLabCrewMembers(): s.getSubjectName is null");
                            }
                            foundInStep = true;
                        }
                    }
                    if (!foundInStep) members.Add(crewMember);
                }
            }
            return members;
        }

        /// <summary>
        /// Gets all lab crew members.
        /// </summary>
        /// <returns>A (possibly empty) list of lab crew members.</returns>
        internal List<string> getAllLabCrewMembers()
        {
            List<string> members = new List<string>();
            if (state == ExperimentState.INSTALLED || state == ExperimentState.RUNNING)
            {
                try {
                    if(store==null) {
                        NE_Helper.logError("getAllLabCrewMembers: store is null!");
                    }
                    Lab lab = ((LabEquipment)store).getLab();
                    if(lab==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab is null!");
                    }
                    if(lab.part==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab.part is null!");
                    }
                    if(lab.part.protoModuleCrew==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab.part.protoModuleCrew is null!");
                    }
                    for (int idx = 0, count = lab.part.protoModuleCrew.Count; idx < count; idx++)
                    {
                        var crewMember = lab.part.protoModuleCrew[idx];
                        members.Add(crewMember.name.Trim());
                    }
                } catch(NullReferenceException nre) {
                    NE_Helper.logError ("getAllLabCrewMembers: nullref!\n" + nre.StackTrace);
                }
            }
            return members;
        }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            if (physicsLabCache == null || cachedVesselID != vessel.id || partCount != vessel.parts.Count)
            {
                physicsLabCache = UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[];
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            for (int idx = 0, count = physicsLabCache.Length; idx < count; idx++)
            {
                var lab = physicsLabCache[idx];
                if (lab.vessel == vessel && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }
    }

    public class KerbalResearchStep : ResourceExperimentStep
    {
        private const string SUBJECT_NAME = "subject";
        private string subject = "";

        internal KerbalResearchStep(ExperimentData exp, string resName, float amount, int index)
            : base(exp, resName, amount, "KerbalResStep",  "name", index)
        {
        }

        public KerbalResearchStep(ExperimentData exp, string name, int index)
            : base(exp, "KerbalResStep", name, index)
        {
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            subject = node.GetValue(SUBJECT_NAME);
        }

        public override ConfigNode getNode()
        {
            ConfigNode node = base.getNode();

            node.AddValue(SUBJECT_NAME, subject);
            return node;
        }

        public string getSubjectName()
        {
            return subject;
        }

        public override void start(startCallback cbMethod)
        {
            ChooseTestSubject gui = ((KerbalResearchExperimentData)exp).getTestSubjectGuiComponent();
            gui.showDialog(((KerbalResearchExperimentData)exp).getAvailableLabCrewMembers(), (KerbalResearchExperimentData)exp, cbMethod);
        }

        internal void start(string crewMember, startCallback cbMethod)
        {
            base.start(cbMethod);
            subject = crewMember;
        }

        internal void abortStep()
        {
            NE_Helper.log("Abort Research");
            ScreenMessages.PostScreenMessage("Test subject left lab. Research aborted!", 6, ScreenMessageStyle.UPPER_CENTER);
            subject = "";
            ((LabEquipment)exp.store).setResourceMaxAmount(res, 0f);
        }
    }
}
