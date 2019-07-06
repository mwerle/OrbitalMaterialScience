Changelog for Nehemiah Engineering Orbital Science
===========================================================

Major features are **bolded**, contributors are *emphasized*.

0.8.1 (for KSP 1.7)
-----------------------------------------------------------
### General
* Recompiled for KSP 1.7
* Added bulkhead profiles to all parts (fixes #38)
  + Compatible with taniwha's Custom Bulkhead Profiles mod.
* Disabled the "Reset" button of the Experiment Results Dialog for all experiments (fixes #8)
* Improved KAC alarm support, especially when pausing and loading the game (fixes #32)

### OSS
* Only play IVA sounds if we're in IVA and in the part causing the sounds.

### MEP
* Pausing the experiment now causes it to retract the experiment (fixes #42).
* Experiment actions can now only be performed if a Kerbal is in the MEP.
* Bugs fixed:
  + KAC Alarms not created properly for MEP experiments.
  + Fixed description to add MSL-1000 or MPL-600 labs as requirements for MEP experiments.

### Kemini:
* Removed Kenmini from Pea pod as it caused the pod to fall off its decoupler.


0.8.0-3 (for KSP 1.7) - 2019.06.22
-----------------------------------------------------------
### General
 * Marked as compatible with KSP 1.7.x


0.8.0-2 (for KSP 1.6) - 2019.04.01
-----------------------------------------------------------
### General
* Marked as compatible with KSP 1.6.x


0.8.0 (for KSP 1.5) - 2018.06.10
-----------------------------------------------------------
### General
* Updated ModuleManager to 3.1.0
* Updated MiniAVC to v1.2.0.2
* Display experiment time in the VAB. Bug #26.

### OMS
* Added support Universal Storage 2 and fixed Universal Storage 1.
* Bugs fixed:
  + Fix "double pressing" to add experiment in VAB. Bug #31.
  + Fix "missing experiments on revert" in VAB. Bug #30.
  + Fix Extraplanetary Launchpads support. Bug #27.
  + Fix missing results for some experiments. Bug #23.

### KEES
* Bugs fixed:
  + Spelling mistake in the PPMD results.

### Kemini
* Clean up GUI - remove some unnecessary buttons
* Mk1 Pod only has 1 experiment if MakingHistory is installed
* Add experiments to addition MakingHistory pods.
* Bugs fixed:
  + Fix "double pressing" to add experiment in VAB. Bug #31.
  + Fix "missing experiments on revert" in VAB. Bug #30.


0.7.2 (recompile) - 2018.06.10
-----------------------------------------------------------
### General
* Updated ModuleManager from 2.8.1 to the newly-released 3.0.7
* Recompile for KSP1.4.3
* Added Kerbal Alarm Clock integration support.
  + When starting an experiment and KAC is installed, a KAC alarm is created which triggers
    when the experiment is complete.
  + A setting is added to the "Difficulty" menu of the game allowing this functionality to
    be en-/dis-abled, and to specify an alarm margin.


0.7.1 (HOTFIX) - 2017.12.03
-----------------------------------------------------------

### General
* Updated ModuleManager from 2.8.1 to the newly-released 3.0.0

### KRP
* Replace missing texture making the Kemini experiments show up in their full glory again.

### KEES
* Fix contract experiment recovery again (wasn't working for user-created parts).


0.7 - 2017.12.02
-----------------------------------------------------------

### General
* Add README and LICENSE files to release packages
* Add MiniAVC and ModuleManager DLLs to release packages

### KRP
* Revert Mk1 Pod change; it now has room for 2 experiments again

### KEES
* New models and textures for the PC and PEC (no more z-fighting on the PEC model)


0.7b3 (pre-release) - 2017.11.26
-----------------------------------------------------------

***NOTE:*** *This release changes the save-file format.*

### General
* **Add KSPedia entries**
* **Recompile for KSP 1.3.1**
* Remove spaces from some part configurations
* Remove superfluous NE_XXX_LabEquipmentSlot node from OMS/KLS; savefiles should get updated automatically.
* Various minor tweaks and fixes
* **Mod Release layout change*** - merged KLS/OMS into OSS and only build release packages for the 3 major sub-components as well as the "all-in-one".
  * KRP - Kemini Research Program
  * KEES - Kerbal Environmental Effects Study
  * OSS - Orbital Station Science
  * NEOS - The new "All-in-one", Nehemiah Engineering Orbital Science

### KRP
* **Can now create new Kemini experiments by editing configuration  files**

### KEES
* **Can now create new KEES experiments by editing configuration files**
* Prevent exceptions if KEES is not installed (affects partial-installs of NEOS)
* Nerf the Mk1 pod to only have a single Kemini experiment slot

### OSS
* Remove "ModuleCommand" from science labs


0.7b2 
-----------------------------------------------------------

### General
* Localisation - spanish added (thanks to *fitiales*)
* Various tweaks and fixes to the localisation

### Kemini
* Added support for the Brumby pod (thanks *Sudragon*)
* Added support for the Bluedog Gemini pod (thanks *CobaltWolf*)

### KEES
* Fix description of POSA-II experiment


0.7b1
-----------------------------------------------------------

### General
* Various localisation tweaks and fixes

### OMS/KLS
* Tweak ESC3 - better flight characteristics and adjust bottom node so heat-shield doesn't clip through model.


0.7a0-mw9 - 2017.06.17
-----------------------------------------------------------

### General
* **Added localisation support**
  * For now only English and German languages
* GUI rework complete
  * No more "click-through"
  * Moving OMS/KLS experiments now works like stock crew transfer

### KEES
* Finally fixed experiment recovery bug


0.7a0-mw8 (PRE-RELEASE) - 2017.05.29
-----------------------------------------------------------

***NOTE:*** *Micha is now the official maintainer of this mod.*

### GENERAL
* **Recompiled for KSP1.3.0.1804**
* Bump and sanitise version numbers (build number is now unified across all components)
* Fix version files to work with (mini)AVC
* Performance improvements
* Adjust some part categories
* Increase crash tolerance of containers slightly
* Made a start on localisation support
* Start of new GUI

### KEES/KEMINI
* Fix contract generation
* Restrict contracts from spawning until a kerballed landing from orbit has been achieved
* Enable experimental parts for contracts
* Disable KEES debug menu in release builds


0.7a0-mw7 (PRE-RELEASE) (Unofficial release) - 2016.10.11
-----------------------------------------------------------

### GENERAL:
* **Recompile for KSP1.2 (build 1564)**
* Fix for Extraplanetary Launchpads
* Various fixes and tweaks required for KSP1.2
* Fix to only allow purchased parts when part-purchasing is enabled
* Fix adjusting weight & cost of container parts when equipment and experiments are added to the containers
* Add some extra tags to parts (KLS, OMS, NEOS - use to filter parts in VAB)
* Make save-file-parsing more robust

### KEMINI:
* contracts can now mark required parts/experiments as "experimental"

### OMS:
* Add ModuleDataTransmitter to ESC3 automated pod


0.7a0-mw6 (PRE-RELEASE) (Unofficial release) - 2016.04.29
-----------------------------------------------------------

* **Recompile for KSP 1.1.1**
* Merge in *Kerbas_ad_astra*'s fixes for empty experiment names


0.7a0-mw5 (PRE-RELEASE) (Unofficial release) - 2016.04.16
-----------------------------------------------------------

* Recompile and fix up for KSP 1.1-pre


0.7a0-mw4 (PRE-RELEASE) (Unofficial release) - 2015.07.01
-----------------------------------------------------------
* **Recompile for KSP 1.04**
* Minor tweak when recovering KEES experiments which may or may not be required to successfully trigger completion of KEES contracts.


0.7a0-mw3 (PRE-RELEASE) (Unofficial release) - 2015.05.13
-----------------------------------------------------------
* Fix KEES contract completion when recovering experiments
* Remove KIS "carry" flag from KEES items; instead bump up their size so only 1 can be in a Kerbal inventory at a time, and only 2 can be stored in the KEES PEC.
* Fix UniversalStorage wedge to reference US's KIS wedge instead of the no-longer existing KAS wedge


0.7a0-mw2 (PRE-RELEASE) (Unofficial release) - 2015.05.03
-----------------------------------------------------------
* Convert textures to DDS
* Move KEES Payload Carrier to "Basic Science" node


0.7a0-mw1 (PRE-RELEASE) (Unofficial release) - 2015.05.02
-----------------------------------------------------------
* **Recompiled using KSP v1.0.2 and KIS 1.1** (minor API fixes to replace deprecated functions)
* Converted KEES experiments from KAS to KIS
* Adjusted heat of parts to match stock values
* Fixed a couple of attachment nodes


0.6.1 (no tag)
-----------------------------------------------------------
* Performance improvements


0.6b1 (PRE-RELEASE) - 2015.02.15
-----------------------------------------------------------
* **Research Program: Kemini**
  * Adds to experiment slots for Kemini experiments to the Command Pod mk1
  * Adds 5 new experiments
* **New Lab: MPL-600 Kolumbus**
* Lab equipment for MPL-600:
  * Microgravity Science Glovebox (MSG)
  * Ultrasound Unit (USU)
  * Experiments CCF and CFE are now MSG experiments
* New experiments for FIR:
  * Preliminary Advanced Colloids Experiment (PACE) and Constrained Vapor Bubble (CVB)
* **Research Program: Kerbal Research(KR):**
  * Uses Ultrasound Unit for 2 new experiments Advanced Diagnostic Ultrasound in Microgravity (ADUM) and Sonographic Astronaut Vertebral Examination (SpiU)
  * KR-experiments use Kerbals as test subjects, each step needs an other Kerbal e.g. ADUM needs 4 Kerbals
* Universal Storage ESC-Wedge
* Rebalanced tech tree: Most part moved up in the tech tree because Kemini and KEES fill the lower part.
* Enable parts and modules only when the requirement are met (KAS or Connected Living Space)


0.6a1 (PRE-RELEASE) - 2015.02.06
-----------------------------------------------------------
* Experiments are now in the category “none”: they show up in the tech tree but not in the standard part list in the VAB or SPH
* Generic experiment transport container: Used to transport and store experiments. Experiments can be moved between empty storage holds.
* Lab equipment rack are now in the category “none”: they show up in the tech tree but not in the standard part list in the VAB or SPH.
* Generic lab equipment container
* KEES PEC detaches from the craft if G-Forces are above 2.5g (designed for micro gravity)
* Configurable Debugging: Config file NehemiahInc/Resources/settings.cfg : “Debug = True” enabled Debug output and disables the boring check for experiments (allows starting experiments on the launch pad)


0.5.7 (no tag)
-----------------------------------------------------------
* IVA for MEP-825
* Improved IVA for MSL-1000.
* KSP Space Agency Logo used with permission of *Hayoo*
* New model for KEES-experiments
* New texture for POSA-II
* Changing texture format to mbm. Because of a bug in the tga loader. This increases the file size but not the memory size
* Fix airlock and ladder positions for MSL-1000 and MEP-825
* Contract system now supports other KAS container
* Contract system now supports recovery by StageRecovery (untested). Contribution by *whiteout1911*.
* New equipment carrier textures
* Fix for KEES experiment "Not Installed on a PEC" bug


0.5 - 2015.01.01
-----------------------------------------------------------
* **New science program: Kerbal Environmental Effects Study (KEES)** inspired by the Mir Environmental Effects Payload
* KEES Experiments:
  * Polished Plate Micrometeoroid and Debris (PPMD)
  * Orbital Debris Collector (ODC)
  * Passive Optical Sample Assemblies I (POSA I)
  * Passive Optical Sample Assemblies II (POSA II)
* Additional KEES Parts:
  * Passive Experiment Carrier (PEC) used to mount the experiment on the outside of the craft.
  * Payload Container (PC) can hold one PEC and one experiment
* Contracts for KEES
* Icons for Alternate Resource Panel


0.4 - 2014.12.27
-----------------------------------------------------------
* The MSL-1000 now only contains basic lab equipment. It weighs therefore much less and is easier to launch. Most experiments need additional equipment to run. This equipment is in transport containers and can be installed in the lab if the container is docked to the station.
* **3D Printing in Space**: New lab equipment and 3 new experiments for 3D printing in space.
* **Multi phase experiments**: Some experiments are split in multiple phases, each phase has different requirements.


0.3.90 - 2014.12.24
-----------------------------------------------------------
* Works without the Station Science Mod. You no longer need the Station Science Mod for some Experiments. This should help to maintain this mod.
* The Cool Flames Experiment is now called Cool Flames Investigation (CFI) after it's real life example.
* Configurations for Connected Living Space contribution by *micha*. Thanks!

0.3
-----------------------------------------------------------
* IVA for the MSL-1000
* Folding platform
* Airlock and handrails for the MEP-825
* There is a small possibility for a failure during robotic arm procedures. The probability does rise by use. EVA to the plattform to fix the problem.
* Multiple code refatorings
