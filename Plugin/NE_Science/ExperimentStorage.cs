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
using UnityEngine.UI;
using KSP.Localization;

namespace NE_Science
{
    public interface ExperimentDataStorage
    {
        void removeExperimentData();

        GameObject getPartGo();

        Part getPart();
    }

    public class ExperimentStorage : ModuleScienceExperiment, ExperimentDataStorage, IPartCostModifier, IPartMassModifier, IScienceResultHelperClient
    {

        [KSPField(isPersistant = false)]
        public string identifier = "";

        [KSPField(isPersistant = false)]
        public bool chanceTexture = false;

        [KSPField(isPersistant = true)]
        public string type = ExperimentFactory.OMS_EXPERIMENTS;

        //[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Contains")]
        //public string contains = "";

        private ExperimentData expData = ExperimentData.getNullObject();
        private int count = 0;

        private List<ExperimentData> availableExperiments = new List<ExperimentData>();
        List<Lab> availableLabs = new List<Lab>();

        private ExpContainerTextureFactory textureReg = new ExpContainerTextureFactory();
        private Material contMat;


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("ExperimentStorage: OnLoad");

            ConfigNode expNode = node.GetNode(ExperimentData.CONFIG_NODE_NAME);
            if (expNode != null)
            {
                setExperiment(ExperimentData.getExperimentDataFromNode(expNode));
            }
            else
            {
                setExperiment(ExperimentData.getNullObject());
            }
        }

        /// <summary>
        /// Sets or clears the stored experiment
        /// </summary>
        /// <param name="experimentData">Experiment data.</param>
        private void setExperiment(ExperimentData experimentData)
        {
            NE_Helper.log("MOVExp.setExp() id: " + experimentData.getId());
            expData = experimentData;
            //contains = expData.getAbbreviation();
            expData.setStorage(this);

            experimentID = expData.getId();
            if (expData.getId () == "") {
                experiment = null;
                ScienceResultHelper.Instance.Unregister(this);
            } else {
                experiment = ResearchAndDevelopment.GetExperiment (experimentID);
                ScienceResultHelper.Instance.Register(this);
            }

            experimentActionName = Localizer.GetStringByTag("#ne_Results");
            resetActionName = Localizer.GetStringByTag("#ne_Discard_Results");
            reviewActionName = Localizer.Format("#ne_Review_1_Results", expData.getAbbreviation());

            useStaging = false;
            useActionGroups = true;
            hideUIwhenUnavailable = true;
            resettable = false;
            resettableOnEVA = false;

            dataIsCollectable = false;
            collectActionName = Localizer.GetStringByTag("#ne_Collect_Results");
            interactionRange = 1.2f;
            xmitDataScalar = 0.05f;
            if (chanceTexture)
            {
                setTexture(expData);
            }
            RefreshMassAndCost();
        }

        public ExperimentData getStoredExperimentData()
        {
            return expData;
        }

        public override void OnSave(ConfigNode node)
        {
            try{
            if (node == null) {
                NE_Helper.logError ("OnSave: node is NULL!");
                //return;
            }
            base.OnSave(node);
            node.AddNode(expData.getNode());
            } catch(NullReferenceException nre) {
                NE_Helper.logError ("ExperimentStorage.OnSave - NullReferenceException:\n"
                    + nre.StackTrace);
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state.Equals(StartState.Editor))
            {
                Events["chooseEquipment"].active = true;
                if (expData.getId() == "")
                {
                    Events["chooseEquipment"].guiName = Localizer.GetStringByTag("#ne_Add_Experiment");
                }
                else
                {
                    Events["chooseEquipment"].guiName = Localizer.Format("#ne_Remove_1", expData.getAbbreviation());
                }
            }
            else
            {
                Events["chooseEquipment"].active = false;
            }
            Events["DeployExperiment"].active = false;
        }

