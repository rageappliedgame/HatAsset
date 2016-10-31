#region Header

/*
Copyright 2015 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

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

#endregion Header

namespace TestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using HAT;
    using AssetPackage;
    using Swiss;

    class Program
    {
        static void Main(string[] args) {
            HATAsset hat = new HATAsset(new MyBridge());

            string adaptID = "Game difficulty - Player skill";
            string gameID = "TileZero";
            string playerID = "Noob"; // [SC] using this player as an example
            string scenarioID = "Hard AI"; // [SC] using this scenario as an example

            // [SC] set this variable to true if you want changes saved to "HATAssetAppSettings.xml" and "gameplaylogs.xml" files
            bool updateDatafiles = false;

            ////////////////////////////////////////////////////////////////

            Console.WriteLine("\nExample player parameters: ");
            printPlayerData(hat, adaptID, gameID, playerID);

            ////////////////////////////////////////////////////////////////

            Console.WriteLine("\nExample scenario parameters: ");
            printScenarioData(hat, adaptID, gameID, scenarioID);

            ////////////////////////////////////////////////////////////////

            // [SC] Asking for the recommendations for the player 'Noob'.
            // [SC] Among 10 requests, the most frequent recommendation should be the scenario 'Hard AI'.
            // [SC] 'Hard AI' scenario is recommended since it has a rating closest to the player's rating
            Console.WriteLine(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}: ", playerID));
            for (int count = 0; count < 10; count++) {
                Console.WriteLine("    " + hat.TargetScenarioID(adaptID, gameID, playerID));
            }

            ////////////////////////////////////////////////////////////////

            Console.WriteLine("\nFirst simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: ");
            hat.UpdateRatings(adaptID, gameID, playerID, scenarioID, 120000, 1, updateDatafiles);
            printPlayerData(hat, adaptID, gameID, playerID);
            Console.WriteLine("\n");
            printScenarioData(hat, adaptID, gameID, scenarioID);

            Console.WriteLine("\nSecond simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: ");
            hat.UpdateRatings(adaptID, gameID, playerID, scenarioID, 230000, 1, updateDatafiles);
            printPlayerData(hat, adaptID, gameID, playerID);
            Console.WriteLine("\n");
            printScenarioData(hat, adaptID, gameID, scenarioID);

            Console.WriteLine("\nThird simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: ");
            hat.UpdateRatings(adaptID, gameID, playerID, scenarioID, 12000, 0, updateDatafiles);
            printPlayerData(hat, adaptID, gameID, playerID);
            Console.WriteLine("\n");
            printScenarioData(hat, adaptID, gameID, scenarioID);

            Console.ReadKey();

            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console output should resemble (not exactly the same since there is some randomness in the asset) the output below
            /*
            Example player parameters: 
                PlayerID: Noob
                Rating: 5.5
                PlayCount: 100
                KFactor: 0.0075
                Uncertainty: 0.01
                LastPlayed: 2016-03-14T12:59:59

            Example scenario parameters: 
                ScenarioID: Hard AI
                Rating: 6
                PlayCount: 100
                KFactor: 0.0075
                Uncertainty: 0.01
                LastPlayed: 2016-03-14T12:59:59
                TimeLimit: 900000

            Ask 10 times for a recommended scenarios for the player Noob: 
                Hard AI
                Hard AI
                Hard AI
                Hard AI
                Hard AI
                Hard AI
                Hard AI
                Very Hard AI
                Hard AI
                Hard AI

            First simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: 
                PlayerID: Noob
                Rating: 5.51002922165744
                PlayCount: 101
                KFactor: 0.00973125
                Uncertainty: 0.085
                LastPlayed: 2016-03-17T01:16:25

                ScenarioID: Hard AI
                Rating: 5.98997077834256
                PlayCount: 101
                KFactor: 0.00973125
                Uncertainty: 0.085
                LastPlayed: 2016-03-17T01:16:25
                TimeLimit: 900000

            Second simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: 
                PlayerID: Noob
                Rating: 5.51821506170315
                PlayCount: 102
                KFactor: 0.009075
                Uncertainty: 0.06
                LastPlayed: 2016-03-17T01:16:25

                ScenarioID: Hard AI
                Rating: 5.98178493829685
                PlayCount: 102
                KFactor: 0.009075
                Uncertainty: 0.06
                LastPlayed: 2016-03-17T01:16:25
                TimeLimit: 900000

            Third simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: 
                PlayerID: Noob
                Rating: 5.51119119088031
                PlayCount: 103
                KFactor: 0.00841875
                Uncertainty: 0.035
                LastPlayed: 2016-03-17T01:16:25

                ScenarioID: Hard AI
                Rating: 5.98880880911969
                PlayCount: 103
                KFactor: 0.00841875
                Uncertainty: 0.035
                LastPlayed: 2016-03-17T01:16:25
                TimeLimit: 900000
            */
        }

        // [SC] just a helper method
        static void printPlayerData(HATAsset hat, string adaptID, string gameID, string playerID) {
            Console.WriteLine("    PlayerID: " + hat.PlayerParam<string>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.PlayerID)));
            Console.WriteLine("    Rating: " + hat.PlayerParam<double>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.Rating)));
            Console.WriteLine("    PlayCount: " + hat.PlayerParam<double>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.PlayCount)));
            Console.WriteLine("    KFactor: " + hat.PlayerParam<double>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.KFactor)));
            Console.WriteLine("    Uncertainty: " + hat.PlayerParam<double>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.Uncertainty)));
            Console.WriteLine("    LastPlayed: " + hat.PlayerParam<string>(adaptID, gameID, playerID, ObjectUtils.GetMemberName<PlayerNode>(p => p.LastPlayed)));
        }

        // [SC] just a helper method
        static void printScenarioData(HATAsset hat, string adaptID, string gameID, string scenarioID) {
            Console.WriteLine("    ScenarioID: " + hat.ScenarioParam<string>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.ScenarioID)));
            Console.WriteLine("    Rating: " + hat.ScenarioParam<double>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.Rating)));
            Console.WriteLine("    PlayCount: " + hat.ScenarioParam<double>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.PlayCount)));
            Console.WriteLine("    KFactor: " + hat.ScenarioParam<double>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.KFactor)));
            Console.WriteLine("    Uncertainty: " + hat.ScenarioParam<double>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.Uncertainty)));
            Console.WriteLine("    LastPlayed: " + hat.ScenarioParam<string>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.LastPlayed)));
            Console.WriteLine("    TimeLimit: " + hat.ScenarioParam<double>(adaptID, gameID, scenarioID, ObjectUtils.GetMemberName<ScenarioNode>(p => p.TimeLimit)));
        }
    }

    class MyBridge : IBridge, IDataStorage
    {
        // [SC] path to "HATAssetAppSettings.xml" and "gameplaylogs.xml" files
        // [SC] these XML files are for running this test only and contain dummy data
        // [SC] to use the HAT asset with your game, use XML files at 'HatAsset\HatAsset\Resources\'
        const string DatafilePath = @".\Resources";

        public MyBridge() {}

        public bool Exists(string fileId) {
            return File.Exists(Path.Combine(DatafilePath, fileId));
        }

        public void Save(string fileId, string fileData) {
            File.WriteAllText(Path.Combine(DatafilePath, fileId), fileData);
        }

        public string Load(string fileId) {
            return File.ReadAllText(Path.Combine(DatafilePath, fileId));
        }

        public String[] Files() {
            return null;
        }

        public bool Delete(string fileId) {
            return false;
        }
    }
}
