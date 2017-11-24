NEOS Texturing
==============

Textures for NEOS are made using GIMP and stored in Git as GIMP XCF files.

Each XCF is a multi-layered file containing, at a minimum, the UV Layout layer and the actual texture. In most cases it will also contain a normal map layer.

**Note:** The XCF file-format is a binary format and various features may not be compatible with future versions of GIMP.

The latest version of GIMP used is 2.8.22.

Requirements
------------
* [GIMP][1] - a free open-source drawing package
* [gimp-normalmap][2] - a plugin for GIMP to create normal maps.

Inputs
------
* UV Layout 
  - generated from Blender / your modelling package
  
Outputs
-------
Outputs can be saved directly into the "Unity Projects" folder into the relevant Asset directory.

* The texture, in PNG format
* (Optional) The normal map, also in PNG format

Procedure
---------
1. Open the UV Layout in GIMP
   - Name the layer UV_Layout
1. Import a texture or draw your own
   - Name the layer Texture
1. Duplicate the Texture layer
   - Name it Texture_Cropped
   - Overlay the UV_Layout layer then crop the texture to only cover the areas of the UV layout
1. Duplicate the Texture_Cropped layer
   - Name it Normal_Map
   - Desaturate the colours (Colors -> Desaturate)
   - Create Normals (Filters -> Map -> Normalmap)
1. Save the file
1. Export the texture and normal map as PNG files
   - Select the layer(s) you want to save
   - Export as PNG (File -> Export As)

 [1]: http://www.gimp.org/
 [2]: https://code.google.com/archive/p/gimp-normalmap/