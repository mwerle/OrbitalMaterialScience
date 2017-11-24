NEOS Modelling
==============

Models for NEOS are made using Blender and stored in Git as Blender '.blend' files.

**Note:** The Blender file-format is a binary format.

The latest version of Blender used is 2.79.

Requirements
------------
* [Blender][1] - a free open-source drawing package

Inputs
------
None

Outputs
-------
* The model, in Blender format for saving
* The model, in ASCII FBX 6.1 format
  - This is for import into Unity
* The UV Layout, in PNG format for texturing

Procedure
---------
1. Create a new Blender project
   - Ensure the Scene Unit Presets have a Length of "None" or "Metric" with a Unit Scale of 1.0
1. Import a model or draw your own
   - Ensure the model is at coordinates 0,0,0 and set its origin appropriately
1. Create a UV Map
   - The [UV ALign/Distribute][2] plugin is useful to help arrange your UV map
   - Export the UV Layout as a PNG
1. Save the file
1. Export your model (File -> Export -> FBX)
   - Select FBX 6.1 ASCII
   - Tick "Selected Objects"
1. Save the file

 [1]: http://www.blender.org/
 [2]: https://github.com/c30ra/uv-align-distribute