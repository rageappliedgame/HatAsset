#region Header

/*
Copyright 2015 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/), Wim van der Vegt

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Namespace: TwoA
Filename: TwoA.cs
*/


// Change history:
// [2016.03.14]
//      - [SC] removed HATMode field
//      - [SC] removed TurnScenarioID method
//      - [SC] removed CurrentAccuracy method
//      - [SC] adapter.CalculateTargetScenarioID(adaptID, gameID, playerID) => adapter.TargetScenarioID(adaptID, gameID, playerID) in TargetScenarioID method
//      - [SC] removed HATAssetSettings settings field
//      - [SC] removed ISettings Settings property
//      - [SC] changed InitSettings method
//      - [SC] LoadGamePlayData => LoadGameplayData
//      - [SC] changed content of LoadGameplayData
//      - [SC] added LoadAdaptationData method
//      - [SC] renamed logfile field to gameplayLogsFile 
//      - [SC] renamed settingFile field to adaptFile
// [2016.03.15]
//      - [SC] changed TargetScenarioID method
//      - [SC] changed UpdateRatings method
//      - [SC] changed PlayerSetting methods
//      - [SC] changed ScenarioSetting methods
//      - [SC] renamed PlayerSetting methods into PlayerParam
//      - [SC] renamed ScenarioSetting methods into PlayerParam
//      - [SC] changed AllScenariosIDs method
//      - [SC] added DATE_FORMAT constant
//      - [SC] added SaveAdaptationData method
//      - [SC] added SaveGameplayData method
//      - [SC] changed CreateNewRecord method
// [2016.03.16]
//      - [SC] added 'updateDatafiles' parameter to the UpdateRatings method's signature
//      - [SC] added 'updateDatafiles' parameter to the CreateNewRecord method's signature
//      - [SC] the design document was updated to reflect changes in the source code. Refer to https://rage.ou.nl/index.php?q=filedepot_download/358/501
// [2016.03.16]
//      - [SC] Removed HATAssetSettings class from the project
//      - [SC] Added the TestApp project to run simple tests to verify proper functionality of the asset. The project can be safely removed if it is not necessary.
// [2016.10.06]
//      - [SC] renamed namespace 'HAT' to 'TwoA'
//      - [SC] renamed class 'HATAsset' to 'TwoA'
//      - [SC] changed the value of 'adaptFile' field from 'HATAssetAppSeettings.xml' to 'TwoAAppSettings.xml'
// [2016.10.07]
//      - [SC] added instantiation of SimpleRNG to InitSettings method (moved from DifficultyAdapter constructor)
// [2016.11.14]
//      - [SC] deleted ObjectUtils.cs; it is not used anymore
// [2016.11.16]
//      - [SC] added a portable project
// [2016.11.23]
//      - [SC] enabled logs in 'UpdateRatings' and 'TargetScenarioID' methods
// [2016.11.29]
//      - [SC] removed the 'Resources\gameplaylogs.xml'
//      - [SC] removed the 'Resources\TwoAAppSettings.xml'
//      - [SC] moved files in 'Resources\Test\' to 'Resources' folder in the 'TestApp' project


// TODO:
//      - InitSettings need total rewriting
//      - Different adaptation algorithms may need different XML nodes; need a XML reading method independent of XML structure
//      - Validate XML files against schema

#endregion Header

