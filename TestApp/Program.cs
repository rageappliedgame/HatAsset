#region Header

/*
Copyright 2017 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Namespace: TestApp
Filename: Program.cs
*/

// Change history:
// [2016.10.06]
//      - [SC] updated used namespace to TwoA
//      - [SC] renamed variable 'hat' to 'twoA'
// [2016.11.14]
//      - [SC] deleted 'using Swiss';
// [2016.11.29]
//      - [SC] "gameplaylogs.xml" and "TwoAAppSettings.xml" are converted to embedded resources
//      - [SC] updated description of example output for 'UpdateRatings' annd 'TargetScenarioID' methods
//      - [SC] added 'testKnowledgeSpaceGeneration' method with examples of using knowledge space generator
// [2017.01.02]
//      - [SC] added 'TileZero' project
//      - [SC] added 'evaluateTileZeroAIDifficulty' method
//      - [SC] added 'doTileZeroSimulation' method
//      - [SC] added references to external libraries: 
//              'Microsoft.Msagl.dll'
//              'Microsoft.Msagl.Drawing.dll'
//              'Microsoft.Msagl.GraphViewerGdi.dll'
// [2017.01.03]
//      - [SC] added 'testScoreCalculations' method
// [2017.02.14]
//      - [SC] added 'testAdaptationAndAssessmentElo' method
//
//

// [TODO] need to change the program
// [TODO] add scenario and player methods in TwoA should return nodes or null instead of boolean 
// [TODO] add log show flag params in scenario and player getters


#endregion Header

