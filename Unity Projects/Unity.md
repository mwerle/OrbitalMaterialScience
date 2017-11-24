Making a Part for KSP
=====================

The current version of KSP runs on Unity 5.4.0p4. For best results, it is recommended to use the same version of Unity for modelling.


Requirements
------------
* Unity 5.4.0p4
* PartTools

Inputs
------
* A model (ideally in FBX format)
* Texture and normal map (in PNG format)

Outputs
-------
* The model packed in '.mu' format
* The model textures in 'mbm' format

Checklist
---------
- [ ] Project configured for Version Control
  - [ ] Visible Meta Files
  - [ ] Asset Serialization : Force Text
- [ ] GameObject has PartTools script attached
  - [ ] Model Name is set to something unique (eg. prefix using your mod)
  - [ ] Texture Format is set to 'MBM'
- [ ] Model is child of GameObject
- [ ] Model is at position 0,0,0 with scale 1,1,1
- [ ] Model has a convex collider
- [ ] Model has a 'KSP' shader
- [ ] Texture converted to DDS format

Procedure
---------

1. Open Unity and create a new project
   - Open "Edit -> Project Settings -> Editor"
   - Set "Version Control" to "Visible Meta Files
   - Set "Asset Serialization" to "Force Text"
1. Import the PartTools package
1. Create a new "GameObject"
   - Add the *PartTools* script to the GameObject
1. Import Blender Model
   - If doing ASCII FBX import, set scale to 100
   - Select "Generate Colliders" (unless using your own collision mesh or using a simple box-mesh)
1. Drag model from Asset to GameObject
1. Set up material (use one of the "KSP" shaders!)
   - assign texture
1. Add collider (if not generated); ensure "Convex" is ticked
   - Best performance is a box collider
1. Export part 
   - on GameObject, Part Tools (script), fill out Model Name and URL (destination path)
   - select "MBM" for texture format
   - Click "Write"
1. Create part.cfg file
   - See the documentation and/or existing part definitions for examples
1. Use "DDS4KSP" utility to convert MBM texture to DDS
   - Try using DXT1 - it's much smaller if you're not using too many colours/alpha in your texture
   - Try reducing the texture size, unless you have fine detail or text, 256 or 512 textures should be more than enough
