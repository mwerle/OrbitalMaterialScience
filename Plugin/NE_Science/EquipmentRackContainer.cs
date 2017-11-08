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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace NE_Science
{
    class EquipmentRackContainer : PartModule, IPartCostModifier, IPartMassModifier
    {
        private const float EMPTY_MASS = 0.4f;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor= true ,guiName = "#ne_Contains")]
        public string status = "";

        private Material contMat = null;

        private LabEquipment leq = LabEquipment.getNullObject();

        private EquipmentContainerTextureFactory texFac = new EquipmentContainerTextureFactory();
        private List<LabEquipment> availableRacks = new List<LabEquipment>();

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            ConfigNode eqNode = node.GetNode(LabEquipment.CONFIG_NODE_NAME);
            if (eqNode != null)
            {
                setEquipment(LabEquipment.getLabEquipmentFromNode(eqNode, null));
            }
            else
            {
                setEquipment(LabEquipment.getNullObject());
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.AddNode(leq.getNode());
        }

        private void setEquipment(LabEquipment er)
        {
            leq = er;
            status = leq.getName();
            if (leq.getType() == EquipmentRacks.NONE)
            {
                Events["chooseEquipment"].guiName = Localizer.GetStringByTag("#ne_Add_Lab_Equipment");
            }
            else
            {
                Events["chooseEquipment"].guiName = Localizer.GetStringByTag("#ne_Remove_Equipment");
            }
            RefreshMassAndCost();
            setTexture(leq);
        }

        private void setTexture(LabEquipment type)
        {
            GameDatabase.TextureInfo tex = texFac.getTextureForEquipment(type.getType());
            if (tex != null)
            {
                changeTexture(tex);
            }
            else
            {
                NE_Helper.logError("Change Equipment Container Texure: Texture Null");
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state.Equals(StartState.Editor))
            {
                Events["chooseEquipment"].active = true;
            }
            else
            {
                Events["chooseEquipment"].active = false;
            }
        }

        [KSPEvent(guiActiveEditor = true, guiName = "#ne_Add_Lab_Equipment", active = false)]
        public void chooseEquipment()
        {
            if (leq.getType() == EquipmentRacks.NONE)
            {
                availableRacks = EquipmentRackRegistry.getAvailableRacks();
                showAddGui();
            }
            else
            {
                setEquipment(LabEquipment.getNullObject());
                Events["chooseEquipment"].guiName = Localizer.GetStringByTag("#ne_Add_Lab_Equipment");
            }
        }

        void OnGUI()
        {
        }

        /* +------------------------------------------------+
         * |                 Add Lab Equipment              |
         * +------------------------------------------------+
         * | | [3DPR] 3D Printer                        |^| |
         * | |                                          | | |
         * | |                                          | | |
         * | |                                          | | |
         * | |                                          |v| |
         * |                  [Close]                       |
         * +------------------------------------------------+
        */
        private void showAddGui()
        {
            // TODO: Add tool-tip or description about which lab the equipment requires

            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            var noPad = new RectOffset();
            DialogGUIButton b;
            DialogGUILabel l;
            DialogGUIHorizontalLayout hl;
            DialogGUIVerticalLayout vl;

            // Window Contents - scroll list of available and tested Kerbals
            vl = new DialogGUIVerticalLayout(true, false);
            vl.padding = new RectOffset(6, 24, 6, 6); // Padding between border and contents - ensure we don't overlay content over scrollbar
            vl.spacing = 4; // Spacing between elements
            vl.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            for (int idx = 0, count = availableRacks.Count; idx < count; idx++)
            {
                var e = availableRacks[idx];

                b = new DialogGUIButton<LabEquipment>(e.getAbbreviation(), onAddEquipment, e, true);
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
                new MultiOptionDialog("", "", "#ne_Add_Lab_Equipment", HighLogic.UISkin, pos, dialog.ToArray()),
                false, HighLogic.UISkin);
        }

        private void onAddEquipment(LabEquipment e)
        {
            setEquipment(e);
        }

        public EquipmentRacks getRackType()
        {
            return leq.getType();
        }

        public LabEquipment install()
        {
            LabEquipment ret = leq;
            setEquipment(LabEquipment.getNullObject());
            return ret;
        }

        public override string GetInfo()
        {
            return Localizer.GetStringByTag("#ne_Choose_from_the_available_lab_equipment");
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
                NE_Helper.logError("Transform NOT found: " + "Equipment Container");
            }
        }

        private Material getContainerMaterial()
        {
            if (contMat == null)
            {
                Transform t = part.FindModelTransform("Container");
                if (t != null)
                {
                    contMat = t.GetComponent<Renderer>().material;
                    return contMat;
                }
                else
                {
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

        /// <summary>Overridden from IPartMassModifier</summary>
        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        /// <summary>Overridden from IPartMassModifier</summary>
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return (leq != null)? leq.getMass() : 0f;
        }

        /// <summary>Overridden from IPartCostModifier</summary>
        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        /// <summary>Overridden from IPartCostModifier</summary>
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return (leq != null)? leq.getCost() : 0f;
        }
    }

    class EquipmentContainerTextureFactory
    {
        private Dictionary<EquipmentRacks, GameDatabase.TextureInfo> textureReg = new Dictionary<EquipmentRacks, GameDatabase.TextureInfo>();
        private Dictionary<EquipmentRacks, KeyValuePair<string, string>> textureNameReg = new Dictionary<EquipmentRacks, KeyValuePair<string, string>>() {
        { EquipmentRacks.NONE, new KeyValuePair<string,string>("NehemiahInc/MultiPurposeParts/Parts/LabEquipmentContainer/", "ContainerTexture")},
        { EquipmentRacks.PRINTER, new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/","Container3PR_Texture") },
        { EquipmentRacks.CIR,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerCIR_Texture") },
        { EquipmentRacks.FIR,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerFIR_Texture") },
        { EquipmentRacks.MSG,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerMSG_Texture") },
        { EquipmentRacks.EXPOSURE, new KeyValuePair<string,string>("NehemiahInc/MultiPurposeParts/Parts/LabEquipmentContainer/", "ContainerTexture") },
        { EquipmentRacks.USU,  new KeyValuePair<string,string>("NehemiahInc/KerbalLifeScience/Parts/LabEquipmentContainer/", "ContainerUSU_Texture" )}};


        internal GameDatabase.TextureInfo getTextureForEquipment(EquipmentRacks type)
        {
            GameDatabase.TextureInfo tex;
            if (textureReg.TryGetValue(type, out tex))
            {
                return tex;
            }
            else
            {
                NE_Helper.log("Loading Texture for experiment: " + type);
                GameDatabase.TextureInfo newTex = getTexture(type);
                if (newTex != null)
                {
                    textureReg.Add(type, newTex);
                    return newTex;
                }
                else
                {
                    NE_Helper.logError("Texture for: " + type + " not found try to return default texture");
                    newTex = getTexture(EquipmentRacks.NONE);
                    return newTex;
                }
            }
        }

        private GameDatabase.TextureInfo getTexture(EquipmentRacks p)
        {
            KeyValuePair<string,string> textureName;
            if (textureNameReg.TryGetValue(p, out textureName))
            {
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