namespace TestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using System.Diagnostics;

    // [SC] Load TwoA and Rage architecture
    using TwoANS;
    using AssetPackage;

    class Program {
        static void Main(string[] args) {
            // [SC] demo of usage of the adaptation module that requires player accuracy and response time measurements
            // [SC] player accuracy should be either 0 (fail) or 1 (success), and response time should be measured in milliseconds
            //demoAdaptationAndAssessment();

            // [SC] demo of usage of the adaptation module that requires player accuracy only
            // [SC] player accuracy can be any value between 0 and 1
            //printMsg("############################################################################");
            demoAdaptationAndAssessmentElo();

            // [SC] test universal score estimation based on player's accuracy and response time
            //printMsg("############################################################################");
            //testScoreCalculations();

            // [SC] test generation of a graph of dependencies among game scenarios based on their difficulties
            //printMsg("############################################################################");
            //testKnowledgeSpaceGeneration();

            Console.ReadKey();
        }

        static void demoAdaptationAndAssessment() {
            string adaptID = DifficultyAdapter.Type;
            string gameID = "TileZero";
            string playerID = "Noob";           // [SC] using this player as an example
            string scenarioID = "Hard AI";      // [SC] using this scenario as an example
            bool updateScenarioRatings = true;  // [SC] alwyas update scenario ratings
            DateTime lastPlayed = DateTime.ParseExact("2012-12-31T11:59:59", TwoA.DATE_FORMAT, null);

            TwoA twoA = new TwoA(new MyBridge());

            #region Examples of adding scenario data into TwoA

            // [SC] Scenario data is strored in 'TwoA.scenarios'. Its datatype is List<ScenarioNode>. It has a public access modifier.
            // [SC] Each ScenarioNode instance contains data for a single scenario.
            // [SC] TwoA also provides predefined methods for adding new scenarios.
            // [SC] Methods in TwoA (cases 1 - 4) ensure that all values are valid and ID combination is unique to the scenario.
            // [SC] Excercise care if you add new scenario by directly accessing the 'TwoA.scenarios' variable. Make sure a combination of adapID, gameID, scenarioID is unique.

            // [SC] Adding scenario data, Case 1
            twoA.AddScenario(
                new ScenarioNode (adaptID, gameID, "Very Easy AI") {
                    Rating = 1.2,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                    TimeLimit = 900000
                }
            );

            // [SC] Adding scenario data, Case 2
            twoA.AddScenario(new ScenarioNode(adaptID, gameID, "Easy AI", 1.4, 100, 0.0075, 0.01, lastPlayed, 900000));

            // [SC] Adding scenario data, Case 3
            twoA.AddScenario(adaptID, gameID, "Medium Color AI", 1.6, 100, 0.0075, 0.01, lastPlayed, 900000);

            // [SC] Adding scenario data, Case 4: scenario parameters will be assigned default values
            // [SC] Changing the default values
            twoA.AddScenario(adaptID, gameID, "Medium Shape AI");
            twoA.ScenarioRating(adaptID, gameID, "Medium Shape AI", 1.6);
            twoA.ScenarioPlayCount(adaptID, gameID, "Medium Shape AI", 100);
            twoA.ScenarioKFactor(adaptID, gameID, "Medium Shape AI", 0.0075);
            twoA.ScenarioUncertainty(adaptID, gameID, "Medium Shape AI", 0.01);
            twoA.ScenarioLastPlayed(adaptID, gameID, "Medium Shape AI", lastPlayed);
            twoA.ScenarioTimeLimit(adaptID, gameID, "Medium Shape AI", 900000);

            // [SC] Adding scenario data, Case 5: directly accessing the List structure
            twoA.scenarios.Add(
                new ScenarioNode (adaptID, gameID, scenarioID) { // [SC] Hard AI  
                    Rating = 6,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                    TimeLimit = 900000
                }
            );

            // [SC] Adding scenario data, Case 6
            twoA.scenarios.Add(new ScenarioNode(adaptID, gameID, "Very Hard AI", 10, 100, 0.0075, 0.01, lastPlayed, 900000));

            // [SC] Retrieveing a scenario node by scenario ID
            ScenarioNode scenarioNode = twoA.Scenario(adaptID, gameID, scenarioID);

            printMsg("\nExample scenario parameters: ");
            printMsg(String.Format("    ScenarioID: {0}.", scenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", scenarioNode.Rating));
            printMsg(String.Format("    Play count: {0}.", scenarioNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", scenarioNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", scenarioNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", scenarioNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));
            printMsg(String.Format("    Time limit: {0}.", scenarioNode.TimeLimit));

            #endregion Examples of adding scenario data into TwoA

            #region Examples of adding player data into TwoA

            // [SC] Player data is strored in 'TwoA.players'. Its datatype is List<PlayerNode>. It has a public access modifier.
            // [SC] Similar to scenarios, predefined methods 'AddPlayer' are provided by the TwoA class.

            // [SC] adding a new player node
            twoA.AddPlayer(
                new PlayerNode (adaptID, gameID, playerID) {
                    Rating = 5.5,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                }
            );

            // [SC] Retrieveing a player node by player ID
            PlayerNode playerNode = twoA.Player(adaptID, gameID, playerID);

            printMsg("\nExample player parameters: ");
            printMsg(String.Format("    Player ID: {0}.", playerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", playerNode.Rating));
            printMsg(String.Format("    Play count: {0}.", playerNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", playerNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", playerNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", playerNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));

            #endregion Examples of adding player data into TwoA

            #region Demo of methods for requesting a recommended scenario

            // [SC] Demo of different methods for requesting a recommended scenario
            // [SC] By default, the success rate P = 0.75, this means that TwoA will recommend a scenario where player's probability of completing the scenario is 75%. 
            // [SC] For more details on the success rate, refer to the "Methods for controlling success rate parameter." section in the API manual.
            // [SC] Asking for the recommendations for the player 'Noob'.
            // [SC] Among 10 requests, the most frequent recommendation should be the scenario 'Hard AI'.
            // [SC] 'Hard AI' scenario is recommended since it has a rating closest to the player's rating
            printMsg(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}; P = 0.75: ", playerID));
            printMsg("    " + twoA.TargetScenarioID(adaptID, gameID, playerID));                // Case 1: directly return scenario ID
            printMsg("    " + twoA.TargetScenarioID(playerNode));                               // Case 2: directly return scenario ID
            printMsg("    " + twoA.TargetScenario(adaptID, gameID, playerID).ScenarioID);       // Case 3: returns ScenarioNode
            printMsg("    " + twoA.TargetScenario(playerNode).ScenarioID);                      // Case 4: returns ScenarioNode
            printMsg("    " + twoA.TargetScenario(playerNode, twoA.scenarios).ScenarioID);      // Case 5: provide a custom list of scenarios from which to choose; returns ScenarioNode
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));

            // [SC] Changing the success rate to P = 0.1. Player has only 10% chance of succeeding.
            // [SC] For more details on the success rate, refer to the "Methods for controlling success rate parameter." section in the API manual.
            // [SC] TwoA should recommend Very Hard AI in some cases.
            twoA.SetTargetDistribution(adaptID, 0.1, 0.05, 0.01, 0.35);
            printMsg(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}; P = 0.5: ", playerID));
            for (int i = 0; i < 10; i++) {
                printMsg("    " + twoA.TargetScenarioID(playerNode));
            }

            #endregion Demo of methods for requesting a recommended scenario

            #region Demo for requesting a recommended difficulty rating

            // [SC] set target success rate to P = 0.75
            twoA.SetTargetDistribution(adaptID, 0.75, 0.1, 0.5, 1.0); // [SC] this is the same as twoA.SetDefaultTargetDistribution(adaptID)
            printMsg(String.Format("\nRecommended difficulty rating {0} for player rating {1} and success rate {2}."
                    , twoA.TargetDifficultyRating(playerNode), playerNode.Rating, twoA.GetTargetDistribution(adaptID)[0]));
            // [SC] set target success rate to P = 0.1
            twoA.SetTargetDistribution(adaptID, 0.1, 0.05, 0.01, 0.35);
            printMsg(String.Format("Recommended difficulty rating {0} for player rating {1} and success rate {2}."
                    , twoA.TargetDifficultyRating(playerNode), playerNode.Rating, twoA.GetTargetDistribution(adaptID)[0]));

            #endregion Demo for requesting a recommended difficulty rating

            #region Demo of methods for reassessing player and scenario ratings

            printMsg("\nFirst simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: ");
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 120000, 1, updateScenarioRatings, 0); // [SC] Case 1: passing player and scenario IDs
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg("\nSecond simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: ");
            twoA.UpdateRatings(playerNode, scenarioNode, 230000, 1, updateScenarioRatings, 0); // [SC] Case 2: passing PlayerNode and ScenarioNode instances
            printMsg(String.Format("    Player ID: {0}.", playerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", playerNode.Rating));
            printMsg(String.Format("    Play count: {0}.", playerNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", playerNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", playerNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", playerNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));
            printMsg("");
            printMsg(String.Format("    ScenarioID: {0}.", scenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", scenarioNode.Rating));
            printMsg(String.Format("    Play count: {0}.", scenarioNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", scenarioNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", scenarioNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", scenarioNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));
            printMsg(String.Format("    Time limit: {0}.", scenarioNode.TimeLimit));

            printMsg("\nThird simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: ");
            twoA.UpdateRatings(playerNode, scenarioNode, 12000, 0, updateScenarioRatings, 0);
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg("\nFourth simulated gameplay. Using custom K factor to scale rating changes. Player rating increases and scenario rating decreases: ");
            PlayerNode clonePlayerNode = playerNode.ShallowClone();
            ScenarioNode cloneScenarioNode = scenarioNode.ShallowClone();
            twoA.UpdateRatings(playerNode, scenarioNode, 12000, 1, updateScenarioRatings, 0); // [SC] as a contrast, use no custom K factor
            twoA.UpdateRatings(clonePlayerNode, cloneScenarioNode, 12000, 1, updateScenarioRatings, 1); // [SC] use K factor of 10 for both player and scenario
            printMsg(String.Format("    Player ID: {0}.", playerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", playerNode.Rating));
            printMsg(String.Format("    K factor: {0}.", playerNode.KFactor));
            printMsg("");
            printMsg(String.Format("    Player ID (custom K factor): {0}.", clonePlayerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", clonePlayerNode.Rating));
            printMsg(String.Format("    K factor: {0}.", clonePlayerNode.KFactor));
            printMsg("");
            printMsg(String.Format("    ScenarioID: {0}.", scenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", scenarioNode.Rating));
            printMsg(String.Format("    K factor: {0}.", scenarioNode.KFactor));
            printMsg("");
            printMsg(String.Format("    ScenarioID (custom K factor): {0}.", cloneScenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", cloneScenarioNode.Rating));
            printMsg(String.Format("    K factor: {0}.", cloneScenarioNode.KFactor));

            #endregion Demo of methods for reassessing player and scenario ratings

            #region example output
            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console/Debug output should resemble (not exactly the same since there is some randomness in the asset) the output below
            //
            //Example scenario parameters: 
            //    ScenarioID: Hard AI
            //    Rating: 6
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //    TimeLimit: 900000
            //
            //Example player parameters: 
            //    PlayerID: Noob
            //    Rating: 5.5
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //
            //Ask 10 times for a recommended scenarios for the player Noob; P = 0.75:  
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Medium Shape AI
            //    Hard AI
            //    Hard AI
            //
            //Ask 10 times for a recommended scenarios for the player Noob; P = 0.5: 
            //    Very Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Very Hard AI
            //    Very Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Hard AI
            //
            // Recommended difficulty rating 4.40138771133189 for player rating 5.5 and success rate 0.75.
            // Recommended difficulty rating 7.69722457733622 for player rating 5.5 and success rate 0.1.
            //
            //First simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.53437762105702
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.96562237894298
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            //Second simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.56336425733209
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.93663574266791
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            //Third simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: 
            //    PlayerID: Noob
            //    Rating: 5.5356982136711
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.9643017863289
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            //Fourth simulated gameplay. Using custom K factor to scale rating changes. Player rating increases and scenario rating decreases: 
            //    Player ID: Noob.
            //    Rating: 5.57109750442052.
            //    K factor: 0.0313875.
            //
            //    Player ID (custom K factor): Noob.
            //    Rating: 6.66351313201197.
            //    K factor: 1.
            //
            //    ScenarioID: Hard AI.
            //    Rating: 5.92890249557948.
            //    K factor: 0.0313875.
            //
            //    ScenarioID (custom K factor): Hard AI.
            //    Rating: 4.83648686798803.
            //    K factor: 1.
            #endregion example output
        }

        static void demoAdaptationAndAssessmentElo() {
            string adaptID = DifficultyAdapterElo.Type; // [SC] Make sure to change the adaptation ID
            string gameID = "TileZero";
            string playerID = "Noob";           // [SC] using this player as an example
            string scenarioID = "Hard AI";      // [SC] using this scenario as an example
            bool updateScenarioRatings = true;  // [SC] alwyas update scenario ratings
            DateTime lastPlayed = DateTime.ParseExact("2012-12-31T11:59:59", TwoA.DATE_FORMAT, null);

            TwoA twoA = new TwoA(new MyBridge());

            #region Examples of adding scenario data into TwoA

            // [SC] Scenario data is strored in 'TwoA.scenarios'. Its datatype is List<ScenarioNode>. It has a public access modifier.
            // [SC] Each ScenarioNode instance contains data for a single scenario.
            // [SC] TwoA also provides predefined methods for adding new scenarios.
            // [SC] Methods in TwoA (cases 1 - 4) ensure that all values are valid and ID combination is unique to the scenario.
            // [SC] Excercise care if you add new scenario by directly accessing the 'TwoA.scenarios' variable. Make sure a combination of adapID, gameID, scenarioID is unique.

            // [SC] Adding scenario data, Case 1
            twoA.AddScenario(
                new ScenarioNode (adaptID, gameID, "Very Easy AI") {
                    Rating = 1.2,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                    TimeLimit = 900000
                }
            );

            // [SC] Adding scenario data, Case 2
            twoA.AddScenario(new ScenarioNode(adaptID, gameID, "Easy AI", 1.4, 100, 0.0075, 0.01, lastPlayed, 900000));

            // [SC] Adding scenario data, Case 3
            twoA.AddScenario(adaptID, gameID, "Medium Color AI", 1.6, 100, 0.0075, 0.01, lastPlayed, 900000);

            // [SC] Adding scenario data, Case 4: scenario parameters will be assigned default values
            // [SC] Changing the default values
            twoA.AddScenario(adaptID, gameID, "Medium Shape AI");
            twoA.ScenarioRating(adaptID, gameID, "Medium Shape AI", 1.6);
            twoA.ScenarioPlayCount(adaptID, gameID, "Medium Shape AI", 100);
            twoA.ScenarioKFactor(adaptID, gameID, "Medium Shape AI", 0.0075);
            twoA.ScenarioUncertainty(adaptID, gameID, "Medium Shape AI", 0.01);
            twoA.ScenarioLastPlayed(adaptID, gameID, "Medium Shape AI", lastPlayed);
            twoA.ScenarioTimeLimit(adaptID, gameID, "Medium Shape AI", 900000);

            // [SC] Adding scenario data, Case 5: directly accessing the List structure
            twoA.scenarios.Add(
                new ScenarioNode (adaptID, gameID, scenarioID) { // [SC] Hard AI   
                    Rating = 6,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                    TimeLimit = 900000
                }
            );

            // [SC] Adding scenario data, Case 6
            twoA.scenarios.Add(new ScenarioNode(adaptID, gameID, "Very Hard AI", 10, 100, 0.0075, 0.01, lastPlayed, 900000));

            // [SC] Retrieveing a scenario node by scenario ID
            ScenarioNode scenarioNode = twoA.Scenario(adaptID, gameID, scenarioID);

            printMsg("\nExample scenario parameters: ");
            printMsg(String.Format("    ScenarioID: {0}.", scenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", scenarioNode.Rating));
            printMsg(String.Format("    Play count: {0}.", scenarioNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", scenarioNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", scenarioNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", scenarioNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));
            printMsg(String.Format("    Time limit: {0}.", scenarioNode.TimeLimit));

            #endregion Examples of adding scenario data into TwoA

            #region Examples of adding player data into TwoA

            // [SC] Player data is strored in 'TwoA.players'. Its datatype is List<PlayerNode>. It has a public access modifier.
            // [SC] Similar to scenarios, predefined methods 'AddPlayer' are provided by the TwoA class.

            // [SC] adding a new player node
            twoA.AddPlayer(
                new PlayerNode (adaptID, gameID, playerID) {
                    Rating = 5.5,
                    PlayCount = 100,
                    KFactor = 0.0075,
                    Uncertainty = 0.01,
                    LastPlayed = lastPlayed,
                }
            );

            // [SC] Retrieveing a player node by player ID
            PlayerNode playerNode = twoA.Player(adaptID, gameID, playerID);

            printMsg("\nExample player parameters: ");
            printMsg(String.Format("    Player ID: {0}.", playerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", playerNode.Rating));
            printMsg(String.Format("    Play count: {0}.", playerNode.PlayCount));
            printMsg(String.Format("    K factor: {0}.", playerNode.KFactor));
            printMsg(String.Format("    Uncertainty: {0}.", playerNode.Uncertainty));
            printMsg(String.Format("    Last played: {0}.", playerNode.LastPlayed.ToString(TwoA.DATE_FORMAT)));

            #endregion Examples of adding player data into TwoA

            #region Demo of methods for requesting a recommended scenario

            // [SC] Demo of different methods for requesting a recommended scenario
            // [SC] By default, the success rate P = 0.75, this means that TwoA will recommend a scenario where player's probability of completing the scenario is 75%. 
            // [SC] For more details on the success rate, refer to the "Methods for controlling success rate parameter." section in the API manual.
            // [SC] Asking for the recommendations for the player 'Noob'.
            // [SC] Among 10 requests, the most frequent recommendation should be the scenario 'Hard AI'.
            // [SC] 'Hard AI' scenario is recommended since it has a rating closest to the player's rating
            printMsg(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}; P = 0.75: ", playerID));
            printMsg("    " + twoA.TargetScenarioID(adaptID, gameID, playerID));                // Case 1: directly return scenario ID
            printMsg("    " + twoA.TargetScenarioID(playerNode));                               // Case 2: directly return scenario ID
            printMsg("    " + twoA.TargetScenario(adaptID, gameID, playerID).ScenarioID);       // Case 3: returns ScenarioNode
            printMsg("    " + twoA.TargetScenario(playerNode).ScenarioID);                      // Case 4: returns ScenarioNode
            printMsg("    " + twoA.TargetScenario(playerNode, twoA.scenarios).ScenarioID);      // Case 5: provide a custom list of scenarios from which to choose; returns ScenarioNode
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));
            printMsg("    " + twoA.TargetScenarioID(playerNode));

            // [SC] Changing the success rate to P = 0.1. Player has only 10% chance of succeeding.
            // [SC] For more details on the success rate, refer to the "Methods for controlling success rate parameter." section in the API manual.
            // [SC] TwoA should recommend Very Hard AI in some cases.
            twoA.SetTargetDistribution(adaptID, 0.1, 0.05, 0.01, 0.35);
            printMsg(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}; P = 0.5: ", playerID));
            for (int i = 0; i < 10; i++) {
                printMsg("    " + twoA.TargetScenarioID(playerNode));
            }

            #endregion Demo of methods for requesting a recommended scenario

            #region Demo for requesting a recommended difficulty rating

            // [SC] set target success rate to P = 0.75
            twoA.SetTargetDistribution(adaptID, 0.75, 0.1, 0.5, 1.0); // [SC] this is the same as twoA.SetDefaultTargetDistribution(adaptID)
            printMsg(String.Format("\nRecommended difficulty rating {0} for player rating {1} and success rate {2}."
                    , twoA.TargetDifficultyRating(playerNode), playerNode.Rating, twoA.GetTargetDistribution(adaptID)[0]));
            // [SC] set target success rate to P = 0.1
            twoA.SetTargetDistribution(adaptID, 0.1, 0.05, 0.01, 0.35);
            printMsg(String.Format("Recommended difficulty rating {0} for player rating {1} and success rate {2}."
                    , twoA.TargetDifficultyRating(playerNode), playerNode.Rating, twoA.GetTargetDistribution(adaptID)[0]));
            twoA.TargetDifficultyRating(playerNode);

            #endregion Demo for requesting a recommended difficulty rating

            #region Demo of methods for reassessing player and scenario ratings

            printMsg(String.Format("\n1st simulated gameplay. Player's accuracy is 1.0. Expected accuracy is {0}. Player rating increases and scenario rating decreases: "
                , twoA.CalculateExpectedScore(adaptID, playerNode.Rating, scenarioNode.Rating, 0)));
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 0, 1.0, updateScenarioRatings, 0); // [SC] any value of rt is automatically ignored
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg(String.Format("\n2nd simulated gameplay. Player's accuracy is 0.75. Expected accuracy is {0}. Player rating increases and scenario rating decreases: "
                , twoA.CalculateExpectedScore(adaptID, playerNode.Rating, scenarioNode.Rating, 0)));
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 0, 0.75, updateScenarioRatings, 0); // [SC] any value of rt is automatically ignored
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg(String.Format("\n3rd simulated gameplay. Player's accuracy is 0.5. Expected accuracy is {0}. Player rating increases slightly and scenario rating decreases slightly: "
                , twoA.CalculateExpectedScore(adaptID, playerNode.Rating, scenarioNode.Rating, 0)));
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 0, 0.5, updateScenarioRatings, 0); // [SC] any value of rt is automatically ignored
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg(String.Format("\n4th simulated gameplay. Player's accuracy is 0.25. Expected accuracy is {0}. Player rating decreass and scenario rating increases: "
                , twoA.CalculateExpectedScore(adaptID, playerNode.Rating, scenarioNode.Rating, 0)));
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 0, 0.25, updateScenarioRatings, 0); // [SC] any value of rt is automatically ignored
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg(String.Format("\n5th simulated gameplay. Player's accuracy is 0.0. Expected accuracy is {0}. Player rating decreass and scenario rating increases: "
               , twoA.CalculateExpectedScore(adaptID, playerNode.Rating, scenarioNode.Rating, 0)));
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 0, 0.0, updateScenarioRatings, 0); // [SC] any value of rt is automatically ignored
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg("\n6th simulated gameplay. Using custom K factor to scale rating changes. Player rating increases and scenario rating decreases: ");
            PlayerNode clonePlayerNode = playerNode.ShallowClone();
            ScenarioNode cloneScenarioNode = scenarioNode.ShallowClone();
            twoA.UpdateRatings(playerNode, scenarioNode, 0, 0.75, updateScenarioRatings, 0); // [SC] as a contrast, use no custom K factor
            twoA.UpdateRatings(clonePlayerNode, cloneScenarioNode, 0, 0.75, updateScenarioRatings, 1); // [SC] use K factor of 10 for both player and scenario
            printMsg(String.Format("    Player ID: {0}.", playerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", playerNode.Rating));
            printMsg(String.Format("    K factor: {0}.", playerNode.KFactor));
            printMsg("");
            printMsg(String.Format("    Player ID (custom K factor): {0}.", clonePlayerNode.PlayerID));
            printMsg(String.Format("    Rating: {0}.", clonePlayerNode.Rating));
            printMsg(String.Format("    K factor: {0}.", clonePlayerNode.KFactor));
            printMsg("");
            printMsg(String.Format("    ScenarioID: {0}.", scenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", scenarioNode.Rating));
            printMsg(String.Format("    K factor: {0}.", scenarioNode.KFactor));
            printMsg("");
            printMsg(String.Format("    ScenarioID (custom K factor): {0}.", cloneScenarioNode.ScenarioID));
            printMsg(String.Format("    Rating: {0}.", cloneScenarioNode.Rating));
            printMsg(String.Format("    K factor: {0}.", cloneScenarioNode.KFactor));

            #endregion Demo of methods for reassessing player and scenario ratings

            #region example output
            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console/Debug output should resemble (not exactly the same since there is some randomness in the asset) the output below
            //
            //Example scenario parameters: 
            //    ScenarioID: Hard AI
            //    Rating: 6
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //    TimeLimit: 900000
            //
            ////Example player parameters: 
            //    PlayerID: Noob
            //    Rating: 5.5
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //
            //Ask 10 times for a recommended scenarios for the player Noob; P = 0.75:  
            //    Medium Color AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Very Easy AI
            //    Easy AI
            //    Hard AI
            //    Hard AI
            //    Medium Color AI
            //
            //Ask 10 times for a recommended scenarios for the player Noob; P = 0.5: 
            //    Very Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Very Hard AI
            //    Very Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Very Hard AI
            //    Hard AI
            //    Hard AI
            //
            // Recommended difficulty rating 4.40138771133189 for player rating 5.5 and success rate 0.75.
            // Recommended difficulty rating 7.69722457733622 for player rating 5.5 and success rate 0.1.
            //
            //1st simulated gameplay. Player's accuracy is 1.0. Expected accuracy is 0.377540051684686. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.52076292965099
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2017-11-06T09:17:40
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.97923707034901
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2017-11-06T09:17:40
            //    TimeLimit: 900000
            //
            //2nd simulated gameplay. Player's accuracy is 0.75. Expected accuracy is 0.387347291047703. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.53262167323373
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2017-11-06T09:17:40
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.96737832676627
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2017-11-06T09:17:40
            //    TimeLimit: 900000
            //
            //3rd simulated gameplay. Player's accuracy is 0.5. Expected accuracy is 0.39299051581039. Player rating increases slightly and scenario rating decreases slightly: 
            //    PlayerID: Noob
            //    Rating: 5.53605065839273
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2017-11-06T09:17:40
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.96394934160727
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2017-11-06T09:17:40
            //    TimeLimit: 900000
            //
            //4th simulated gameplay. Player's accuracy is 0.25. Expected accuracy is 0.394627681212804. Player rating decreass and scenario rating increases: 
            //    PlayerID: Noob
            //    Rating: 5.53151115704867
            //    PlayCount: 104
            //    KFactor: 0.0313875
            //    Uncertainty: 0.91
            //    LastPlayed: 2017-11-06T09:17:40
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.96848884295133
            //    PlayCount: 104
            //    KFactor: 0.0313875
            //    Uncertainty: 0.91
            //    LastPlayed: 2017-11-06T09:17:40
            //    TimeLimit: 900000
            //
            //5th simulated gameplay. Player's accuracy is 0.0. Expected accuracy is 0.392460814156309. Player rating decreass and scenario rating increases: 
            //    PlayerID: Noob
            //    Rating: 5.51945034565363
            //    PlayCount: 105
            //    KFactor: 0.03073125
            //    Uncertainty: 0.885
            //    LastPlayed: 2017-11-06T09:17:40
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.98054965434637
            //    PlayCount: 105
            //    KFactor: 0.03073125
            //    Uncertainty: 0.885
            //    LastPlayed: 2017-11-06T09:17:40
            //    TimeLimit: 900000
            //
            //6th simulated gameplay. Using custom K factor to scale rating changes. Player rating increases and scenario rating decreases: 
            //    Player ID: Noob.
            //    Rating: 5.53037585645568.
            //    K factor: 0.030075.
            //
            //    Player ID (custom K factor): Noob.
            //    Rating: 5.88272585029387.
            //    K factor: 1.
            //
            //    ScenarioID: Hard AI.
            //    Rating: 5.96962414354432.
            //    K factor: 0.030075.
            //
            //    ScenarioID (custom K factor): Hard AI.
            //    Rating: 5.61727414970613.
            //    K factor: 1.
            //

            #endregion example output
        }

        static void testKnowledgeSpaceGeneration() {
            // [SC] creating an instance of knowledge structure generator
            KSGenerator ksg = new KSGenerator(null, 0.4);

            // [SC] creating a list of rated categories that will be used to generate a knowledge structure
            // [SC] a category represents a set of problems/scenarios that have same difficulty and structure
            List<PCategory> categories = new List<PCategory>();
            categories.Add(new PCategory("a", 3.391156));
            categories.Add(new PCategory("b", 24.182423));
            categories.Add(new PCategory("c", 24.313351));
            categories.Add(new PCategory("d", 32.193103));
            categories.Add(new PCategory("e", 35.618040));
            categories.Add(new PCategory("f", 37.046992));
            categories.Add(new PCategory("g", 45.166948));

            // [SC] first step is to create a rank order from the list of categories
            RankOrder ro = ksg.createRankOrder(categories);

            // [SC] second step is to create a knowledge structure using previsouly created rank order
            KStructure ks = ksg.createKStructure(ro);

            // [SC] third optional step is to create an expanded knowledge structure by identifying additional knowledge states
            KStructure ksExpand = ksg.createKStructure(ro);
            ksg.createExpandedKStructure(ksExpand);

            //////////////////////////////////////////////////////////////////////////////////////////////
            // [SC] note that KStructure object can be serialized into an XML format

            // [SC] first, create XML factory singleton
            XMLFactory xmlFct = XMLFactory.Instance;
            // [SC] next, creating XML document from KStructue object
            XDocument xmlDoc = xmlFct.createXml(ks);
            // [SC] XML document can be further serialized into a string (e.g., to store in a file)
            string xmlTxt = xmlFct.serialize(xmlDoc);
            // [SC] finally, XML document can be deserialized into a KStructure object
            KStructure deserKS = xmlFct.createKStructure(xmlDoc);

            //////////////////////////////////////////////////////////////////////////////////////////////
            // [SC] to visualize results, printing RankOrder, KStructure and XML objects into console diagnostic window

            // [SC] printing the rank order
            printMsg("======================================");
            printMsg("Traversing ranks in rank order:\n");
            for (int rankCounter = 0; rankCounter < ro.getRankCount(); rankCounter++) {
                Rank rank = ro.getRankAt(rankCounter);

                printMsg("Current rank: " + rank.RankIndex);

                for (int catCounter = 0; catCounter < rank.getCategoryCount(); catCounter++) {
                    PCategory category = rank.getCategoryAt(catCounter);

                    printMsg("   Category: " + category.Id + "; Rating: " + category.Rating);
                }
            }

            // [SC] printing the knowledge structure
            // [SC] note that all knowledge states in unexpanded knowledge structure have the type of 'core'
            printMsg("\n======================================");
            printMsg("Traversing knowledge states in the unexpanded knowledge structure:\n");
            foreach (KSRank rank in ks.getRanks()) {
                printMsg("Rank " + rank.RankIndex);
                foreach (KState state in rank.getStates()) {
                    printMsg("    Current state: " + state.ToString() + "; State type: " + state.StateType + "; State ID: " + state.Id);
                    foreach (KState prevState in state.getPrevStates()) {
                        printMsg("        Prev state: " + prevState.ToString());
                    }
                    foreach (KState nextState in state.getNextStates()) {
                        printMsg("        Next state: " + nextState.ToString());
                    }
                }
            }

            // [SC] printing the expanded knowledge structure
            // [SC] note that all new states added to the expanded structure have the type of 'expanded'
            printMsg("\n======================================");
            printMsg("Traversing knowledge states in the expanded knowledge structure:\n");
            foreach (KSRank rank in ksExpand.getRanks()) {
                printMsg("Rank " + rank.RankIndex);
                foreach (KState state in rank.getStates()) {
                    printMsg("    Current state: " + state.ToString() + "; State type: " + state.StateType + "; State ID: " + state.Id);
                    foreach (KState prevState in state.getPrevStates()) {
                        printMsg("        Prev state: " + prevState.ToString());
                    }
                    foreach (KState nextState in state.getNextStates()) {
                        printMsg("        Next state: " + nextState.ToString());
                    }
                }
            }

            // [SC] printing the xml document
            // [SC] the XML document consists of three main elements:
            //  - PCategories: this element contains a list of rated categories
            //  - RankOrder: this element contains deserialization of the RankOrder object that was used to create the knowledge structure
            //  - KStructure: this element contains the deserialization of the knowledge structure
            printMsg("\n======================================");
            printMsg("The XML document:\n");
            printMsg(xmlTxt);

            #region example output
            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console/Debug output should resemble (not exactly the same since there is some randomness in the asset) the output below
            //
            //======================================
            //Traversing ranks in rank order:
            //
            //Current rank: 1
            //   Category: a; Rating: 3.391156
            //Current rank: 2
            //   Category: b; Rating: 24.182423
            //   Category: c; Rating: 24.313351
            //Current rank: 3
            //   Category: d; Rating: 32.193103
            //Current rank: 4
            //   Category: e; Rating: 35.61804
            //   Category: f; Rating: 37.046992
            //Current rank: 5
            //   Category: g; Rating: 45.166948
            //
            //======================================
            //Traversing knowledge states in the unexpanded knowledge structure:
            //
            //Rank 0
            //    Current state: (); State type: root; State ID: S0.1
            //        Next state: (a)
            //Rank 1
            //    Current state: (a); State type: core; State ID: S1.1
            //        Prev state: ()
            //        Next state: (a,b)
            //        Next state: (a,c)
            //Rank 2
            //    Current state: (a,b); State type: core; State ID: S2.1
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //    Current state: (a,c); State type: core; State ID: S2.2
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //Rank 3
            //    Current state: (a,b,c); State type: core; State ID: S3.1
            //        Prev state: (a,b)
            //        Prev state: (a,c)
            //        Next state: (a,b,c,d)
            //Rank 4
            //    Current state: (a,b,c,d); State type: core; State ID: S4.1
            //        Prev state: (a,b,c)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,b,c,d,f)
            //Rank 5
            //    Current state: (a,b,c,d,e); State type: core; State ID: S5.1
            //        Prev state: (a,b,c,d)
            //        Next state: (a,b,c,d,e,f)
            //    Current state: (a,b,c,d,f); State type: core; State ID: S5.2
            //        Prev state: (a,b,c,d)
            //        Next state: (a,b,c,d,e,f)
            //Rank 6
            //    Current state: (a,b,c,d,e,f); State type: core; State ID: S6.1
            //        Prev state: (a,b,c,d,e)
            //        Prev state: (a,b,c,d,f)
            //        Next state: (a,b,c,d,e,f,g)
            //Rank 7
            //    Current state: (a,b,c,d,e,f,g); State type: core; State ID: S7.1
            //        Prev state: (a,b,c,d,e,f)
            //
            //======================================
            //Traversing knowledge states in the expanded knowledge structure:
            //
            //Rank 0
            //    Current state: (); State type: root; State ID: S0.1
            //        Next state: (a)
            //Rank 1
            //    Current state: (a); State type: core; State ID: S1.1
            //        Prev state: ()
            //        Next state: (a,b)
            //        Next state: (a,c)
            //Rank 2
            //    Current state: (a,b); State type: core; State ID: S2.1
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //        Next state: (a,b,d)
            //    Current state: (a,c); State type: core; State ID: S2.2
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //        Next state: (a,c,d)
            //Rank 3
            //    Current state: (a,b,c); State type: core; State ID: S3.1
            //        Prev state: (a,b)
            //        Prev state: (a,c)
            //        Next state: (a,b,c,d)
            //    Current state: (a,b,d); State type: expanded; State ID: S3.2
            //        Prev state: (a,b)
            //        Next state: (a,b,c,d)
            //        Next state: (a,b,d,e)
            //        Next state: (a,b,d,f)
            //    Current state: (a,c,d); State type: expanded; State ID: S3.3
            //        Prev state: (a,c)
            //        Next state: (a,b,c,d)
            //        Next state: (a,c,d,e)
            //        Next state: (a,c,d,f)
            //Rank 4
            //    Current state: (a,b,c,d); State type: core; State ID: S4.1
            //        Prev state: (a,b,c)
            //        Prev state: (a,b,d)
            //        Prev state: (a,c,d)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,b,c,d,f)
            //    Current state: (a,b,d,e); State type: expanded; State ID: S4.2
            //        Prev state: (a,b,d)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,b,d,e,f)
            //        Next state: (a,b,d,e,g)
            //    Current state: (a,c,d,e); State type: expanded; State ID: S4.3
            //        Prev state: (a,c,d)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,c,d,e,f)
            //        Next state: (a,c,d,e,g)
            //    Current state: (a,b,d,f); State type: expanded; State ID: S4.4
            //        Prev state: (a,b,d)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,b,d,e,f)
            //        Next state: (a,b,d,f,g)
            //    Current state: (a,c,d,f); State type: expanded; State ID: S4.5
            //        Prev state: (a,c,d)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,c,d,e,f)
            //        Next state: (a,c,d,f,g)
            //Rank 5
            //    Current state: (a,b,c,d,e); State type: core; State ID: S5.1
            //        Prev state: (a,b,c,d)
            //        Prev state: (a,b,d,e)
            //        Prev state: (a,c,d,e)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,c,d,e,g)
            //    Current state: (a,b,c,d,f); State type: core; State ID: S5.2
            //        Prev state: (a,b,c,d)
            //        Prev state: (a,b,d,f)
            //        Prev state: (a,c,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,c,d,f,g)
            //    Current state: (a,b,d,e,f); State type: expanded; State ID: S5.3
            //        Prev state: (a,b,d,e)
            //        Prev state: (a,b,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,d,e,f,g)
            //    Current state: (a,c,d,e,f); State type: expanded; State ID: S5.4
            //        Prev state: (a,c,d,e)
            //        Prev state: (a,c,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,c,d,e,f,g)
            //    Current state: (a,b,d,e,g); State type: expanded; State ID: S5.5
            //        Prev state: (a,b,d,e)
            //        Next state: (a,b,c,d,e,g)
            //        Next state: (a,b,d,e,f,g)
            //    Current state: (a,c,d,e,g); State type: expanded; State ID: S5.6
            //        Prev state: (a,c,d,e)
            //        Next state: (a,b,c,d,e,g)
            //        Next state: (a,c,d,e,f,g)
            //    Current state: (a,b,d,f,g); State type: expanded; State ID: S5.7
            //        Prev state: (a,b,d,f)
            //        Next state: (a,b,c,d,f,g)
            //        Next state: (a,b,d,e,f,g)
            //    Current state: (a,c,d,f,g); State type: expanded; State ID: S5.8
            //        Prev state: (a,c,d,f)
            //        Next state: (a,b,c,d,f,g)
            //        Next state: (a,c,d,e,f,g)
            //Rank 6
            //    Current state: (a,b,c,d,e,f); State type: core; State ID: S6.1
            //        Prev state: (a,b,c,d,e)
            //        Prev state: (a,b,c,d,f)
            //        Prev state: (a,b,d,e,f)
            //        Prev state: (a,c,d,e,f)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,c,d,e,g); State type: expanded; State ID: S6.2
            //        Prev state: (a,b,c,d,e)
            //        Prev state: (a,b,d,e,g)
            //        Prev state: (a,c,d,e,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,c,d,f,g); State type: expanded; State ID: S6.3
            //        Prev state: (a,b,c,d,f)
            //        Prev state: (a,b,d,f,g)
            //        Prev state: (a,c,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,d,e,f,g); State type: expanded; State ID: S6.4
            //        Prev state: (a,b,d,e,f)
            //        Prev state: (a,b,d,e,g)
            //        Prev state: (a,b,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,c,d,e,f,g); State type: expanded; State ID: S6.5
            //        Prev state: (a,c,d,e,f)
            //        Prev state: (a,c,d,e,g)
            //        Prev state: (a,c,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //Rank 7
            //    Current state: (a,b,c,d,e,f,g); State type: core; State ID: S7.1
            //        Prev state: (a,b,c,d,e,f)
            //        Prev state: (a,b,c,d,e,g)
            //        Prev state: (a,b,c,d,f,g)
            //        Prev state: (a,b,d,e,f,g)
            //        Prev state: (a,c,d,e,f,g)
            //
            //======================================
            //The XML document:
            //
            //<?xml version="1.0" encoding="utf-16" standalone="yes"?>
            //<TwoA xmlns="https://github.com/rageappliedgame/HatAsset" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
            //  <PCategories>
            //    <PCategory xsd:id="a">
            //      <Rating>3.391156</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="b">
            //      <Rating>24.182423</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="c">
            //      <Rating>24.313351</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="d">
            //      <Rating>32.193103</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="e">
            //      <Rating>35.61804</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="f">
            //      <Rating>37.046992</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="g">
            //      <Rating>45.166948</Rating>
            //    </PCategory>
            //  </PCategories>
            //  <RankOrder>
            //    <Params>
            //      <Threshold>0.4</Threshold>
            //    </Params>
            //    <Ranks>
            //      <Rank Index="1">
            //        <PCategory xsd:idref="a" />
            //      </Rank>
            //      <Rank Index="2">
            //        <PCategory xsd:idref="b" />
            //        <PCategory xsd:idref="c" />
            //      </Rank>
            //      <Rank Index="3">
            //        <PCategory xsd:idref="d" />
            //      </Rank>
            //      <Rank Index="4">
            //        <PCategory xsd:idref="e" />
            //        <PCategory xsd:idref="f" />
            //      </Rank>
            //      <Rank Index="5">
            //        <PCategory xsd:idref="g" />
            //      </Rank>
            //    </Ranks>
            //  </RankOrder>
            //  <KStructure>
            //    <KSRank Index="0">
            //      <KState xsd:id="S0.1" Type="root">
            //        <PCategories />
            //        <PreviousStates />
            //        <NextStates>
            //          <KState xsd:idref="S1.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="1">
            //      <KState xsd:id="S1.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S0.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S2.1" />
            //          <KState xsd:idref="S2.2" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="2">
            //      <KState xsd:id="S2.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S1.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S3.1" />
            //        </NextStates>
            //      </KState>
            //      <KState xsd:id="S2.2" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="c" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S1.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S3.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="3">
            //      <KState xsd:id="S3.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S2.1" />
            //          <KState xsd:idref="S2.2" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S4.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="4">
            //      <KState xsd:id="S4.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S3.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S5.1" />
            //          <KState xsd:idref="S5.2" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="5">
            //      <KState xsd:id="S5.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S4.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S6.1" />
            //        </NextStates>
            //      </KState>
            //      <KState xsd:id="S5.2" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="f" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S4.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S6.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="6">
            //      <KState xsd:id="S6.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //          <PCategory xsd:idref="f" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S5.1" />
            //          <KState xsd:idref="S5.2" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S7.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="7">
            //      <KState xsd:id="S7.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //          <PCategory xsd:idref="f" />
            //          <PCategory xsd:idref="g" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S6.1" />
            //        </PreviousStates>
            //        <NextStates />
            //      </KState>
            //    </KSRank>
            //  </KStructure>
            //</TwoA>
            //
            #endregion example output
        }

        static void testScoreCalculations() {
            TwoA twoA = new TwoA(new MyBridge());

            /* SCORE MATRIX
             *              ----------------------------------------------
             *              | Low response  | High response | Time limit |
             *              | time          | time          | reached    |
             * -------------|---------------|---------------|------------|
             * Response = 1 | High positive | Low positive  |     0      |
             *              | score         | score         |            |
             * -------------|---------------|---------------|------------|
             * Response = 0 | High negative | Low negative  |     0      |
             *              | score         | score         |            |
             * ----------------------------------------------------------*/

            double maxItemDuration = 600000; // [SC] a player has max 10 min to solve a problem

            double responseTime = 30000; // [SC] assume the player spend only 30 seconds on the problem
            double correctAnswer = 1; // [SC] assume the player provided a correct response
            printMsg(String.Format("{3}RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)
                                    , Environment.NewLine));

            responseTime = 540000; // [SC] assume the player spend 9 minutes to provide a correct response
            printMsg(String.Format("RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)));

            correctAnswer = 0; // [SC] assume the player spent 9 minutes to provided an incorrect response
            printMsg(String.Format("RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)));

            responseTime = 30000; // [SC] assume the player spend only 30 seconds and provided incorrect response
            printMsg(String.Format("RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)));


            responseTime = 600000; // [SC] assume time limit was reached
            correctAnswer = 0; // [SC] answer does not matter
            printMsg(String.Format("{3}RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)
                                    , Environment.NewLine));
            correctAnswer = 1; // [SC] answer does not matter
            printMsg(String.Format("RT = {0}; Response = {1}; Score = {2}.", responseTime, correctAnswer
                                    , twoA.CalculateScore(correctAnswer, responseTime, maxItemDuration)));


            ////////////////////////////////////////////////////////////////////
            // [SC] expected output at Console and Debug window
            //
            // RT = 30000;  Response = 1; Score = 0.95.
            // RT = 540000; Response = 1; Score = 0.1.
            // RT = 540000; Response = 0; Score = -0.1.
            // RT = 30000;  Response = 0; Score = -0.95.
            //
            // RT = 600000; Response = 0; Score = 0.
            // RT = 600000; Response = 1; Score = 0.
        }

        // [SC] just a helper method
        static void printPlayerData(TwoA twoA, string adaptID, string gameID, string playerID) {
            printMsg("    PlayerID: " + playerID);
            printMsg("    Rating: " + twoA.PlayerRating(adaptID, gameID, playerID));
            printMsg("    PlayCount: " + twoA.PlayerPlayCount(adaptID, gameID, playerID));
            printMsg("    KFactor: " + twoA.PlayerKFactor(adaptID, gameID, playerID));
            printMsg("    Uncertainty: " + twoA.PlayerUncertainty(adaptID, gameID, playerID));
            printMsg("    LastPlayed: " + twoA.PlayerLastPlayed(adaptID, gameID, playerID).ToString(TwoA.DATE_FORMAT));
        }

        // [SC] just a helper method
        static void printScenarioData(TwoA twoA, string adaptID, string gameID, string scenarioID) {
            printMsg("    ScenarioID: " + scenarioID);
            printMsg("    Rating: " + twoA.ScenarioRating(adaptID, gameID, scenarioID));
            printMsg("    PlayCount: " + twoA.ScenarioPlayCount(adaptID, gameID, scenarioID));
            printMsg("    KFactor: " + twoA.ScenarioKFactor(adaptID, gameID, scenarioID));
            printMsg("    Uncertainty: " + twoA.ScenarioUncertainty(adaptID, gameID, scenarioID));
            printMsg("    LastPlayed: " + twoA.ScenarioLastPlayed(adaptID, gameID, scenarioID).ToString(TwoA.DATE_FORMAT));
            printMsg("    TimeLimit: " + twoA.ScenarioTimeLimit(adaptID, gameID, scenarioID));
        }

        // [SC] prints a message to both console and debug output window
        static void printMsg(string msg) {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }

    // [SC] implement the bridge
    class MyBridge : IBridge, IDataStorage, ILog
    {
        public MyBridge() { }

        public bool Exists(string fileId) {
            throw new NotImplementedException();
        }

        public void Save(string fileId, string fileData) {
            throw new NotImplementedException();
        }

        public string Load(string fileId) {
            throw new NotImplementedException();
        }

        public String[] Files() {
            throw new NotImplementedException();
        }

        public bool Delete(string fileId) {
            throw new NotImplementedException();
        }

        public void Log(Severity severity, string msg) {
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        } 
    }
}
