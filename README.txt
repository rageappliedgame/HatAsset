TwoA (formerly HAT) asset implementation v1.1.

Authors: Enkhbold Nyamsuren, Wim van der Vegt
Organization: OUNL
Task: T3.4a
For any questions contact Enkhbold Nyamsuren via Enkhbold.Nyamsuren [AT] ou [DOT] nl

The solution consists of two projects:
	- 'TwoA' should be integrated with the game engine to utilize TwoA functionality.
	- 'TwoA_Portable' provides portable version of the asset
	- 'TestApp' provides
		- a simple example for writing a Bridge for loading and saving file. 
		  This Bridge is necessary for HAT functionality and should be written by a game developer.
		- a simple example for calling the 'TargetScenarioID' and 'UpdateRatings' methods
		- a simple example of constructing a knowledge structure using difficulty ratings from the TwoA asset

The 'TestApp' project is not necessary and can be safely removed.

The TwoA asset dependent on the AssetManager package. You can download it from https://github.com/rageappliedgame/AssetManager
 
For managing XML data files and visualizing/analyzing data, refer to the accompanying widget (https://github.com/rageappliedgame/HATWidget).

Refer to the software design document (https://rage.ou.nl/filedepot?fid=501) for more implementation and integration details.
Refer to the asset use case description (https://rage.ou.nl/filedepot?fid=502) for HAT asset application notes.
Refer to the manual (https://rage.ou.nl/filedepot?fid=503) on the accompanying widget for data management and analysis/visualization.

Summary of most important changes from the previous version of the TwoA asset:
- 'calcTargetBeta' method used by the 'TargetScenarioID' method uses a new equation to calculate beta value: theta + Math.Log((1 - randomNum) / randomNum)
- seeds for a random variable in 'calcTargetBeta' are initialized based on system time to facilitate more randomness
- removed dependency on the Swiss package to support portable version of the asset
- added portable version of the asset
- the asset includes a new feature of building knowledge structures from scenario difficulty ratings; see code inside TwoA.KS and examples provided in TestApp
For more detaileg log of changes, refer to headers of source files.