        public override void OnUpdate()
        {

            base.OnUpdate();
            if (count == 0)
            {
                Events["installExperiment"].active = expData.canInstall(part.vessel);
                if (Events["installExperiment"].active)
                {
                    if (type == ExperimentFactory.KEMINI_EXPERIMENTS)
                    {
                        Events["installExperiment"].guiName = Localizer.Format("#ne_Run_1", expData.getAbbreviation());
                    }
                    else
                    {
                        Events["installExperiment"].guiName = Localizer.Format("#ne_Install_1", expData.getAbbreviation());
                    }
                }
                Events["moveExp"].active = expData.canMove(part.vessel);
                if (Events["moveExp"].active)
                {
                    Events["moveExp"].guiName = Localizer.Format("#ne_Move_1", expData.getAbbreviation());
                }
                Events["finalize"].active = expData.canFinalize();
                if (Events["installExperiment"].active)
                {
                    Events["finalize"].guiName = Localizer.Format("#ne_Finalize_1", expData.getAbbreviation());
                }
                Events["DeployExperiment"].active = false;
            }
            count = (count + 1) % 3;

        }

        public new void DeployExperiment()
        {
            if (expData.canFinalize())
            {
                base.DeployExperiment();
                // TODO: If we don't suppress the 'reset' button and instead hook into it,
                // let's now check whether we should actually finalize the experiment or
                // do something else.
                expData.finalize();
            }
            else
            {
                string s = Localizer.Format("#ne_Experiment_1_is_not_finished_Run_the_experiment_first", expData.getAbbreviation());
                ScreenMessages.PostScreenMessage(s, 6, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public new void ResetExperiment()
        {
            NE_Helper.log("ResetExperiment");
            base.ResetExperiment();
        }

        public new void ResetExperimentExternal()
        {
            NE_Helper.log("ResetExperimentExpernal");
            base.ResetExperimentExternal();
        }

        public new void ResetAction(KSPActionParam p)
        {
            NE_Helper.log("ResetAction");
            base.ResetAction(p);
        }

        public new void DumpData(ScienceData data)
        {
            NE_Helper.log("DumpData");
            base.DumpData(data);
        }

        [KSPEvent(guiActiveEditor = true, guiName = "#ne_Add_Experiment", active = false)]
        public void chooseEquipment()
        {
            if (expData.getId() == "")
            {
                availableExperiments = ExperimentFactory.getAvailableExperiments(type);
                showAddWindow();
            }
            else
            {
                removeExperimentData();
                Events["chooseEquipment"].guiName = Localizer.GetStringByTag("#ne_Add_Experiment");
            }
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Install_Experiment", active = false)]
        public void installExperiment()
        {
            // TODO: Refactor and use the "move" logic
            // Idea is that if the target is another storage container, the experiment is moved,
            // and if it is a lab with suitable equipment, then it is installed
            availableLabs = expData.getFreeLabsWithEquipment(part.vessel);
            if (availableLabs.Count > 0)
            {
                if (availableLabs.Count == 1)
                {
                    installExperimentInLab(availableLabs[0]);
                }
                else
                {
                    showLabWindow();
                }
            }
            else
            {
                NE_Helper.logError("Experiment install: No lab found");
            }
        }

        private void installExperimentInLab(Lab lab)
        {
            lab.installExperiment(expData);
            removeExperimentData();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_Experiment", active = false)]
        public void moveExp()
        {
            expData.move(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Finalize_Experiment", active = false)]
        public void finalize()
        {
            if (type == ExperimentFactory.KEMINI_EXPERIMENTS)
            {
                DeployExperiment();
            }
            else
            {
                showFinalizeWarning();
            }
        }

        void OnGUI()
        {
        }

        private void showLabWindow()
        {
            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            dialog.Add(new DialogGUILabel("#ne_Chooose_Lab"));

            // Build a button list of all available experiments with their descriptions
            int numLabs = availableLabs.Count;
            DialogGUIBase[] scrollList = new DialogGUIBase[numLabs + 1];
            scrollList[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int idx = 0; idx < numLabs; idx++)
            {
                var lab = availableLabs[idx];
                var button = new DialogGUIButton<Lab>(lab.abbreviation, installExperimentInLab, lab, true);
                button.size = new Vector2(60, 30);
                var label = new DialogGUILabel(lab.GetInfo(), true, true);
                var h = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { button, label });

                scrollList[idx + 1] = h;
            }

#if true
            dialog.Add(new DialogGUIScrollList(new Vector2(200,300), false, true, //Vector2.one, false, true,
                new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.UpperLeft, scrollList)
            ));
#else
            dialog.Add( new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleLeft, scrollList) );
#endif

            // Add a centered "Cancel" button
            dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBase[]
            {
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("#ne_Cancel", null, true),
                new DialogGUIFlexibleSpace(),
            }));

            // Actually create and show the dialog
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("", "", "#ne_Install_Experiment", HighLogic.UISkin, dialog.ToArray()),
                false, HighLogic.UISkin);

        }

        private void closeGui()
        {
            resetHighlight();
        }

        private void resetHighlight()
        {
            for (int idx = 0, count = availableLabs.Count; idx < count; idx++)
            {
                availableLabs[idx].part.SetHighlightDefault();
            }
        }

        private void showFinalizeWarning()
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog(
                    "",
                    "#ne_You_can_no_longer_move_the_experiment_after_finalization",
                    Localizer.Format("#ne_Finalize_1_Experiment", expData.getAbbreviation()),
                    HighLogic.UISkin,
                    new DialogGUISpace(4),
                    new DialogGUIHorizontalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("#ne_Cancel", null, 60, 30, true),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("#ne_OK", DeployExperiment, 60, 30, true),
                        new DialogGUIFlexibleSpace()
                    )
                ),
                false,
                HighLogic.UISkin);
        }

        // TODO: Complete implementation
        private void showAddWindow()
        {
            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            var noPad = new RectOffset();
            DialogGUIButton b;
            DialogGUILabel l;
            DialogGUIHorizontalLayout hl;
            DialogGUIVerticalLayout vl;

            // Window Contents - scroll list of all available experiments with their descriptions
            vl = new DialogGUIVerticalLayout(true, false);
            vl.padding = new RectOffset(6, 24, 6, 6); // Padding between border and contents - ensure we don't overlay content over scrollbar
            vl.spacing = 4; // Spacing between elements
            vl.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            int numExperiments = availableExperiments.Count;
            for (int idx = 0; idx < numExperiments; idx++)
            {
                var e = availableExperiments[idx];
                b = new DialogGUIButton<ExperimentData>(e.getAbbreviation(), onAddExperiment, e, true);
                b.size = new Vector2(60, 30);
                l = new DialogGUILabel(e.getDescription(), true, false);
                hl = new DialogGUIHorizontalLayout(false, false, 4, new RectOffset(), TextAnchor.MiddleCenter, b, l);

                vl.AddChild(hl);
            }

            hl = new DialogGUIHorizontalLayout(true, true, new DialogGUIScrollList(Vector2.one, false, true, vl));
            dialog.Add(hl);

            // Add a centered "Cancel" button
            dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBase[]
            {
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("#ne_Cancel", null, true),
                new DialogGUIFlexibleSpace(),
            }));

            // Actually create and show the dialog
            Rect pos = new Rect(0.5f, 0.5f, 400, 400);
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("", "", "#ne_Add_Experiment", HighLogic.UISkin, pos, dialog.ToArray()),
                false, HighLogic.UISkin);
        }

        private void onAddExperiment(ExperimentData e)
        {
            setExperiment(e);
            Events["chooseEquipment"].guiName = Localizer.Format("#ne_Remove_1", e.getAbbreviation());
        }

        public bool isEmpty()
        {
            return expData == null || expData.getId() == "";
        }

        internal void storeExperiment(ExperimentData experimentData)
        {
            setExperiment(experimentData);
        }

        public void removeExperimentData()
        {
            setExperiment(ExperimentData.getNullObject());
        }

        public GameObject getPartGo()
        {
            return part.gameObject;
        }

        private void setTexture(ExperimentData expData)
        {
            GameDatabase.TextureInfo tex = textureReg.getTextureForExperiment(expData);
            if (tex != null)
            {
                changeTexture(tex);
            }
            else
            {
                NE_Helper.logError("Change Experiment Container Texure: Texture Null");
            }
        }

        private void changeTexture(GameDatabase.TextureInfo newTexture)
        {
            Material mat = getContainerMaterial();
            if (mat != null)
            {
                mat.mainTexture = newTexture.texture;
            }
            else
            {
                NE_Helper.logError("Transform NOT found: " + "ESC1");
            }
        }

        private Material getContainerMaterial()
        {
            if (contMat == null)
            {
                Transform t = part.FindModelTransform("ESC1");
                if (t != null)
                {
                    contMat = t.GetComponent<Renderer>().material;
                    return contMat;
                }
                else
                {
                    NE_Helper.logError("Experiment Container Material null");
                    return null;
                }
            }
            else
            {
                return contMat;
            }
        }

        /// <summary>Refresh cost and mass</summary>
        public void RefreshMassAndCost()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
        }

        #region IPartMassModifier interface
        /// <summary>Overridden from IPartMassModifier</summary>
        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        /// <summary>Overridden from IPartMassModifier</summary>
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return (expData != null)? expData.getMass() : 0f;
        }
        #endregion

        #region IPartCostModifier interface
        /// <summary>Overridden from IPartCostModifier</summary>
        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        /// <summary>Overridden from IPartCostModifier</summary>
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return (expData != null)? expData.getCost() : 0f;
        }
        #endregion

        #region ExperimentResultDialog interface and callbacks
        /// <summary>
        /// ExperimentResultHelperClient interface
        /// </summary>
        public virtual Part getPart()
        {
            return part;
        }
        /** Default implementation; does nothing. */
        public virtual void OnExperimentResultDialogResetClicked()
        {
            NE_Helper.log("Lab: OnExperimentResultDialogResetClicked()");
        }
        /** Default implementation; disabled 'Reset' button. */
        public virtual void OnExperimentResultDialogOpened()
        {
            NE_Helper.log("Lab: OnExperimentResultDialogOpened()");
            ScienceResultHelper.Instance.DisableButton(ScienceResultHelper.ExperimentResultDialogButton.ButtonReset);
        }
        /** Default implementation; does nothing. */
        public virtual void OnExperimentResultDialogClosed()
        {
            NE_Helper.log("Lab: OnExperimentResultDialogClosed()");
        }
        #endregion
    }

    class ExpContainerTextureFactory
    {
        private Dictionary<string, GameDatabase.TextureInfo> textureReg = new Dictionary<string, GameDatabase.TextureInfo>();
        private Dictionary<string, KeyValuePair<string, string>> textureNameReg = new Dictionary<string, KeyValuePair<string, string>>() {
        { "", new KeyValuePair<string,string>("NehemiahInc/MultiPurposeParts/Parts/ExperimentContainer/","ESC1Texture")},
        { "FLEX",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","FlexContainerTexture") },
        { "CFI",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","CfiContainerTexture") },
        { "CCF",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","CcfContainerTexture" )},
        { "CFE",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/", "CfeContainerTexture" )},
        { "MIS1",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/", "Msi1ContainerTexture") },
        { "MIS2",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/", "Msi2ContainerTexture") },
        { "MIS3",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/", "Msi3ContainerTexture") },
        { "MEE1",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","Mee1ContainerTexture" )},
        { "MEE2",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","Mee2ContainerTexture") },
        { "CVB",  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/","CvbContainerTexture") },
        { "PACE", new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/ExperimentContainer/", "PACEContainerTexture")},
        { "ADUM",  new KeyValuePair<string,string>("NehemiahInc/KerbalLifeScience/Parts/ExperimentContainer/","AdumContainerTexture") },
        { "SpiU", new KeyValuePair<string,string>("NehemiahInc/KerbalLifeScience/Parts/ExperimentContainer/", "SpiuContainerTexture") }};


        internal GameDatabase.TextureInfo getTextureForExperiment(ExperimentData expData)
        {
            GameDatabase.TextureInfo tex;
            if (textureReg.TryGetValue(expData.getType(), out tex))
            {
                return tex;
            }
            else
            {
                NE_Helper.log("Loading Texture for experiment: " + expData.getType());
                GameDatabase.TextureInfo newTex = getTexture(expData.getType());
                if (newTex != null)
                {
                    textureReg.Add(expData.getType(), newTex);
                    return newTex;
                }
            }

            return null;
        }

        private GameDatabase.TextureInfo getTexture(string p)
        {
            KeyValuePair<string, string> textureName;
            if (textureNameReg.TryGetValue(p, out textureName))
            {
                NE_Helper.log("Looking for Texture:" + textureName.Value + " in : " + textureName.Key);
                GameDatabase.TextureInfo newTex = GameDatabase.Instance.GetTextureInfoIn(textureName.Key, textureName.Value);
                if (newTex != null)
                {
                    return newTex;
                }
            }
            NE_Helper.logError("Could not load texture for Exp: " + p);
            return null;
        }
    }
}
