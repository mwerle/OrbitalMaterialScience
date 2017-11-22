# NEOS - Contributing
**N**ehemia **E**ngineering **O**rbital **S**cience - previously knows as OrbitalMaterialScience (OMS).

This document outlines some information useful if you wish to contribute to this mod. Any and all help, especially modelling, is greatly appreciated.


## Sources
Presumably you've already found this, but the source code for this mod is maintained on GitHub.  The original code is in [Nehemia's repository][10], whereas this current fork is maintained in [Micha's repository][11]

[10]: https://github.com/N3h3miah/OrbitalMaterialScience
[11]: https://github.com/mwerle/OrbitalMaterialScience

## Plugin

The plugin source code solution is in the **Plugin** directory.  The source has been developed on both Windows using [Visual Studio][20] and Linux using [MonoDevelop][21].

Eventual output is to the GameData/NehemiaInc/NE_Science_Common/Plugins directory.

### Project File
Every attempt has been made to keep the project file platform-agnostic. Please keep it this way if making changes to it.

The post-build script is using Xbuild commands; if you need to edit or add to this, you must edit the project file directly as there is no IDE support for this.

### Project User Macros
There are two user-macros which define some paths, they are:
* KSP_DIR - the root-directory of the target KSP install you wish to compile against
* PDB2MDB_TOOL - the full path to the [pd2mdb.exe utility][15]

Both of these macros are defined in the project file; PLEASE DO NOT EDIT THESE!

To override these macros to suit your own build environment, copy or merge the
"NE_Science.csproj.user.template" to "NE_Science.csproj.user" and edit it
before loading the solution.

[15]: https://forum.kerbalspaceprogram.com/index.php?showtopic=102909


### Source code formatting

When providing patches or pull requests, please adhere to the existing source code formatting in the source files.  Some files may have different formatting, but in general the following conventions are used in this mod:

* Indentation: 4 spaces
* Layout:
  * Opening braces on new line
  * Space between if/for/while and opening bracket
  * No space between function/method call and opening bracket
  * No trailing whitespace

To assist in keeping the formatting consistent, a [.editorconfig][22] file has been provided. There are plugins for most editors/IDEs to support this and using them is encouraged.

Any requests to change the formatting over the entire project will be ignored.

[20]: https://www.visualstudio.com/free-developer-offers/
[21]: http://www.monodevelop.com/
[22]: http://editorconfig.org/

## Models
Models are, surprisingly, maintained in the **Models** directory.

The modelling tool used to create these is [Blender][30]. For consistencies sake, any new models should also be created using Blender, but ultimately there is no requirement to actually do so.

**IMPORTANT:** Models are saved (for import into Unity) in **ASCII FBX format** for ultimate interoperability and kindness to Git.

[30]: https://www.blender.org/

## Unity Projects
The **Unity Projects** directory contains the Unity configuration for all parts used in NEOS.

The projects are in the process of being updated to Unity **5.4.0p4**, which can be downloaded directly from the [Unity website][40].

Each project contains the [PartTools][41] plugin, which must be added to any new projects. Older projects are in the process of being upgraded from PartTools 0.23.

For the sake of interoperability, the *PartTools.cfg* file should be edited manually to provide a relative path to the *GameData* directory, which should be set to be the one at the root directory of NEOS.

**IMPORTANT:** Ensure that each project is configured to use *"Visible Meta Files"* (Edit -> Project Settings -> Editor) and that the meta-files are added to Git.  The "Library" folder should not be added! Furthermore the *"Asset Serialisation"* setting should be set to *"Force Text"*. 

### Textures
Model textures must be exported in the **MDM** format to ensure that they can be replaced with other formats with different file extensions.

**IMPORTANT:** Only the textures which are actually attached to the model are exported by *PartTools*. As each experiment has its own texture which overwrites the default model texture, the experiment textures must be manually copied into the output directory if they have been modified.

After generating the final files, textures should be converted to **DDS** format. While various utilities exist, the easiest by far is the [KSP to DDS textured converter][42] although it is currently only available for Windows.

### KSPedia
The KSPedia unity project additionally requires the "TextMeshPro" utility and a small patch available on the forum. Both of these should already be present if you clone this repository.

Please see DMagic's excellent [forum post][43] for more details on how to create KSPedia articles.

[40]: https://unity3d.com/unity/qa/patch-releases?version=5.4
[41]: http://forum.kerbalspaceprogram.com/index.php?showtopic=135228
[42]: http://forum.kerbalspaceprogram.com/index.php?showtopic=88972
[43]: http://forum.kerbalspaceprogram.com/index.php?showtopic=137628

## Part Configurations
The actual KSP Part and Experiment Configurations are maintained directly in the **GameData** directory.

## Localization
The localization files are also maintained directly in the **GameData** directory.

## Making a Release
Version information is maintained in various **.version** files in the **GameData** directory hierarchy. Update as required; at the very least the "Build Version" should be incremented.

Each component of NEOS is versioned individually, the idea being that updating part of the mod would only require downloading that part. This is especially important for people on slow internet connections and could be leveraged by [CKAN][60].

To make a release, a [NAnt build script][61] is provided. Edit this to update the version numbers, then run **nant** in the top-level directory to generate the various release files.

[60]: http://forum.kerbalspaceprogram.com/index.php?showtopic=90246
[61]: http://nant.sourceforge.net/
