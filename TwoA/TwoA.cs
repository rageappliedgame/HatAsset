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
// [2016.12.02]     
//      - [SC] added GameplaysData property
//      - [SC] added AdaptationData property
// [2016.12.06]
//      - [SC] removed SimpleRNG instantiation from InitSettings method
// [2017.01.03]
//      - [SC] added 'CalculateScore' method
// [2017.01.05]
//      - [SC] added 'SetTargetDistribution' method
//      - [SC] added 'SetDefaultTargetDistribution' method
// [2017.02.09]
//      - [SC] modified the signature and the body of 'UpdateRatings' method. Updating scenario parameters is optional.
// [2017.02.10]
//      - [SC] added 'private DifficultyAdapterElo adapterElo' field
//      - [SC] 'TargetScenarioID' methods supports 'DifficultyAdapterElo'
//      - [SC] 'UpdateRatings' method support 'DifficultyAdapterElo'
//      - [SC] 'SetTargetDistribution' method supports 'DifficultyAdapterElo'
//      - [SC] 'SetDefaultTargetDistribution' method supports 'DifficultyAdapterElo'
// [2017.02.14]
//      - [SC] added a public method 'AvailableAdapters'
//      - [SC] added a public method 'GetAdapter'; currently it is commented; not sure if it is a good idea to make instances accessible from outside
// [2017.02.16]
//      - [SC] added 'GetTargetDistribution'
//      - [SC] added 'GetFiSDMultiplier', 'SetFiSDMultiplier', 'SetDefaultFiSDMultiplier' methods
//      - [SC] added 'GetMaxDelay', 'SetMaxDelay', 'SetDefaultMaxDelay' methods
// []
//      - [SC] added 'GetMaxPlay', 'SetMaxPlay', 'SetDefaultMaxPlay' methods
//      - [SC] added 'GetKConst', 'SetKConst', 'SetDefaultKConst' methods
//      - [SC] added 'GetKUp', 'SetKUp', 'SetDefaultKUp' methods
//      - [SC] added 'GetKDown', 'SetKDown', 'SetDefaultKDown' methods
//      - [SC] added 'GetExpectScoreMagnifier', 'SetExpectScoreMagnifier', 'SetDefaultExpectScoreMagnifier' methods
//      - [SC] added 'GetMagnifierStepSize', 'SetMagnifierStepSize', 'SetDefaultMagnifierStepSize' methods
//

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
        #region Constants

        /// <summary>
        /// Date/time format used in TwoA
        /// </summary>
        public const string DATE_FORMAT = "yyyy-MM-ddThh:mm:ss";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The adapter based on accuracy (0 or 1) and response time measured in milliseconds.
        /// </summary>
        private DifficultyAdapter adapter;

        /// <summary>
        /// The adapter based on accuracy only (any value within [0, 1]); uses Elo equation for expected score.
        /// </summary>
        private DifficultyAdapterElo adapterElo;

        /// <summary>
        /// List of available players.
        /// </summary>
        public List<PlayerNode> players;

        /// <summary>
        /// List of available players.
        /// </summary>
        public List<ScenarioNode> scenarios;

        /// <summary>
        /// Gameplays
        /// </summary>
        public List<Gameplay> gameplays;

        #endregion Fields

        #region Constructors

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
        private void InitSettings() {
            // [SC] list of available players
            this.players = new List<PlayerNode>();

            // [SC] list of available scenarios
            this.scenarios = new List<ScenarioNode>();

            // [SC] list of gameplays
            this.gameplays = new List<Gameplay>();

            // [SC] create the TwoA adapter
            this.adapter = new DifficultyAdapter(this);

            // [SC] create the TwoA-Elo adapter
            this.adapterElo = new DifficultyAdapterElo(this);
        }

        #region Methods for target scenario retrievals

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
            return TargetScenarioID(Player(adaptID, gameID, playerID));
        }

        /// <summary>
        /// Get the Target scenario ID from the adapter.
        /// </summary>
        ///
        /// <param name="playerNode"> Player node. </param>
        ///
        /// <returns>
        /// A string.
        /// </returns>
        public string TargetScenarioID(PlayerNode playerNode) {
            ScenarioNode scenarioNode = TargetScenario(playerNode);

            if (scenarioNode == null) {
                Log(Severity.Error, "In TwoA.TargetScenarioID: Unable to recommend a scenario ID. Returning null.");
                return null;
            }

            return scenarioNode.ScenarioID;
        }

        /// <summary>
        /// Get the Target scenario from the adapter.
        /// </summary>
        ///
        /// <param name="adaptID">  Identifier for the adapt. </param>
        /// <param name="gameID">   Identifier for the game. </param>
        /// <param name="playerID"> Identifier for the player. </param>
        ///
        /// <returns>
        /// ScenarioNode of the recommended scenario.
        /// </returns>
        public ScenarioNode TargetScenario(string adaptID, string gameID, string playerID) {
            return TargetScenario(Player(adaptID, gameID, playerID));
        }

        /// <summary>
        /// Get the Target scenario from the adapter.
        /// </summary>
        ///
        /// <param name="playerNode"> Player node. </param>
        ///
        /// <returns>
        /// ScenarioNode of the recommended scenario.
        /// </returns>
        public ScenarioNode TargetScenario(PlayerNode playerNode) {
            if (playerNode == null) {
                Log(Severity.Error, "In TwoA.TargetScenario: Null player node object. Returning null.");
                return null;
            }

            // [SC] get available scenario nodes
            List<ScenarioNode> scenarioList = AllScenarios(playerNode.AdaptationID, playerNode.GameID);
            if (scenarioList == null || scenarioList.Count == 0) {
                Log(Severity.Error, "In TwoA.TargetScenario: Unable to retrieve scenario node list. Returning null.");
                return null;
            }

            return TargetScenario(playerNode, scenarioList);
        }

        /// <summary>
        /// Get the Target scenario from the adapter.
        /// </summary>
        /// <param name="playerNode">       Player node. </param>
        /// <param name="scenarioList">     List of scenario nodes from which to recommend. </param>
        /// <returns>
        /// ScenarioNode of the recommended scenario.
        /// </returns>
        public ScenarioNode TargetScenario(PlayerNode playerNode, List<ScenarioNode> scenarioList) {
            if (playerNode == null) {
                Log(Severity.Error, "In TwoA.TargetScenario: Null player node object. Returning null.");
                return null;
            }

            if (scenarioList == null || scenarioList.Count == 0) {
                Log(Severity.Error, "In TwoA.TargetScenario: Unable to retrieve scenario node list. Returning null.");
                return null;
            }

            if (playerNode.AdaptationID.Equals(adapter.Type)) {
                return adapter.TargetScenario(playerNode, scenarioList);
            }
            else if (playerNode.AdaptationID.Equals(adapterElo.Type)) {
                return adapterElo.TargetScenario(playerNode, scenarioList);
            }
            else {
                Log(Severity.Error, String.Format("In TwoA.TargetScenario: Unknown adapter {0}. Returning null.", playerNode.AdaptationID));
                return null;
            }
        }

        #endregion Methods for target scenario retrievals

        #region Methods for target difficulty rating retrieval

        /// <summary>
        /// Returns target difficulty rating given a player's skill rating.
        /// </summary>
        /// <param name="adaptID">          Adaptation ID.</param>
        /// <param name="playerRating">     Player's skill rating.</param>
        /// <returns>Difficulty rating</returns>
        public double TargetDifficultyRating(string adaptID, double playerRating) {
            if (String.IsNullOrEmpty(adaptID)) {
                Log(Severity.Error, "In TwoA.TargetDifficultyRating: Null player node object. Returning 0.");
                return 0;
            }

            if (adaptID.Equals(adapter.Type)) {
                return adapter.TargetDifficultyRating(playerRating);
            }
            else if (adaptID.Equals(adapterElo.Type)) {
                return adapterElo.TargetDifficultyRating(playerRating);
            }
            else {
                Log(Severity.Error, String.Format("In TwoA.TargetDifficultyRating: Unknown adapter {0}. Returning 0.", adaptID));
                return 0;
            }
        }

        /// <summary>
        /// Returns target difficulty rating given a player's skill rating.
        /// </summary>
        /// <param name="playerNode">Player's node</param>
        /// <returns>Difficulty rating</returns>
        public double TargetDifficultyRating(PlayerNode playerNode) {
            if (playerNode == null) {
                Log(Severity.Error, "In TwoA.TargetDifficultyRating: Null player node object. Returning 0.");
                return 0;
            }

            return TargetDifficultyRating(playerNode.AdaptationID, playerNode.Rating);
        }

        /// <summary>
        /// Returns target difficulty rating given a player's skill rating.
        /// </summary>
        /// <param name="adaptID">      Adaptation ID.</param>
        /// <param name="gameID">       Game ID.</param>
        /// <param name="playerID">     Player ID.</param>
        /// <returns>Difficulty rating</returns>
        public double TargetDifficultyRating(string adaptID, string gameID, string playerID) {
            return TargetDifficultyRating(Player(adaptID, gameID, playerID));
        }

        #endregion Methods for target difficulty rating retrieval

        #region Methods for updating ratings

        /// <summary>
        /// Updates the ratings based on player's performance in a scenario.
        /// </summary>
        ///
        /// <param name="adaptID">                  Identifier for the adapt. </param>
        /// <param name="gameID">                   Identifier for the game. </param>
        /// <param name="playerID">                 Identifier for the player. </param>
        /// <param name="scenarioID">               Identifier for the scenario. </param>
        /// <param name="rt">                       The response time. </param>
        /// <param name="correctAnswer">            The correct answer. </param>
        /// <param name="updateScenarioRating">     Set to false if updating scenario parameters is not necessary. </param>
        /// <param name="customKfct">               If non-0 value is provided then it is used as a weight to scale changes in player's and scenario's ratings. Otherwise, adapter calculates its own K factors. </param>
        /// <returns>True if updates are successfull, and false otherwise.</returns>
        public bool UpdateRatings(string adaptID, string gameID, string playerID, string scenarioID
                                    , double rt, double correctAnswer
                                    , bool updateScenarioRating, double customKfct) {

            PlayerNode playerNode = Player(adaptID, gameID, playerID);
            ScenarioNode scenarioNode = Scenario(adaptID, gameID, scenarioID);

            return UpdateRatings(playerNode, scenarioNode, rt, correctAnswer, updateScenarioRating, customKfct);
        }

        /// <summary>
        /// Updates the ratings based on player's performance in a scenario.
        /// </summary>
        /// <param name="playerNode">               Player node to be updated. </param>
        /// <param name="scenarioNode">             Scenario node to be updated. </param>
        /// <param name="rt">                       Player's response time. </param>
        /// <param name="correctAnswer">            Player's accuracy. </param>
        /// <param name="updateScenarioRating">     Set to false to avoid updating scenario node. </param>
        /// <param name="customKfct">               If non-0 value is provided then it is used as a weight to scale changes in player's and scenario's ratings. Otherwise, adapter calculates its own K factors. </param>
        /// <returns>True if updates are successfull, and false otherwise.</returns>
        public bool UpdateRatings(PlayerNode playerNode, ScenarioNode scenarioNode, double rt, double correctAnswer
                                , bool updateScenarioRating, double customKfct) {
            if (playerNode == null) {
                Log(Severity.Error, "In TwoA.UpdateRatings: Player node is null. No update is done.");
                return false;
            }

            if (scenarioNode == null) {
                Log(Severity.Error, "In TwoA.UpdateRatings: Scenario node is null. No update is done.");
                return false;
            }

            if (!playerNode.AdaptationID.Equals(scenarioNode.AdaptationID)) {
                Log(Severity.Error, "In TwoA.UpdateRatings: Inconsistent adaptation IDs in player and scenario nodes. No update is done.");
                return false;
            }

            if (!playerNode.GameID.Equals(scenarioNode.GameID)) {
                Log(Severity.Error, "In TwoA.UpdateRatings: Inconsistent game IDs in player and scenario nodes. No update is done.");
                return false;
            }

            if (playerNode.AdaptationID.Equals(adapter.Type)) {
                return adapter.UpdateRatings(playerNode, scenarioNode, rt, correctAnswer, updateScenarioRating, customKfct, customKfct);
            }
            else if (playerNode.AdaptationID.Equals(adapterElo.Type)) {
                return adapterElo.UpdateRatings(playerNode, scenarioNode, rt, correctAnswer, updateScenarioRating, customKfct, customKfct);
            }
            else {
                Log(Severity.Error, String.Format("In TwoA.UpdateRatings: Unknown adapter {0}. No update is done.", playerNode.AdaptationID));
                return false;
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
        public void CreateNewRecord(string adaptID, string gameID, string playerID, string scenarioID
                                        , double rt, double accuracy
                                        , double playerRating, double scenarioRating, DateTime timestamp) {
            this.gameplays.Add(
                new Gameplay() {
                    AdaptationID = adaptID,
                    GameID = gameID,
                    PlayerID = playerID,
                    ScenarioID = scenarioID,
                    Timestamp = timestamp.ToString(TwoA.DATE_FORMAT),
                    RT = rt,
                    Accuracy = accuracy,
                    PlayerRating = playerRating,
                    ScenarioRating = scenarioRating,
                }
            );
        }

        #endregion Methods for updating ratings

        /// <summary>
        /// Calculates a normalized score based on player's performance defined by response time and accuracy.
        /// </summary>
        /// 
        /// <param name="correctAnswer">    1 if player provided correct answer and 0 otherwise</param>
        /// <param name="responseTime">     Players response time in milliseconds</param>
        /// <param name="itemMaxDuration">  Max allowed time in millisecond given to player to solve the problem.</param>
        /// 
        /// <returns>A score within range (-1, 1)</returns>
        public double CalculateScore(double correctAnswer, double responseTime, double itemMaxDuration) {
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

            return adapter.calcActualScore(correctAnswer, responseTime, itemMaxDuration);
        }

        /// <summary>
        /// Calculates player's expected score based on player's skill rating and scenarios difficulty rating.
        /// </summary>
        /// <param name="adaptID">          Adaptation ID</param>
        /// <param name="playerRating">     Player's skill rating</param>
        /// <param name="scenarioRating">   Scenario's difficulty rating</param>
        /// <param name="itemMaxDuration">  Max allowed time in millisecond given to player to solve the problem.</param>
        /// <returns>Expected score or error code.</returns>
        public double CalculateExpectedScore(string adaptID, double playerRating, double scenarioRating, double itemMaxDuration) {
            if (adaptID.Equals(adapter.Type)) {
                return adapter.calcExpectedScore(playerRating, scenarioRating, itemMaxDuration);
            }
            else if (adaptID.Equals(adapterElo.Type)) {
                return adapterElo.calcExpectedScore(playerRating, scenarioRating);
            }
            else {
                Log(Severity.Error,
                    String.Format("In TwoA.CalculateExpectedScore: Unknown adapter {0}. No update is done. Returning error code {1}.", adaptID, BaseAdapter.ERROR_CODE));
                return BaseAdapter.ERROR_CODE;
            }
        }

        /// <summary>
        /// Returns a 2D array with descriptions of available adapters.
        /// The first column contains class name.
        /// The second column contains adapter IDs. 
        /// The third column contains adapter descriptions.
        /// </summary>
        /// 
        /// <returns>2D array of strings</returns>
        public string[,] AvailableAdapters() { 
            return new string[,] { 
                {adapter.GetType().Name, adapter.Type, adapter.Description }
                , {adapterElo.GetType().Name, adapterElo.Type, adapterElo.Description}
            };
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for setting adapter parameters
        #region Methods for setting adapter parameters

        #region Methods for target distribution

        /// <summary>
        /// Returns the mean, sd, lower and upper limits of target distribution as an array.
        /// </summary>
        /// <param name="adaptID">Adapter ID.</param>
        /// <returns>An array with four elements.</returns>
        public double[] GetTargetDistribution(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return new double[] { this.adapter.TargetDistrMean, this.adapter.TargetDistrSD
                                    , this.adapter.TargetLowerLimit, this.adapter.TargetUpperLimit };
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return new double[] { this.adapterElo.TargetDistrMean, this.adapterElo.TargetDistrSD
                                    , this.adapterElo.TargetLowerLimit, this.adapterElo.TargetUpperLimit };
            }
            else {
                Log(AssetPackage.Severity.Error
                        , String.Format("In 'TwoA.GetTargetDistribution' method: adapter ID '{0}' is not recognized. Returning null."
                        , adaptID, BaseAdapter.ErrorCode));
                return null;
            }
        }

        /// <summary>
        /// Sets the target distribution parameters to custom value.
        /// </summary>
        /// <param name="adaptID">      Adapter ID.</param>
        /// <param name="mean">         Distribution mean.</param>
        /// <param name="sd">           Distribution standard deviation.</param>
        /// <param name="lowerLimit">   Distribution lower limit.</param>
        /// <param name="upperLimit">   Distribution upper limit.</param>
        public void SetTargetDistribution(string adaptID, double mean, double sd, double lowerLimit, double upperLimit) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setTargetDistribution(mean, sd, lowerLimit, upperLimit);
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setTargetDistribution(mean, sd, lowerLimit, upperLimit);
            } 
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetTargetDistribution' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the target distribution parameters to default values.
        /// </summary>
        /// <param name="adaptID">  Adapter ID.</param>
        public void SetDefaultTargetDistribution(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultTargetDistribution();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultTargetDistribution();
            }
            else {
                Log(AssetPackage.Severity.Error
                        , String.Format("In 'TwoA.SetDefaultTargetDistribution' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        #endregion Methods for target distribution

        #region Methods for fuzzy intervals

        /// <summary>
        /// Gets the fuzzy interval SD multiplier.
        /// </summary>
        /// <param name="adaptID">Adapter ID></param>
        /// <returns>Multiplier value, or 0 if the adapter is not found.</returns>
        public double GetFiSDMultiplier(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.FiSDMultiplier;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.FiSDMultiplier;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetFiSDMultiplier' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Sets the fuzzy interval SD multiplier
        /// </summary>
        /// <param name="adaptID">      Adapter ID.</param>
        /// <param name="multiplier">   The value of the multiplier.</param>
        public void SetFiSDMultiplier(string adaptID, double multiplier) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.FiSDMultiplier = multiplier;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.FiSDMultiplier = multiplier;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetFiSDMultiplier' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the fuzzy interval SD multiplier to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID.</param>
        public void SetDefaultFiSDMultiplier(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultFiSDMultiplier();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultFiSDMultiplier();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultFiSDMultiplier' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        #endregion Methods for fuzzy intervals

        #region Methods for uncertainty parameters

        /// <summary>
        /// Gets the maximum delay for the uncertainty calculation.
        /// </summary>
        /// <param name="adaptID"> Adapter ID.</param>
        /// <returns>The number of days as double value, or 0 if adapter is not found</returns>
        public double GetMaxDelay(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.MaxDelay;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.MaxDelay;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetMaxDelay' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Sets the maximum delay for uncertainty calculation.
        /// </summary>
        /// <param name="adaptID">  Adapter ID.</param>
        /// <param name="maxDelay"> Maximum delay in days.</param>
        public void SetMaxDelay(string adaptID, double maxDelay) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.MaxDelay = maxDelay;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.MaxDelay = maxDelay;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetMaxDelay' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the maximum delay for uncertainty calculation to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID.</param>
        public void SetDefaultMaxDelay(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultMaxDelay();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultMaxDelay();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultMaxDelay' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Get the maximum play count for uncertainty calculation.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        /// <returns>The number of play counts as double value.</returns>
        public double GetMaxPlay(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.MaxPlay;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.MaxPlay;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetMaxPlay' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the maximum play count for uncertainty calculation.
        /// </summary>
        /// <param name="adaptID">  Adapter ID</param>
        /// <param name="maxPlay">  Max play count</param>
        public void SetMaxPlay(string adaptID, double maxPlay) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.MaxPlay = maxPlay;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.MaxPlay = maxPlay;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetMaxPlay' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the maximum play count to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultMaxPlay(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultMaxPlay();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultMaxPlay();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultMaxPlay' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        #endregion Methods for uncertainty parameters

        #region Methods for K factor

        /// <summary>
        /// Get the K constant
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        /// <returns>K constant as double value</returns>
        public double GetKConst(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.KConst;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.KConst;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetKConst' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the K constant value
        /// </summary>
        /// <param name="adaptID">  Adapter ID</param>
        /// <param name="kConst">   The value of the K constant</param>
        public void SetKConst(string adaptID, double kConst) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.KConst = kConst;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.KConst = kConst;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetKConst' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the K constant to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultKConst(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultKConst();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultKConst();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultKConst' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Get the value of the upward uncertainty weight.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        /// <returns>Upward uncertainty weight as double value</returns>
        public double GetKUp(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.KUp;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.KUp;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetKUp' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the value for the upward uncertainty weight.
        /// </summary>
        /// <param name="adaptID">  Adapter ID</param>
        /// <param name="kUp">      Weight value</param>
        public void SetKUp(string adaptID, double kUp) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.KUp = kUp;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.KUp = kUp;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetKUp' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Set the upward uncertainty weight to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultKUp(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultKUp();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultKUp();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultKUp' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Get the value of the downward uncertainty weight.
        /// </summary>
        /// <param name="adaptID">  Adapter ID</param>
        /// <returns>Weight value as double number</returns>
        public double GetKDown(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                return this.adapter.KDown;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.KDown;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetKDown' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the value of the downward uncertainty weight.
        /// </summary>
        /// <param name="adaptID">  Adapter ID</param>
        /// <param name="kDown">    Weight value</param>
        public void SetKDown(string adaptID, double kDown) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.KDown = kDown;
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.KDown = kDown;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetKDown' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the downward uncertainty weight to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultKDown(string adaptID) {
            if (adaptID.Equals(this.adapter.Type)) {
                this.adapter.setDefaultKDown();
            }
            else if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultKDown();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultKDown' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        #endregion Methods for K factor

        #region Methods for Elo-based expected score params

        /// <summary>
        /// Get the value of the expected score magnifier
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        /// <returns>Magnifier as double value</returns>
        public double GetExpectScoreMagnifier(string adaptID) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.ExpectScoreMagnifier;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetExpectScoreMagnifier' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the value of the expected score magnifier.
        /// </summary>
        /// <param name="adaptID">              Adapter ID</param>
        /// <param name="expectScoreMagnifier"> The value for the magnifier</param>
        public void SetExpectScoreMagnifier(string adaptID, double expectScoreMagnifier) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.ExpectScoreMagnifier = expectScoreMagnifier;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetExpectScoreMagnifier' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the expected score magnifier to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultExpectScoreMagnifier(string adaptID) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultExpectScoreMagnifier();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultExpectScoreMagnifier' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Get the value of the magnifier step size.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        /// <returns>Magnifier step size as double value</returns>
        public double GetMagnifierStepSize(string adaptID) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                return this.adapterElo.MagnifierStepSize;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.GetMagnifierStepSize' method: adapter ID '{0}' is not recognized. Returning error code '{1}'."
                    , adaptID, BaseAdapter.ErrorCode));
                return BaseAdapter.ErrorCode;
            }
        }

        /// <summary>
        /// Set the value of teh magnifier step size.
        /// </summary>
        /// <param name="adaptID">              Adapter ID</param>
        /// <param name="magnifierStepSize">    The value of the magnifier step size</param>
        public void SetMagnifierStepSize(string adaptID, double magnifierStepSize) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.MagnifierStepSize = magnifierStepSize;
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetMagnifierStepSize' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        /// <summary>
        /// Sets the magnifier step size to its default value.
        /// </summary>
        /// <param name="adaptID">Adapter ID</param>
        public void SetDefaultMagnifierStepSize(string adaptID) {
            if (adaptID.Equals(this.adapterElo.Type)) {
                this.adapterElo.setDefaultMagnifierStepSize();
            }
            else {
                Log(AssetPackage.Severity.Error
                    , String.Format("In 'TwoA.SetDefaultMagnifierStepSize' method: adapter ID '{0}' is not recognized.", adaptID));
            }
        }

        #endregion Methods for Elo-based expected score params

        #endregion Methods for setting adapter parameters
        ////// END: methods for setting adapter parameters
        //////////////////////////////////////////////////////////////////////////////////////

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
                return player.LastPlayed;
            }
        }

        #endregion Player param getters

        #region Player param setters

        /// <summary>
        /// Set a Rating value for a player.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="rating">       The value of Rating. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool PlayerRating(string adaptID, string gameID, string playerID, double rating) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                Log(Severity.Error, String.Format("Unable to set Rating for player {0} for adaptation {1} in game {2}. Player not found."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            player.Rating = rating;
            return true;
        }

        /// <summary>
        /// Set a PlayCount value for a player.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="playCount">    The value of PlayCount. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool PlayerPlayCount(string adaptID, string gameID, string playerID, double playCount) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                Log(Severity.Error, String.Format("Unable to set PlayCount for player {0} for adaptation {1} in game {2}. Player not found."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            if (!IsValidPlayCount(playCount)) {
                Log(Severity.Error, String.Format("Unable to set PlayCount for player {0} for adaptation {1} in game {2}. Invalid play count."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            player.PlayCount = playCount;
            return true;
        }

        /// <summary>
        /// Set a KFactor value for a player.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="kFactor">      The value of KFactor. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool PlayerKFactor(string adaptID, string gameID, string playerID, double kFactor) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                Log(Severity.Error, String.Format("Unable to set KFactor for player {0} for adaptation {1} in game {2}. Player not found."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            if (!IsValidKFactor(kFactor)) {
                Log(Severity.Error, String.Format("Unable to set KFactor for player {0} for adaptation {1} in game {2}. Invalid K factor."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            player.KFactor = kFactor;
            return true;
        }

        /// <summary>
        /// Set an Uncertainty value for a player.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="uncertainty">  The value of Uncertainty. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool PlayerUncertainty(string adaptID, string gameID, string playerID, double uncertainty) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                Log(Severity.Error, String.Format("Unable to set Uncertainty for player {0} for adaptation {1} in game {2}. Player not found."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            if (!IsValidUncertainty(uncertainty)) {
                Log(Severity.Error, String.Format("Unable to set Uncertainty for player {0} for adaptation {1} in game {2}. Invalid uncertainty."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            player.Uncertainty = uncertainty;
            return true;
        }

        /// <summary>
        /// Set a LastPlayed datetime for a player.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="playerID">     Identifier for the player. </param>
        /// <param name="lastPlayed">   The DateTime object for LastPlayed datetime. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool PlayerLastPlayed(string adaptID, string gameID, string playerID, DateTime lastPlayed) {
            PlayerNode player = Player(adaptID, gameID, playerID);
            if (player == null) {
                Log(Severity.Error, String.Format("Unable to set LastPlayed for player {0} for adaptation {1} in game {2}. Player not found."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            if (lastPlayed == null) {
                Log(Severity.Error, String.Format("Unable to set LastPlayed for player {0} for adaptation {1} in game {2}. Null date object."
                                                                , playerID, adaptID, gameID));
                return false;
            }

            player.LastPlayed = lastPlayed;
            return true;
        }

        #endregion Player param setters

        #region PlayerNode adders

        /// <summary>
        /// Add a new player node.
        /// </summary>
        /// <param name="playerNode">New player node.</param>
        /// <returns>True if new player node was added and false otherwise.</returns>
        public bool AddPlayer(PlayerNode playerNode) {
            if (String.IsNullOrEmpty(playerNode.AdaptationID)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Adaptation ID is null or empty string.");
                return false;
            }

            if (String.IsNullOrEmpty(playerNode.GameID)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Game ID is null or empty string.");
                return false;
            }

            if (String.IsNullOrEmpty(playerNode.PlayerID)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Player ID is null or empty string.");
                return false;
            }

            if (Player(playerNode.AdaptationID, playerNode.GameID, playerNode.PlayerID) != null) {
                Log(Severity.Error, String.Format("In TwoA.AddPlayer: Cannot add player. Player '{0}' in game '{1}' with adaptation '{2}' already exists.",
                                                    playerNode.PlayerID, playerNode.GameID, playerNode.AdaptationID));
                return false;
            }

            if (!IsValidPlayCount(playerNode.PlayCount)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Invalid play count.");
                return false;
            }

            if (!IsValidKFactor(playerNode.KFactor)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Invalid K factor.");
                return false;
            }

            if (!IsValidUncertainty(playerNode.Uncertainty)) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Invalid uncertainty.");
                return false;
            }

            if (playerNode.LastPlayed == null) {
                Log(Severity.Error, "In TwoA.AddPlayer: Cannot add player. Null or empty string for last played date.");
                return false;
            }
            
            this.players.Add(playerNode);

            return true;
        }

        /// <summary>
        /// Add a new player node with custom parameters.
        /// </summary>
        /// <param name="adaptID">Adaptation ID.</param>
        /// <param name="gameID">Game ID.</param>
        /// <param name="playerID">Player ID.</param>
        /// <param name="rating">Player's skill rating.</param>
        /// <param name="playCount">The number of past games played by the player.</param>
        /// <param name="kFactor">Player's K factor.</param>
        /// <param name="uncertainty">Player's uncertainty.</param>
        /// <param name="lastPlayed">The datetime the player played the last game. Should have 'yyyy-MM-ddThh:mm:ss' format.</param>
        /// <returns>True if new player node was added and false otherwise.</returns>
        public bool AddPlayer(string adaptID, string gameID, string playerID
                            , double rating, double playCount, double kFactor, double uncertainty, DateTime lastPlayed) {
            return this.AddPlayer(new PlayerNode { 
                    AdaptationID = adaptID
                    , GameID = gameID
                    , PlayerID = playerID
                    , Rating = rating
                    , PlayCount = playCount
                    , KFactor = kFactor
                    , Uncertainty = uncertainty
                    , LastPlayed = lastPlayed
                }
            );
        }

        /// <summary>
        /// Add a new player node with default parameters.
        /// </summary>
        /// <param name="adaptID">Adaptation ID.</param>
        /// <param name="gameID">Game ID.</param>
        /// <param name="playerID">Player ID.</param>
        /// <returns>True if new player node was added and false otherwise.</returns>
        public bool AddPlayer(string adaptID, string gameID, string playerID) {
            return this.AddPlayer(adaptID, gameID, playerID
                                    , BaseAdapter.INITIAL_RATING, 0d, BaseAdapter.INITIAL_K_FCT, BaseAdapter.INITIAL_UNCERTAINTY
                                    , DateTime.ParseExact("2015-07-22T11:56:17", TwoA.DATE_FORMAT, null));
        }

        #endregion PlayerNode adders

        #region PlayerNode getter

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

            if (String.IsNullOrEmpty(adaptID) || String.IsNullOrEmpty(gameID) || String.IsNullOrEmpty(playerID)) {
                Log(Severity.Error, String.Format("In TwoA.Player method: one or more parameters are null. Expected string values. Returning null."));
                return null;
            }

            return players.Find(p => p.AdaptationID.Equals(adaptID) 
                                && p.GameID.Equals(gameID)
                                && p.PlayerID.Equals(playerID));
        }

        /// <summary>
        /// Gets a list of all player nodes.
        /// </summary>
        ///
        /// <param name="adaptID"> Identifier for the adapt. </param>
        /// <param name="gameID">  Identifier for the game. </param>
        ///
        /// <returns>
        /// List of PlayerNode instances.
        /// </returns>
        public List<PlayerNode> AllPlayers(string adaptID, string gameID) {
            if (String.IsNullOrEmpty(adaptID) || String.IsNullOrEmpty(gameID)) {
                Log(Severity.Error, String.Format("In TwoA.AllPlayers method: one or more parameters are null. Expected string values. Returning null."));
                return null;
            }

            List<PlayerNode> matchingPlayers = this.players.FindAll(p => p.AdaptationID.Equals(adaptID)
                                                                                && p.GameID.Equals(gameID));

            if (matchingPlayers == null || matchingPlayers.Count == 0) {
                Log(Severity.Error, String.Format("In TwoA.AllPlayers method: Unable to retrieve players for game {0} with adaptation {1}. No matching scenarios.", adaptID, gameID));
                return null;
            }

            return matchingPlayers.OrderBy(p => p.Rating).ToList<PlayerNode>();
        }

        #endregion PlayerNode getter

        #region PlayerNode removers

        /// <summary>
        /// Removes a specified player.
        /// </summary>
        /// <param name="adaptID">Adaptation ID</param>
        /// <param name="gameID">Game ID</param>
        /// <param name="playerID">Player ID</param>
        /// <returns>True if the player was removed, and false otherwise.</returns>
        public bool RemovePlayer(string adaptID, string gameID, string playerID) {
            return RemovePlayer(Player(adaptID, gameID, playerID));
        }

        /// <summary>
        /// Removes a specified player.
        /// </summary>
        /// <param name="playerNode">PlayerNode instance to remove.</param>
        /// <returns>True if player was removed, and false otherwise.</returns>
        public bool RemovePlayer(PlayerNode playerNode) {
            if (playerNode == null) {
                Log(Severity.Error, "In TwoA.RemovePlayer: Cannot remove player. The playerNode parameter is null.");
                return false;
            }

            return this.players.Remove(playerNode);
        }

        #endregion PlayerNode removers

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
                return scenario.LastPlayed;
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

        /// <summary>
        /// Set a Rating value for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="rating">       The value of Rating. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioRating(string adaptID, string gameID, string scenarioID, double rating) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set Rating for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            scenario.Rating = rating;
            return true;
        }

        /// <summary>
        /// Set a PlayCount value for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="playCount">    The value of PlayCount. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioPlayCount(string adaptID, string gameID, string scenarioID, double playCount) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set PlayCount for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            if (!IsValidPlayCount(playCount)) {
                Log(Severity.Error, String.Format("Unable to set PlayCount for scenario {0} for adaptation {1} in game {2}. Invalid play count."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            scenario.PlayCount = playCount;
            return true;
        }

        /// <summary>
        /// Set a KFactor value for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="kFactor">      The value of KFactor. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioKFactor(string adaptID, string gameID, string scenarioID, double kFactor) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set KFactor for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            if (!IsValidKFactor(kFactor)) {
                Log(Severity.Error, String.Format("Unable to set KFactor for scenario {0} for adaptation {1} in game {2}. Invalid K factor."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            scenario.KFactor = kFactor;
            return true;
        }

        /// <summary>
        /// Set an Uncertainty value for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="uncertainty">  The value of Uncertainty. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioUncertainty(string adaptID, string gameID, string scenarioID, double uncertainty) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set Uncertainty for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            if (!IsValidUncertainty(uncertainty)) {
                Log(Severity.Error, String.Format("Unable to set Uncertainty for scenario {0} for adaptation {1} in game {2}. Invalid uncertainty."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }
            
            scenario.Uncertainty = uncertainty;
            return true;
        }

        /// <summary>
        /// Set a LastPlayed datetime for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="lastPlayed">   The DateTime object for LastPlayed datetime. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioLastPlayed(string adaptID, string gameID, string scenarioID, DateTime lastPlayed) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set LastPlayed for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            if (lastPlayed == null) {
                Log(Severity.Error, String.Format("Unable to set LastPlayed for scenario {0} for adaptation {1} in game {2}. Null date object."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            scenario.LastPlayed = lastPlayed;
            return true;
        }

        /// <summary>
        /// Set a TimeLimit for a scenario.
        /// </summary>
        /// 
        /// <param name="adaptID">      Identifier for the adapt. </param>
        /// <param name="gameID">       Identifier for the game. </param>
        /// <param name="scenarioID">   Identifier for the scenario. </param>
        /// <param name="timeLimit">    The value of TimeLimit. </param>
        /// 
        /// <returns>
        /// True if value was changed, false otherwise.
        /// </returns>
        public bool ScenarioTimeLimit(string adaptID, string gameID, string scenarioID, double timeLimit) {
            ScenarioNode scenario = Scenario(adaptID, gameID, scenarioID);
            if (scenario == null) {
                Log(Severity.Error, String.Format("Unable to set TimeLimit for scenario {0} for adaptation {1} in game {2}. The scenario not found."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            if (!IsValidTimeLimit(timeLimit)) {
                Log(Severity.Error, String.Format("Unable to set TimeLimit for scenario {0} for adaptation {1} in game {2}. Invalid time limit."
                                                                , scenarioID, adaptID, gameID));
                return false;
            }

            scenario.TimeLimit = timeLimit;
            return true;
        }

        #endregion Scenario param setters

        #region ScenarioNode adders

        /// <summary>
        /// Add a new scenario node.
        /// </summary>
        /// <param name="scenarioNode">New scenario node.</param>
        /// <returns>True if new scenario node was added and false otherwise.</returns>
        public bool AddScenario(ScenarioNode scenarioNode) {
            if (String.IsNullOrEmpty(scenarioNode.AdaptationID)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Adaptation ID is null or empty string.");
                return false;
            }

            if (String.IsNullOrEmpty(scenarioNode.GameID)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Game ID is null or empty string.");
                return false;
            }

            if (String.IsNullOrEmpty(scenarioNode.ScenarioID)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Scenario ID is null or empty string.");
                return false;
            }

            if (Scenario(scenarioNode.AdaptationID, scenarioNode.GameID, scenarioNode.ScenarioID) != null) {
                Log(Severity.Error, String.Format("In TwoA.AddScenario: Cannot add scenario. Scenario '{0}' in game '{1}' with adaptation '{2}' already exists.",
                                                    scenarioNode.ScenarioID, scenarioNode.GameID, scenarioNode.AdaptationID));
                return false;
            }

            if (!IsValidPlayCount(scenarioNode.PlayCount)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Invalid play count.");
                return false;
            }

            if (!IsValidKFactor(scenarioNode.KFactor)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Invalid K factor.");
                return false;
            }

            if (!IsValidUncertainty(scenarioNode.Uncertainty)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Invalid uncertainty.");
                return false;
            }

            if (scenarioNode.LastPlayed == null) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Null or empty string for last played date.");
                return false;
            }

            if (!IsValidTimeLimit(scenarioNode.TimeLimit)) {
                Log(Severity.Error, "In TwoA.AddScenario: Cannot add scenario. Invalid time limit.");
                return false;
            }

            this.scenarios.Add(scenarioNode);

            return true;
        }

        /// <summary>
        /// Add a new scenario node with custom parameters.
        /// </summary>
        /// <param name="adaptID">Adaptation ID.</param>
        /// <param name="gameID">Game ID.</param>
        /// <param name="scenarioID">Scenario ID.</param>
        /// <param name="rating">Scenario's difficulty rating.</param>
        /// <param name="playCount">The number of time the scenario was played to calculate the difficulty rating.</param>
        /// <param name="kFactor">Scenario's K factor.</param>
        /// <param name="uncertainty">Scenario's uncertainty.</param>
        /// <param name="lastPlayed">The datetime the scenario was last played. Should have 'yyyy-MM-ddThh:mm:ss' format.</param>
        /// <param name="timeLimit">Time limit to complete the scenario (in milliseconds).</param>
        /// <returns>True if new scenario node was added and false otherwise.</returns>
        public bool AddScenario(string adaptID, string gameID, string scenarioID
                            , double rating, double playCount, double kFactor, double uncertainty, DateTime lastPlayed, double timeLimit) {
            return this.AddScenario(new ScenarioNode {
                    AdaptationID = adaptID,
                    GameID = gameID,
                    ScenarioID = scenarioID,
                    Rating = rating,
                    PlayCount = playCount,
                    KFactor = kFactor,
                    Uncertainty = uncertainty,
                    LastPlayed = lastPlayed,
                    TimeLimit = timeLimit
                }
            );
        }

        /// <summary>
        /// Add a new scenario node with default parameters.
        /// </summary>
        /// <param name="adaptID">Adaptation ID.</param>
        /// <param name="gameID">Game ID.</param>
        /// <param name="scenarioID">Scenario ID.</param>
        /// <returns>True if new scenario node was added and false otherwise.</returns>
        public bool AddScenario(string adaptID, string gameID, string scenarioID) {
            return this.AddScenario(adaptID, gameID, scenarioID
                                    , BaseAdapter.INITIAL_RATING, 0d, BaseAdapter.INITIAL_K_FCT, BaseAdapter.INITIAL_UNCERTAINTY
                                    , DateTime.ParseExact("2015-07-22T11:56:17", TwoA.DATE_FORMAT, null), BaseAdapter.DEFAULT_TIME_LIMIT);
        }

        #endregion ScenarioNode adders

        #region ScenarioNode getter

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
            if (String.IsNullOrEmpty(adaptID) || String.IsNullOrEmpty(gameID) || String.IsNullOrEmpty(scenarioID)) {
                Log(Severity.Error, String.Format("In TwoA.Scenario method: one or more parameters are null. Expected string values. Returning null."));
                return null;
            }

            return this.scenarios.Find(p => p.AdaptationID.Equals(adaptID)
                                                        && p.GameID.Equals(gameID)
                                                        && p.ScenarioID.Equals(scenarioID));
        }

        /// <summary>
        /// Gets a list of all scenario nodes.
        /// </summary>
        ///
        /// <param name="adaptID"> Identifier for the adapt. </param>
        /// <param name="gameID">  Identifier for the game. </param>
        ///
        /// <returns>
        /// all scenarios.
        /// </returns>
        public List<ScenarioNode> AllScenarios(string adaptID, string gameID) {
            if (String.IsNullOrEmpty(adaptID) || String.IsNullOrEmpty(gameID)) {
                Log(Severity.Error, String.Format("In AllScenarios method: one or more parameters are null. Expected string values. Returning null."));
                return null;
            }

            List<ScenarioNode> matchingScenarios = this.scenarios.FindAll(p => p.AdaptationID.Equals(adaptID)
                                                                                && p.GameID.Equals(gameID));

            if (matchingScenarios == null || matchingScenarios.Count == 0) {
                Log(Severity.Error, String.Format("Unable to retrieve scenario for game {0} with adaptation {1}. No matching scenarios.", adaptID, gameID));
                return null;
            }

            //return matchingScenarios.OrderBy(p => p.Rating).Select(p => p.ScenarioID).ToList<string>()
            return matchingScenarios.OrderBy(p => p.Rating).ToList<ScenarioNode>();
        }

        #endregion ScenarioNode getter

        #region ScenarioNode removers

        /// <summary>
        /// Removes a specified scenario.
        /// </summary>
        /// <param name="adaptID">Adaptation ID</param>
        /// <param name="gameID">Game ID</param>
        /// <param name="scenarioID">Scenario ID</param>
        /// <returns>True if scenario was removed, and false otherwise.</returns>
        public bool RemoveScenario(string adaptID, string gameID, string scenarioID) {
            return RemoveScenario(Scenario(adaptID, gameID, scenarioID));
        }

        /// <summary>
        /// Removes a specified scenario.
        /// </summary>
        /// <param name="scenarioNode">ScenarioNode instance to remove.</param>
        /// <returns>True if scenario was removed, and false otherwise.</returns>
        public bool RemoveScenario(ScenarioNode scenarioNode) {
            if (scenarioNode == null) {
                Log(Severity.Error, "In TwoA.RemoveScenario: Cannot remove scenario. The scenarioNode parameter is null.");
                return false;
            }

            return this.scenarios.Remove(scenarioNode);
        }

        #endregion ScenarioNode removers

        ////// END: methods for scenario data
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for value validity checks
        #region Methods for value validity checks

        /// <summary>
        /// Returns true if play count value is valid.
        /// </summary>
        /// <param name="playCount">Play count value</param>
        /// <returns>bool</returns>
        public bool IsValidPlayCount(double playCount) {
            if (playCount < 0) {
                Log(Severity.Information, String.Format("Play count should be equal to or higher than 0."));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if K factor value is valid.
        /// </summary>
        /// <param name="kFactor">K factor value</param>
        /// <returns>bool</returns>
        public bool IsValidKFactor(double kFactor) {
            if (kFactor <= 0) {
                Log(Severity.Information, String.Format("K factor should be equal to or higher than {0}.", BaseAdapter.MIN_K_FCT));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if uncertainty value is valid.
        /// </summary>
        /// <param name="uncertainty">Uncertainty value</param>
        /// <returns>bool</returns>
        public bool IsValidUncertainty(double uncertainty) {
            if (uncertainty < 0 || uncertainty > 1) {
                Log(Severity.Information, String.Format("The uncertainty should be within [0, 1]."));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if time limit value is valid.
        /// </summary>
        /// <param name="timeLimit">Time limit value</param>
        /// <returns>bool</returns>
        public bool IsValidTimeLimit(double timeLimit) {
            if (timeLimit <= 0) {
                Log(Severity.Error, String.Format("Time limit should be higher than 0."));
                return false;
            }

            return true;
        }

        #endregion Methods for value validity checks
        ////// END: methods for value validity checks
        //////////////////////////////////////////////////////////////////////////////////////

        #endregion Methods
    }
}