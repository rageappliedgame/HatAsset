TwoA (formerly HAT) asset implementation v1.2.5

Authors: Enkhbold Nyamsuren
Organization: OUNL
Task: T3.4a
For any questions contact Enkhbold Nyamsuren via Enkhbold.Nyamsuren [AT] ou [DOT] nl

The solution consists of two projects:
	- 'manual' folder contains a detailed TwoA API manual
	- 'binary' folder contains release binaries of the TwoA as DLL
	- 'TwoA' should be integrated with the game engine to utilize TwoA functionality.
	- 'TwoA_Portable' provides portable version of the asset
	- 'TestApp' provides
		- a simple example for writing a Bridge object
		- a simple example for managing scenario and player data
		- a simple example for calling TwoA methods for requesting a recommended scenario
		- a simple example for calling TwoA methods for (re)assessing player and scenario ratings
		- a simple example of constructing a knowledge structure using difficulty ratings from the TwoA asset

The 'TestApp' project is not necessary and can be safely removed.

The TwoA asset dependent on the AssetManager package. You can download it from https://github.com/rageappliedgame/AssetManager

Refer to the software design document (https://rage.ou.nl/filedepot?fid=501) for more implementation and integration details.
Refer to the asset use case description (https://rage.ou.nl/filedepot?fid=502) for HAT asset application notes.
Refer to the manual (https://rage.ou.nl/filedepot?fid=503) on the accompanying widget for data management and analysis/visualization.

Summary of most important changes in the version 1.2.5 of the TwoA asset:
- Added a calibration phase: during the first 30 games, changes in player and/or scenario ratings are higher due to bigger K factor (this feature is not validated;  use with caution)
- Changed the name space for the TwoA asset to "TwoANS"
- Added an empty constructor for the TwoA class
- Removed empty constructors from the PlayerNode and ScenarioNode classes

Summary of most important changes in the version 1.2 of the TwoA asset:
- Added a second adaptation module that requires only player accuracy. Accuracy can be any value between 0 and 1.
- Remove dependency on external files. Now it is assumed that the game developer will add scenario and game data programatically instead of storing them in an xml file.
- Extended API for greater flexibitility of managing player and scenario data.
- Added methods to request recommended difficulty rating instead of scenario.
- Added a parameter (K factor) for scaling changes in ratings in reassessment methods.

