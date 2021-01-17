# Ruler of the Plane

Our game is called Castle Crushers, and our game files can be found in 
unity\Assets\CastleCrushers and unity\Scripts\CastleCrushers. All of our modifications can be found inside these subdirectories.

Debug features:
- press 's' to show computed solution
- press 'i' to show sweep line input (yellow) and events (green, red, blue).
- press spacebar to skip a level in endless

Custom levels can be created from ipe files. A wall is created by drawing individual line segments with the polygon tool, meaning every polygon you draw must consist of two points. The maximum number of shots is determined by the number after the underscore in the filename. As the interesting algorithms are only used for level generation, creating levels manually mostly serves as a fun extra feature.

Have fun!