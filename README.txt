HAT asset implementation v1.0.

Authors: Enkhbold Nyamsuren, Wim van der Vegt
Organization: OUNL
Task: T3.4a
For any questions contact Enkhbold Nyamsuren via Enkhbold.Nyamsuren [AT] ou [DOT] nl

The solution consists of two projects:
	- 'HatAsset' should be integrated with the game engine to utilize HAT functionality.
	- 'TestApp' provides
		- a simple example for writing a Bridge for loading and saving file. 
		  This Bridge is necessary for HAT functionality and should be written by a game developer.
		- a simple example for calling the 'TargetScenarioID' and 'UpdateRatings' methods

The 'TestApp' project is not necessary and can be safely removed.

Additionally I have included the AssetManager.zip. This archive includes AssetManager package that was used to test the asset. 
Use this package only if current version of the AssetManager (https://github.com/rageappliedgame/AssetManager) is not compatible with the HAT asset.

For managing XML data files and visualizing/analyzing data, refer to the accompanying widget (https://github.com/rageappliedgame/HATWidget).

Refer to the software design document (https://rage.ou.nl/filedepot?fid=501) for more implementation and integration details.
Refer to the asset use case description (https://rage.ou.nl/filedepot?fid=502) for HAT asset application notes.
Refer to the manual (https://rage.ou.nl/filedepot?fid=503) on the accompanying widget for data management and analysis/visualization.