namespace TwoA
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    using AssetPackage;

    /// <summary>
    /// A TwoA asset.
    /// </summary>
    public class TwoA : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Date/time format used in TwoA
        /// </summary>
        public const string DATE_FORMAT = "yyyy-MM-ddThh:mm:ss";

        /// The adapter.
        private DifficultyAdapter adapter;
        private GameplaysData gameplaydata;
        private AdaptationData adaptData;

        /// <summary>
        /// The logfile.
        /// </summary>
        private string gameplayLogsFile = "gameplaylogs.xml";

        /// <summary>
        /// The setting file.
        /// </summary>
        private string adaptFile = "TwoAAppSettings.xml";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets datetime format
        /// </summary>
        ///
        /// <value>
        /// Datetime format as string.
        /// </value> 
        internal string DateFormat {
            get { return DATE_FORMAT; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TwoA.TwoA class.
        /// </summary>
        ///
        /// <remarks>
        /// Please use TwoA(IBridge bridge) and a Bridge implementing at least
        /// IDataStorage instead as this Asset needs to load data <br/>
        /// OR <br/>
        /// call the InitSettings() method after creation and configuring a Bridge.
        /// </remarks>
        public TwoA() : this(null) {
            //
        }

        /// <summary>
        /// Initializes a new instance of the TwoA.TwoA class.
        /// </summary>
        ///
        /// <param name="bridge"> The bridge. </param>
        public TwoA(IBridge bridge) : base(bridge) {
            InitSettings();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initialises the settings.
        /// </summary>
        public void InitSettings() {
            new SimpleRNG(); // [SC][2016.01.07] make sure that static constructor was called and initialized seed values; should not necessary; but there might be bug in .NET 4.0

            // [SC] Load XML file containing adaptation data 
            LoadAdaptationData();

            // [SC] load XML file coontaining log records of all gameplays 
            LoadGameplayData();

            // [SC] create the ELO_CAT adapter
            this.adapter = new DifficultyAdapter(this);
        }

        /// <summary>
        /// Gets a list containing all scenarios IDs.
        /// </summary>
        ///
        /// <param name="adaptID"> Identifier for the adapt. </param>
        /// <param name="gameID">  Identifier for the game. </param>
        ///
        /// <returns>
        /// all scenarios.
        /// </returns>
        public List<string> AllScenariosIDs(string adaptID, string gameID) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    ScenarioDataNode scenarioDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).ScenarioData;

                    return scenarioDataNode.ScenarioList.OrderBy(p => p.Rating).Select(p => p.ScenarioID).ToList<string>();
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to retrieve scenario IDs for game {0} with adaptation {1}.", adaptID, gameID));
        }

        /// <summary>
        /// Get the Target scenario ID from the adapter.
        /// </summary>
        ///
        /// <param name="adaptID">  Identifier for the adapt. </param>
        /// <param name="gameID">   Identifier for the game. </param>
        /// <param name="playerID"> Identifier for the player. </param>
        ///
        /// <returns>
        /// A string.
        /// </returns>
        public string TargetScenarioID(string adaptID, string gameID, string playerID) {
            Log(Severity.Verbose
                , @"TwoA.TargetScenarioID('{0}','{1}','{2}')"
                , adaptID, gameID, playerID);

            if (adaptID.Equals(adapter.Type)) {
                return adapter.TargetScenarioID(gameID, playerID);
            }
            else {
                return null; // [TODO]
            }
        }

        /// <summary>
        /// Updates the ratings of the adapter.
        /// </summary>
        ///
        /// <param name="adaptID">          Identifier for the adapt. </param>
        /// <param name="gameID">           Identifier for the game. </param>
        /// <param name="playerID">         Identifier for the player. </param>
        /// <param name="scenarioID">       Identifier for the scenario. </param>
        /// <param name="rt">               The right. </param>
        /// <param name="correctAnswer">    The correct answer. </param>
        /// <param name="updateDatafiles">  Set to true to update adaptation and gameplay logs files. </param>
        public void UpdateRatings(string adaptID, string gameID, string playerID, string scenarioID, double rt, double correctAnswer, bool updateDatafiles) {
            Log(Severity.Verbose
                , @"TwoA.UpdateRatings('{0}','{1}','{2}','{3}',{4:0.0},{5:0.0})"
                , adaptID, gameID, playerID, scenarioID, rt, correctAnswer);

            if (adaptID.Equals(adapter.Type)) {
                adapter.UpdateRatings(gameID, playerID, scenarioID, rt, correctAnswer, updateDatafiles);
            }
            else {
                return; // [TODO]
            }
        }

        /// <summary>
        /// Creates new record to the game log.
        /// </summary>
        ///
        /// <param name="adaptID">          Identifier for the adapt. </param>
        /// <param name="gameID">           Identifier for the game. </param>
        /// <param name="playerID">         Identifier for the player. </param>
        /// <param name="scenarioID">       Identifier for the scenario. </param>
        /// <param name="rt">               The right. </param>
        /// <param name="accuracy">         The correct answer. </param>
        /// <param name="playerRating">     The player new rating. </param>
        /// <param name="scenarioRating">   The scenario new rating. </param>
        /// <param name="timestamp">        The current date time. </param>
        /// <param name="updateDatafiles">  Set to true to update adaptation and gameplay logs files. </param>
        public void CreateNewRecord(string adaptID, string gameID, string playerID, string scenarioID
                                        , double rt, double accuracy
                                        , double playerRating, double scenarioRating, DateTime timestamp, bool updateDatafiles) {

            // Check if Adaption is there.
            if (gameplaydata.Adaptation.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                TwoAAdaptation adaptNode = gameplaydata.Adaptation.First(p => p.AdaptationID.Equals(adaptID));

                // Check if Game is there
                if (adaptNode.Game.Count(p => p.GameID.Equals(gameID)) == 1) {
                    TwoAGame gameNode = adaptNode.Game.First(p => p.GameID.Equals(gameID));

                    gameNode.Gameplay.Add(
                        new TwoAGameplay() {
                            PlayerID = playerID
                            , ScenarioID = scenarioID
                            , Timestamp = timestamp.ToString(DATE_FORMAT)
                            , RT = rt
                            , Accuracy = accuracy
                            , PlayerRating = playerRating
                            , ScenarioRating = scenarioRating
                        }
                    );

                    // [SC] save the modified data into local XML file
                    if (updateDatafiles) {
                        SaveGameplayData();
                    }

                    return;
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to log a gameplay record for player {0} playing scenario {1} with adaptation {2} in game {3}."
                                                            , playerID, scenarioID, adaptID, gameID));
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for player data

        #region Player param getters

        // [2016.11.14]
        /// <summary>
        /// Get a value of Rating for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// 
        /// <returns>
        /// Rating as double value.
        /// </returns>
        public double PlayerRating(string adaptID, string gameID, string playerID) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to get Rating for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                return player.Rating;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of PlayCount for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// 
        /// <returns>
        /// PlayCount as double value.
        /// </returns>
        public double PlayerPlayCount(string adaptID, string gameID, string playerID) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to get PlayCount for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                return player.PlayCount;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of KFactor for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// 
        /// <returns>
        /// KFactor as double value.
        /// </returns>
        public double PlayerKFactor(string adaptID, string gameID, string playerID) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to get KFactor for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                return player.KFactor;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of Uncertainty for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// 
        /// <returns>
        /// Uncertainty as double value.
        /// </returns>
        public double PlayerUncertainty(string adaptID, string gameID, string playerID) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to get Uncertainty for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                return player.Uncertainty;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of LastPlayed for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// 
        /// <returns>
        /// LastPlayed as DateTime object.
        /// </returns>
        public DateTime PlayerLastPlayed(string adaptID, string gameID, string playerID) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to get LastPlayed for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                return DateTime.ParseExact(player.LastPlayed, TwoA.DATE_FORMAT, null);
            }
        }

        #endregion Player param getters

        #region Player param setters

        // [2016.11.14]
        /// <summary>
        /// Set a Rating value for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="rating">       The value of Rating. </param>
        public void PlayerRating(string adaptID, string gameID, string playerID, double rating) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to set Rating for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                player.Rating = rating;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a PlayCount value for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="playCount">    The value of PlayCount. </param>
        public void PlayerPlayCount(string adaptID, string gameID, string playerID, double playCount) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to set PlayCount for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                player.PlayCount = playCount;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a KFactor value for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="kFactor">      The value of KFactor. </param>
        public void PlayerKFactor(string adaptID, string gameID, string playerID, double kFactor) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to set KFactor for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                player.KFactor = kFactor;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set an Uncertainty value for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="uncertainty">  The value of Uncertainty. </param>
        public void PlayerUncertainty(string adaptID, string gameID, string playerID, double uncertainty) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to set Uncertainty for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                player.Uncertainty = uncertainty;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a LastPlayed datetime for a player.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching player is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="lastPlayed">   The DateTime object for LastPlayed datetime. </param>
        public void PlayerLastPlayed(string adaptID, string gameID, string playerID, DateTime lastPlayed) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                throw new NullReferenceException(String.Format("Unable to set LastPlayed for player {0} for adaptation {1} in game {2}."
                                                                , playerID, adaptID, gameID)); // [TODO]
            }
            else {
                player.LastPlayed = lastPlayed.ToString(TwoA.DATE_FORMAT);
            }
        }

        #endregion Player param setters

        #region PlayerNode getter
        
        // [2016.11.14]
        /// <summary>
        /// Get a PlayerNode with a given player ID.
        /// </summary>
        ///
        /// <param name="adaptID">  Identifier for the adapt. </param>
        /// <param name="gameID">   Identifier for the game. </param>
        /// <param name="playerID"> Identifier for the player. </param>
        ///
        /// <returns>
        /// PlayerNode object, or null if no ID match is found.
        /// </returns>
        public PlayerNode Player(string adaptID, string gameID, string playerID) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    PlayerDataNode playerDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).PlayerData;

                    // [SC] verify that the player exists
                    if (playerDataNode.PlayerList.Count(p => p.PlayerID.Equals(playerID)) == 1) {
                        return playerDataNode.PlayerList.First(p => p.PlayerID.Equals(playerID));
                    }
                }
            }

            return null;
        }

        #endregion PlayerNode getter

        ////// END: methods for player data
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for scenario data

        #region Scenario param getters

        // [2016.11.14]
        /// <summary>
        /// Get a value of Rating for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// Rating as double value.
        /// </returns>
        public double ScenarioRating(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get Rating for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return scenario.Rating;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of PlayCount for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// PlayCount as double value.
        /// </returns>
        public double ScenarioPlayCount(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get PlayCount for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return scenario.PlayCount;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of KFactor for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// KFactor as double value.
        /// </returns>
        public double ScenarioKFactor(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get KFactor for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return scenario.KFactor;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of Uncertainty for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// Uncertainty as double value.
        /// </returns>
        public double ScenarioUncertainty(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get Uncertainty for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return scenario.Uncertainty;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of LastPlayed for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// LastPlayed as DateTime object.
        /// </returns>
        public DateTime ScenarioLastPlayed(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get LastPlayed for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return DateTime.ParseExact(scenario.LastPlayed, TwoA.DATE_FORMAT, null);
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Get a value of TimeLimit for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// 
        /// <returns>
        /// TimeLimit as double value.
        /// </returns>
        public double ScenarioTimeLimit(string adaptID, string gameID, string scenarioID) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to get TimeLimit for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                return scenario.TimeLimit;
            }
        }

        #endregion Scenario param getters

        #region Scenario param setters

        // [2016.11.14]
        /// <summary>
        /// Set a Rating value for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="rating">       The value of Rating. </param>
        public void ScenarioRating(string adaptID, string gameID, string scenarioID, double rating) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set Rating for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.Rating = rating;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a PlayCount value for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="playCount">    The value of PlayCount. </param>
        public void ScenarioPlayCount(string adaptID, string gameID, string scenarioID, double playCount) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set PlayCount for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.PlayCount = playCount;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a KFactor value for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="kFactor">      The value of KFactor. </param>
        public void ScenarioKFactor(string adaptID, string gameID, string scenarioID, double kFactor) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set KFactor for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.KFactor = kFactor;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set an Uncertainty value for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="uncertainty">  The value of Uncertainty. </param>
        public void ScenarioUncertainty(string adaptID, string gameID, string scenarioID, double uncertainty) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set Uncertainty for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.Uncertainty = uncertainty;
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a LastPlayed datetime for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="lastPlayed">   The DateTime object for LastPlayed datetime. </param>
        public void ScenarioLastPlayed(string adaptID, string gameID, string scenarioID, DateTime lastPlayed) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set LastPlayed for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.LastPlayed = lastPlayed.ToString(TwoA.DATE_FORMAT);
            }
        }

        // [2016.11.14]
        /// <summary>
        /// Set a TimeLimit for a scenario.
        /// </summary>
        /// 
        /// <exception cref="NullReferenceException">    Thrown when matching scenario is not found 
        ///                                              and null is returned. </exception>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="timeLimit">    The value of TimeLimit. </param>
        public void ScenarioTimeLimit(string adaptID, string gameID, string scenarioID, double timeLimit) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                throw new NullReferenceException(String.Format("Unable to set TimeLimit for scenario {0} for adaptation {1} in game {2}."
                                                                , scenarioID, adaptID, gameID)); // [TODO]
            }
            else {
                scenario.TimeLimit = timeLimit;
            }
        }

        #endregion Scenario param setters

        #region ScenarioNode getter

        // [2016.11.14]
        /// <summary>
        /// Get a ScenarioNode with a given scenario ID.
        /// </summary>
        ///
        /// <param name="adaptID">    Identifier for the adapt. </param>
        /// <param name="gameID">     Identifier for the game. </param>
        /// <param name="scenarioID"> Identifier for the scenario. </param>
        ///
        /// <returns>
        /// ScenarioNode object, or null if no ID match is found.
        /// </returns>
        public ScenarioNode Scenario(string adaptID, string gameID, string scenarioID) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    ScenarioDataNode scenarioDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).ScenarioData;

                    // [SC] verify that the scenario exists
                    if (scenarioDataNode.ScenarioList.Count(p => p.ScenarioID.Equals(scenarioID)) == 1) {
                        return scenarioDataNode.ScenarioList.First(p => p.ScenarioID.Equals(scenarioID));
                    }
                }
            }

            return null;
        }

        #endregion ScenarioNode getter

        ////// END: methods for scenario data
        //////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads adaptation data.
        /// </summary>
        private void LoadAdaptationData() {
            IDataStorage ds = getInterface<IDataStorage>();

            if (ds == null) {
                throw new ArgumentException(String.Format("Unable to load the file for adaptation data '{0}'. Cannot access IDataStorage interface.", adaptFile));
            }

            if (!ds.Exists(adaptFile)) {
                throw new ArgumentException(String.Format("The file for adaptation data '{0}' does not exist.", adaptFile));
            }

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ds.Load(adaptFile)))) {
                XmlSerializer serializer = new XmlSerializer(typeof(AdaptationData));

                adaptData = (AdaptationData)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// Saves AdaptationData to local XML file. Needs IDataStorage interface from RAGE architecture.
        /// </summary>
        internal void SaveAdaptationData() {
            IDataStorage ds = getInterface<IDataStorage>();

            if (ds == null) {
                throw new ArgumentException(String.Format("Unable to open file for adaptation data '{0}'. Cannot access IDataStorage interface.", adaptFile));
            }

            if (!ds.Exists(adaptFile)) {
                throw new ArgumentException(String.Format("The file for adaptation data '{0}' does not exist.", adaptFile));
            }

            XmlSerializer ser = new XmlSerializer(typeof(AdaptationData));

            using (StringWriterUtf8 textWriter = new StringWriterUtf8()) {
                //! Use DataContractSerializer or DataContractJsonSerializer?
                // See https://msdn.microsoft.com/en-us/library/bb412170(v=vs.100).aspx
                // See https://msdn.microsoft.com/en-us/library/bb924435(v=vs.110).aspx
                // See https://msdn.microsoft.com/en-us/library/aa347875(v=vs.110).aspx
                //
                ser.Serialize(textWriter, adaptData);

                textWriter.Flush();

                ds.Save(adaptFile, textWriter.ToString());
            }
        }

        /// <summary>
        /// Loads gameplay data.
        /// </summary>
        private void LoadGameplayData() {
            IDataStorage ds = getInterface<IDataStorage>();

            if (ds == null) {
                throw new ArgumentException(String.Format("Unable to load the file for gameplay logs '{0}'. Cannot access IDataStorage interface.", gameplayLogsFile));
            }

            if (!ds.Exists(gameplayLogsFile)) {
                throw new ArgumentException(String.Format("The file for gameplay logs '{0}' does not exist.", gameplayLogsFile));
            }

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ds.Load(gameplayLogsFile)))) {
                XmlSerializer serializer = new XmlSerializer(typeof(GameplaysData));

                gameplaydata = (GameplaysData)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// Saves GameplaysData to local XML file. Needs IDataStorage interface from RAGE architecture.
        /// </summary>
        internal void SaveGameplayData() {
            IDataStorage ds = getInterface<IDataStorage>();

            if (ds == null) {
                throw new ArgumentException(String.Format("Unable to open file for gameplay logs {0}. Cannot access IDataStorage interface.", gameplayLogsFile));
            }

            if (!ds.Exists(gameplayLogsFile)) {
                throw new ArgumentException(String.Format("The file for gameplay logs '{0}' does not exist.", gameplayLogsFile));
            }

            XmlSerializer ser = new XmlSerializer(typeof(GameplaysData));

            using (StringWriterUtf8 textWriter = new StringWriterUtf8()) {
                //! Use DataContractSerializer or DataContractJsonSerializer?
                // See https://msdn.microsoft.com/en-us/library/bb412170(v=vs.100).aspx
                // See https://msdn.microsoft.com/en-us/library/bb924435(v=vs.110).aspx
                // See https://msdn.microsoft.com/en-us/library/aa347875(v=vs.110).aspx
                //
                ser.Serialize(textWriter, gameplaydata);

                textWriter.Flush();

                ds.Save(gameplayLogsFile, textWriter.ToString());
            }
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// A string writer utf-8.
        /// </summary>
        ///
        /// <remarks>
        /// Fix-up for XDocument Serialization defaulting to utf-16.
        /// </remarks>
        internal class StringWriterUtf8 : StringWriter
        {
            #region Properties

            public override Encoding Encoding {
                get { return Encoding.UTF8; }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}