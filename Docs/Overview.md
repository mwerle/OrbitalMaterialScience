# Nehemiah Engineering Orbital Science

## Major components

- Kemini
  - Tech-tree only parts: experiment boxes
- KEES
  - Proper parts: payload carrier and animated experiment parts
  - Tech-tree only parts: experiments
- OMS/KLS (OSS)
  - Proper parts: Labs with IVA, cargo parts
  - Tech-tree parts: Lab Equipment, Experiments
- Shared Resources, Science, etc

## Naming Scheme

- Parts: should have a fully-qualified naming scheme
  - NE_KEMINI_xxx
  - NE_KEES_xxx
  - OSS:
    - NE_LAB_xxx
    - NE_EQUIPMENT_xxx
    - NE_EXPERIMENT_xxx


- Blender:
  - Use simplified name; eg MSL1000, KEES_PEC, ...
- Materials:
  - eg, MSL1000_External, MSL1000_Internal, MSL1000_3d_Printer, ...
- Textures:
  - MSL1000_External, MSL1000_Internal, ...

Aim is to be able to quickly and uniquely identify things in Unity.


NOTES:
- some things need names which are referenced in the configuration files and/or
  in the plugin code.
- existing part names can't be changed without breaking save files..

-------------------------------------

## Existing Naming

**NOTE**: a lot of these names are saved in the savegames. Unless there is a
way to run a converter during game startup, we cannot rename stuff.

### Kemini
- Tech-tree parts:
  - NE_KeminiD5
  - NE_KeminiD7
  - NE_KeminiD8
  - NE_KeminiD10
  - NE_KeminiMSC3

- Modules
  - NE_ExperimentModule

- Resources:
  - Kemini_LabTime
    - RENAME: NE_Kemini_LabTime

- Experiment Definitions:
  - NE_KeminiD5
  - NE_KeminiD7
  - NE_KeminiD8
  - NE_KeminiD10
  - NE_KeminiMSC3

### KEES
- Parts:
  - NE_KEES_PC      (Payload Carrier; Cargo)
  - NE_KEES_PEC     (Payload Exposure Carrier)
  - NE_KEES_ODC     (Experiment; Lab)
  - NE_KEES_POSA1   (Experiment; Lab)
  - NE_KEES_POSA2   (Experiment; Lab)
  - NE_KEES_PPMD    (Experiment; Lab)

- Modules
  - KEESExperiment
  - KEES_Lab
  - KEES_PEC
  

  - Rename all to prefix with "NE_"

- Experiment Definitions:
  - NE_KEES_ODC
  - NE_KEES_POSA1
  - NE_KEES_POSA2
  - NE_KEES_PPMD

OSS:
- Tech-tree parts:
  - NE_ADUM            (Experiment)
  - NE_SpiU            (Experiment)
  - NE_MIS1            (Experiment)
  - NE_MIS2            (Experiment)
  - NE_MIS3            (Experiment)
  - NE_CFI             (Experiment)
  - NE_FLEX            (Experiment)
  - NE_CVB             (Experiment)
  - NE_PACE            (Experiment)
  - NE_MEE1            (Experiment)
  - NE_MEE2            (Experiment)
  - NE_CCFE            (Experiment)
  - NE_CFE             (Experiment)
  
  - NE_USU             (Lab Equipment)
  - NE_3PR             (Lab Equipment)
  - NE_CIR             (Lab Equipment)
  - NE_FIR             (Lab Equipment)
  - NE_MSG             (Lab Equipment)

- Parts:
  - NE_MPL             (MPL600; Lab)
  - NE_MEP             (MEP825; Lab)
  - NE_MSL             (MSL1000; Lab)

  - NE_Rack_Container  (Cargo - lab equipment)
  - NE_ESC1            (Cargo - experiment)
  - NE_ESC2            (Cargo - experiment)
  - NE_ESC3            (Cargo - experiment)
  - NE_ESC4            (Cargo - experiment)

- Internals
  - neMPL600IVA
  - neMEP825Internals
  - neMSL1000Internals
  

- Modules
  - NE_ExperimentModule
  - MPL_Module
    - RENAME: NE_MPL600_Module
  - ExperimentStorage
    - RENAME: NE_ExperimentStorage
  - ESCStorageManifest
    - RENAME: NE_StorageManifest
 - LabEquipmentModule
   - RENAME: NE_LabEquipmentModule

- Resources
  - CIRBurnTime
  - FIRTestRun
  - 3D_PrintLayer
  - MSG_Time
  - MSL_LabTime
  - ExposureTime
    - **NOTE**: This conflicts with other mods!!
    
    
--------------------------------------------------------------------------------

{ name = "GAME"
{ type = RESOURCE_DEFINITION, name = ExposureTime, ... }

--------------------------------------------------------------------------------

# User:Greys/The CFG File and ConfigNodes
< User:Greys

The CFG file is a plain text document storing information which has been serialized from ConfigNodes and can be loaded back into those CongifNodes. `ConfigNode` is a Type used throughout KSP's code to organize information. The key feature of Confignodes is that they can contain Values, which have Strings in them, and they can also contain other ConfigNodes, allowing for branching organization.

## CFG File Syntax

The information stored in a cfg file exists as two and only two structures, nodes and values, this is also true in the code. Nodes can contain values, but they can also contain other nodes.

```
NODE
{
    value = foo
    Value = bar
    node
    {
        value = baz
    }
}
```

It is important to note from the above example that both Nodes and Values are case sensitive, `NODE` and `node` are not the same, but also every Node is a discrete context, the value `value` from the node `node` does not conflict with either of the values in `NODE`. It is common, but not required, that all Nodes will be in ALL UPPERCASE, while all values will be in all lowercase, but even Squad deviates from this.

## Working with ConfigNodes

`ConfigNode` is a type which behaves similarly to a `Dictionary<string, string>` but can also contain other `ConfigNodes`, allowing them to be nested. The primary ways of interacting with them is via the methods `.GetConfigNode()`, `.GetConfigNodes()`, `.GetValue()`, and `.SetValue()`, but there are a lot of other methods. Several methods, such as `GetConfigNodes()`, return an array of ConfigNodes, and have to be put in an object of type `ConfigNode[]`.

It is important to keep in mind that all values stored in ConfigNodes are strings, when loading values you must parse them into whatever the intended type is.

KSP automatically loads all the confignodes defined in CFG files in the `/KSP/GameData/` folder into the `GameDatabase.Instance` object during loading, regardless of if any code wants them. ConfigNodes can also be found in `persistent.sfs` and `*.craft` files located in the `/KSP/saves/` folder, though the `.craft` files differ slightly in that they have values outside of the 'Top Level Node' of the file, KSP sets the entire file into a ConfigNode when it's loaded similarly to how part.cfg files worked prior to KSP 0.20.0

Unnecessary nodes and values do no harm at all, they do take up space in the RAM, but it's a very small amount.

ConfigNodes are by default references, so when you `GetConfigNode` and `SetValue`, that change is available to any code in the game which looks up that same ConfigNode, unless of course the ConfigNode is private. A ConfigNode can be duplicated by: `ConfigNode node = new confignodesomewhereelse.GetConfigNode("name of the node you want");`

## Bringing it all together

CFG files store the content of ConfigNodes in a plain text file so that it's easily accessible and editable. You can make a CFG file that contains any valid structure, regardless of it if gets used. Both in and out of code this information exists as string text, and everything is case sensitive. In many cases, but not all, a given Node will only be recognized by the code if it is inside a given other Node, an example of this is `MODULE`, which only counts if it's in a `PART` node.
