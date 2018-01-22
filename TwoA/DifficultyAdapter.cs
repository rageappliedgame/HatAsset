#region Header

/*
Copyright 2018 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/), Wim van der Vegt

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Namespace: TwoANS
Filename: DifficultyAdapter.cs
Description:
    The asset implements ELO-based difficulty to skill adaptation algorithm described in "Klinkenberg, S., Straatemeier, M., & Van der Maas, H. L. J. (2011).
    Computer adaptive practice of maths ability using a new item response model for on the fly ability and difficulty estimation.
    Computers & Education, 57 (2), 1813-1824.".
*/


// Change history:
// [2016.03.14]
//      - [SC] changed calcTargetBeta method
//      - [SC] changed calcExpectedScore method to prevent division by 0
//      - [SC] corrected ProvU property
// [2016.03.15]
//      - [SC] added ADAPTER_TYPE field
//      - [SC] added property for ADAPTER_TYPE field
//      - [SC] replaced all adaptID parameters in TargetScenarioID method with ADAPTER_TYPE field
//      - [SC] replaced all adaptID parameters in UpdateRatings method with ADAPTER_TYPE field
//      - [SC] changed UpdateRatings method
// [2016.03.15]
//      - [SC] added 'updateDatafiles' parameter to the updateDatafiles method's signature
//      - [SC] the design document was updated to reflect changes in the source code. Refer to https://rage.ou.nl/index.php?q=filedepot_download/358/501
// [2016.10.06]
//      - [SC] renamed namespace 'HAT' to 'TwoA'
// [2016.10.07]
//      - [SC] from the constructor, moved instantiation of SimpleRNG to 'TwoA.InitSettings' method
//      - [SC] in calcTargetBeta method, changed equation from 'theta + Math.Log(randomNum / (1 - randomNum))' to 'theta + Math.Log((1 - randomNum) / randomNum)'
// [2016.11.14]
//      - [SC] deleted 'using Swiss';
// [2016.11.29]
//      - [SC] added 'SimpleRNG.SetSeedFromSystemTime()' to 'calcTargetBeta' method;
// [2016.12.07]
//      - [SC] added replaced 'calcTargetBeta' method with 'calcTargetBetas' method that calculates a range for a target beta rather than single beta value
//      - [SC] changed the approach used by TargetScenarioID method to decide on the recommended scenario
// [2016.12.14]
//      - [SC] updated 'calcTargetBetas' to calculate a fuzzy interval consisting of four rating values
// [2016.12.15]
//      - [SC] changed TargetScenarioID method to use a fuzzy interval to decide on the recommended scenario
// [2017.01.03]
//      - [SC] added 'validateCorrectAnswer' method
//      - [SC] modified body of 'calcActualScore' method
//      - [SC] changed 'calcActualScore' method to internal instead of private
//      - [SC] added setter to 'ProvDate' property
//      - [SC] renamed property 'TargetDistributionMean' to 'TargetDistrMean'
// [2017.01.04]
//      - [SC] modified 'TargetDistrMean' property
//      - [SC] added 'TargetLowerLimit' property
//      - [SC] added 'TargetUpperLimit' property
//      - [SC] added 'TargetDistrSD' property
//      - [SC] added 'FiSDMultiplier' property
//      - [SC] modified 'calcTargetBetas' method
// [2017.01.05]
//      - [SC] modified 'DifficultyAdapter(TwoA asset)' contructor
//      - [SC] added 'setDefaultTargetDistribution' method
//      - [SC] added 'setTargetDistribution' method
//      - [SC] modified properties MaxDelay, MaxPlay, and ProvU
//      - [SC] removed exception throws from 'UpdateRatings'
//      - [SC] removed exception throws from 'TargetScenarioID'
//      - [SC] removed exception throws from validateCorrectAnswer, validateResponseTime, validateItemMaxDuration
// [2017.01.12]
//      - [SC] added 'log(Severity severity, string msg)' and 'log(string msg)' methods
// [2017.02.09]
//      - [SC] changed UpdateRatings method body and signature. Scenario parameter updates are optional.
// [2017.02.13]
//      - [SC] Some code shared with other adapters was transfered to BaseAdapter class
//      - [SC] DifficultyAdapter class extends BaseAdapter class
//      - [SC] added 'Description' property
//      - [SC] changed access modifiers for all properties and methods to 'internal'
// [2017.02.16]
//      - [SC] added 'setDefaultFiSDMultiplier' method
//      - [SC] added 'setDefaultMaxDelay' and 'setDefaultMaxPlay' methods
//      - [SC] commented provU and provDate since they are not used anywhere
//      - [SC] added 'setDefaultKConst', 'setDefaultKUp', 'setDefaultKDown'
// [2017.12.19]
//      - [SC] changed the namespace from TwoA to TwoANS
//      - [SC] changed the access modifier for DifficultyAdapter class to public from internal
//      - [SC] changed the access modifiers for Type and Description properties to public from internal
// [2017.12.20]
//      - [SC] changed the Type property to static
//      - [SC] changed the Description property to static
// [2018.01.17]
//      - [SC] added a calibration phase that affect calculation of the K factors to speed up rating convergence during first few games
//      - [SC] added a region "const, fields, and properties for the calibration phase"
//      - [SC] modified calcThetaKFctr and calcBetaKFctr methods
//

//
// [TODO]: 
//      - transaction style update of player and scenario data; no partial update should be possible; if any value fails to update then all values should fail to update
//      - provU and provDate are not used anywhere yet

#endregion Header

namespace TwoANS
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using AssetPackage;

    /// <summary>
    /// Performs assessment and adaptation based on player's response time and accuracy. 
    /// </summary>
    public class DifficultyAdapter : BaseAdapter 
    {
        #region Consts, Fields, Properties

        /// <summary>
        /// Gets the type of the adapter
        /// </summary>
        public new static string Type {
            get { return "Game difficulty - Player skill"; }
        }

        /// <summary>
        /// Description of this adapter
        /// </summary>
        public new static string Description {
            get {
                return "Adapts game difficulty to player skill. Skill ratings are evaluated for individual players. "
                + "Requires player accuracy (0 or 1) and response time. Uses a modified version of the CAP algorithm.";
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: const, fields, and properties for calculating target betas
        #region const, fields, and properties for calculating target betas

        private const double TARGET_DISTR_MEAN = 0.75;      // [SC] default value for 'targetDistrMean' field
        private const double TARGET_DISTR_SD = 0.1;         // [SC] default value for 'targetDistrSD' field
        private const double TARGET_LOWER_LIMIT = 0.50;     // [SC] default value for 'targetLowerLimit' field
        private const double TARGET_UPPER_LIMIT = 1.0;      // [SC] default value for 'targetUpperLimit' field

        private const double FI_SD_MULTIPLIER = 1.0;        // [SC] multipler for SD used to calculate the means of normal distributions used to decide on lower and upper bounds of the supports in a fuzzy interval

        private double targetDistrMean = DifficultyAdapter.TARGET_DISTR_MEAN;
        private double targetDistrSD = DifficultyAdapter.TARGET_DISTR_SD;
        private double targetLowerLimit = DifficultyAdapter.TARGET_LOWER_LIMIT;
        private double targetUpperLimit = DifficultyAdapter.TARGET_UPPER_LIMIT;

        private double fiSDMultiplier = DifficultyAdapter.FI_SD_MULTIPLIER;

        /// <summary>
        /// Getter for target distribution mean. See 'setTargetDistribution' method for setting a value.
        /// </summary>
        internal double TargetDistrMean {
            get { return this.targetDistrMean; }
            private set { this.targetDistrMean = value; }
        }

        /// <summary>
        /// Getter for target distribution standard deviation. See 'setTargetDistribution' method for setting a value.
        /// </summary>
        internal double TargetDistrSD {
            get { return this.targetDistrSD; }
            private set { this.targetDistrSD = value; }
        }

        /// <summary>
        /// Getter for target distribution lower limit. See 'setTargetDistribution' method for setting a value.
        /// </summary>
        internal double TargetLowerLimit {
            get { return this.targetLowerLimit; }
            private set { this.targetLowerLimit = value; }
        }

        /// <summary>
        /// Getter for target distribution upper limit. See 'setTargetDistribution' method for setting a value.
        /// </summary>
        internal double TargetUpperLimit {
            get { return this.targetUpperLimit; }
            private set { this.targetUpperLimit = value; }
        }

        /// <summary>
        /// Getter/setter for a weight used to calculate distribution means for a fuzzy selection algorithm.
        /// </summary>
        internal double FiSDMultiplier {
            get { return this.fiSDMultiplier; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In FiSDMultiplier: The standard deviation multiplier '{0}' is less than or equal to 0.", value));
                }
                else {
                    this.fiSDMultiplier = value;
                }
            }
        }

        /// <summary>
        /// Sets FiSDMultiplier to a default value
        /// </summary>
        internal void setDefaultFiSDMultiplier() {
            this.FiSDMultiplier = DifficultyAdapter.FI_SD_MULTIPLIER;
        }

        /// <summary>
        /// Sets target distribution parameters to their default values.
        /// </summary>
        internal void setDefaultTargetDistribution() {
            setTargetDistribution(DifficultyAdapter.TARGET_DISTR_MEAN, DifficultyAdapter.TARGET_DISTR_SD,
                                    DifficultyAdapter.TARGET_LOWER_LIMIT, DifficultyAdapter.TARGET_UPPER_LIMIT);
        }

        // [TEST]
        /// <summary>
        /// Sets target distribution parameters to custom values.
        /// </summary>
        /// 
        /// <param name="tDistrMean">   Dstribution mean</param>
        /// <param name="tDistrSD">     Distribution standard deviation</param>
        /// <param name="tLowerLimit">  Distribution lower limit</param>
        /// <param name="tUpperLimit">  Distribution upper limit</param>
        internal void setTargetDistribution(double tDistrMean, double tDistrSD, double tLowerLimit, double tUpperLimit) {
            bool validValuesFlag = true;

            // [SD] setting distribution mean
            if (tDistrMean <= 0 || tDistrMean >= 1) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The target distribution mean '{0}' is not within the open interval (0, 1).", tDistrMean));

                validValuesFlag = false;
            }

            // [SC] setting distribution SD
            if (tDistrSD <= 0 || tDistrSD >= 1) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The target distribution standard deviation '{0}' is not within the open interval (0, 1).", tDistrSD));

                validValuesFlag = false;
            }

            // [SC] setting distribution lower limit
            if (tLowerLimit < 0 || tLowerLimit > 1) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The lower limit of distribution '{0}' is not within the closed interval [0, 1].", tLowerLimit));

                validValuesFlag = false;
            }
            if (tLowerLimit >= tDistrMean) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The lower limit of distribution '{0}' is bigger than or equal to the mean of the distribution '{1}'."
                        , tLowerLimit, tDistrMean));

                validValuesFlag = false;
            }

            // [SC] setting distribution upper limit
            if (tUpperLimit < 0 || tUpperLimit > 1) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The upper limit of distribution '{0}' is not within the closed interval [0, 1].", tUpperLimit));

                validValuesFlag = false;
            }
            if (tUpperLimit <= tDistrMean) {
                log(AssetPackage.Severity.Warning,
                    String.Format("In DifficultyAdapter.setTargetDistribution: The upper limit of distribution '{0}' is less than or equal to the mean of the distribution {1}."
                        , tUpperLimit, tDistrMean));

                validValuesFlag = false;
            }

            if (validValuesFlag) {
                this.TargetDistrMean = tDistrMean;
                this.TargetDistrSD = tDistrSD;
                this.TargetLowerLimit = tLowerLimit;
                this.TargetUpperLimit = tUpperLimit;
            }
            else {
                log(AssetPackage.Severity.Warning, String.Format("In DifficultyAdapter.setTargetDistribution: Invalid value combination is found."));
            }
        }

        #endregion const, fields, and properties for calculating target betas
        ////// END: const, fields, and properties for calculating target betas
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: const, fields, and properties for calculating rating uncertainties
        #region const, fields, and properties for calculating rating uncertainties

        private const double DEF_MAX_DELAY = 30;                // [SC] The default value for the max number of days after which player's or item's undertainty reaches the maximum
        private const double DEF_MAX_PLAY = 40;                 // [SC] The default value for the max number of administrations that should result in minimum uncertaint in item's or player's ratings

        private double maxDelay = DifficultyAdapter.DEF_MAX_DELAY;        // [SC] set to DEF_MAX_DELAY in the constructor
        private double maxPlay = DifficultyAdapter.DEF_MAX_PLAY;         // [SC] set to DEF_MAX_PLAY in the constructor

        /// <summary>
        /// Gets or sets the maximum delay.
        /// </summary>
        internal double MaxDelay {
            get { return this.maxDelay; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.MaxDelay: The maximum number of delay days '{0}' should be higher than 0.", value));
                }
                else {
                    this.maxDelay = value;
                }
            }
        }

        /// <summary>
        /// Sets MaxDelay to its default value.
        /// </summary>
        internal void setDefaultMaxDelay() {
            this.MaxDelay = DifficultyAdapter.DEF_MAX_DELAY;
        }

        /// <summary>
        /// Gets or sets the maximum play.
        /// </summary>
        internal double MaxPlay {
            get { return this.maxPlay; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.MaxPlay: The maximum administration parameter '{0}' should be higher than 0.", value));
                }
                else {
                    this.maxPlay = value;
                }
            }
        }

        /// <summary>
        /// Sets MaxPlay to its default value
        /// </summary>
        internal void setDefaultMaxPlay() {
            this.MaxPlay = DifficultyAdapter.DEF_MAX_PLAY;
        }

        #endregion const, fields, and properties for calculating rating uncertainties
        ////// END: const, fields, and properties for calculating rating uncertainties
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: const, fields, and properties for calculating k factors
        #region const, fields, and properties for calculating k factors

        private const double DEF_K = 0.0075;    // [SC] The default value for the K constant when there is no uncertainty
        private const double DEF_K_UP = 4.0;    // [SC] the default value for the upward uncertainty weight
        private const double DEF_K_DOWN = 0.5;  // [SC] The default value for the downward uncertainty weight

        private double kConst = DifficultyAdapter.DEF_K;          // [SC] set to DEF_K in the constructor
        private double kUp = DifficultyAdapter.DEF_K_UP;             // [SC] set to DEF_K_UP in the constructor
        private double kDown = DifficultyAdapter.DEF_K_DOWN;           // [SC] set to DEF_K_DOWN in the constructor

        /// <summary>
        /// Getter/setter for the K constant.
        /// </summary>
        internal double KConst {
            get { return this.kConst; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning
                        , String.Format("In DifficultyAdapter.KConst: K constant '{0}' cannot be 0 or a negative number.", value));
                }
                else {
                    this.kConst = value;
                }
            }
        }

        /// <summary>
        /// Sets the K constant to its deafult value
        /// </summary>
        internal void setDefaultKConst() {
            this.KConst = DifficultyAdapter.DEF_K;
        }

        /// <summary>
        /// Getter/setter for the upward uncertainty weight.
        /// </summary>
        internal double KUp {
            get { return this.kUp; }
            set {
                if (value < 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.KUp: The upward uncertianty weight '{0}' cannot be a negative number.", value));
                }
                else {
                    this.kUp = value;
                }
            }
        }

        /// <summary>
        /// Sets the upward uncertainty weight to its default value.
        /// </summary>
        internal void setDefaultKUp() {
            this.KUp = DifficultyAdapter.DEF_K_UP;
        }

        /// <summary>
        /// Getter/setter for the downward uncertainty weight.
        /// </summary>
        internal double KDown {
            get { return this.kDown; }
            set {
                if (value < 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.KDown: The downward uncertainty weight '{0}' cannot be a negative number.", value));
                }
                else {
                    this.kDown = value;
                }
            }
        }

        /// <summary>
        /// Sets the downward uncetrtainty weight to its default value.
        /// </summary>
        internal void setDefaultKDown() {
            this.KDown = DifficultyAdapter.DEF_K_DOWN;
        }

        #endregion const, fields, and properties for calculating k factors
        ////// END: properties for calculating k factors
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: const, fields, and properties for the calibration phase
        #region const, fields, and properties for the calibration phase

        // [SC] The default value for the length (number of gameplays) of player's calibration
        private const int DEF_PLAYER_CAL_LENGTH = 30;
        // [SC] The default value for the length (number of gameplays) of scenario's calibration
        private const int DEF_SCENARIO_CAL_LENGTH = 30;
        // [SC] The default K factor for player's calibration
        private const double DEF_PLAYER_CAL_K = 0.1;
        // [SC] The default K factor for scenario's calibration
        private const double DEF_SCENARIO_CAL_K = 0.1;

        private int playerCalLength = DifficultyAdapter.DEF_PLAYER_CAL_LENGTH;
        private int scenarioCalLength = DifficultyAdapter.DEF_SCENARIO_CAL_LENGTH;
        private double playerCalK = DifficultyAdapter.DEF_PLAYER_CAL_K;
        private double scenarioCalK = DifficultyAdapter.DEF_SCENARIO_CAL_K;

        /// <summary>
        /// Gets or sets the player's calibration length.
        /// </summary>
        internal int PlayerCalLength {
            get { return this.playerCalLength; }
            set {
                if (value < 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.PlayerCalLength: The calibration length '{0}' should be equal to or higher than 0.", value));
                }
                else {
                    this.playerCalLength = value;
                }
            }
        }

        /// <summary>
        /// Sets player calibration length to its default value.
        /// </summary>
        internal void setDefaultPlayerCalLength() {
            this.PlayerCalLength = DifficultyAdapter.DEF_PLAYER_CAL_LENGTH;
        }

        /// <summary>
        /// Gets or sets the scenario's calibration length.
        /// </summary>
        internal int ScenarioCalLength {
            get { return this.scenarioCalLength; }
            set {
                if (value < 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.ScenarioCalLength: The calibration length '{0}' should be equal to or higher than 0.", value));
                }
                else {
                    this.scenarioCalLength = value;
                }
            }
        }

        /// <summary>
        /// Sets scenario calibration length to its default value.
        /// </summary>
        internal void setDefaultScenarioCalLength() {
            this.ScenarioCalLength = DifficultyAdapter.DEF_SCENARIO_CAL_LENGTH;
        }

        /// <summary>
        /// Sets the scenario and player calibration length to the same value
        /// </summary>
        internal int CalLength {
            set {
                if (value < 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.CalLength: The calibration length '{0}' should be equal to or higher than 0.", value));
                }
                else {
                    this.playerCalLength = value;
                    this.scenarioCalLength = value;
                }
            }
        }

        /// <summary>
        /// Sets scenario and player calibration lengths to its default values.
        /// </summary>
        internal void setDefaultCalLength() {
            this.PlayerCalLength = DifficultyAdapter.DEF_PLAYER_CAL_LENGTH;
            this.ScenarioCalLength = DifficultyAdapter.DEF_SCENARIO_CAL_LENGTH;
        }

        /// <summary>
        /// Gets or sets the player calibration K factor.
        /// </summary>
        internal double PlayerCalK {
            get { return this.playerCalK; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.PlayerCalK: The calibration K factor '{0}' cannot be 0 or a negative number.", value));
                }
                else {
                    this.playerCalK = value;
                }
            }
        }

        /// <summary>
        /// Sets player calibration K factor to its default value.
        /// </summary>
        internal void setDefaultPlayerCalK() {
            this.PlayerCalK = DifficultyAdapter.DEF_PLAYER_CAL_K;
        }

        /// <summary>
        /// Gets or sets the scenario calibration K factor.
        /// </summary>
        internal double ScenarioCalK {
            get { return this.scenarioCalK; }
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.ScenarioCalK: The calibration K factor '{0}' cannot be 0 or a negative number.", value));
                }
                else {
                    this.scenarioCalK = value;
                }
            }
        }

        /// <summary>
        /// Sets scenario calibration K factor to its default value.
        /// </summary>
        internal void setDefaultScenarioCalK() {
            this.ScenarioCalK = DifficultyAdapter.DEF_SCENARIO_CAL_K;
        }

        /// <summary>
        /// Sets the player and scenario calibration K factors to the same value.
        /// </summary>
        internal double CalK {
            set {
                if (value <= 0) {
                    log(AssetPackage.Severity.Warning,
                        String.Format("In DifficultyAdapter.CalK: The calibration K factor '{0}' cannot be 0 or a negative number.", value));
                }
                else {
                    this.playerCalK = value;
                    this.scenarioCalK = value;
                }
            }
        }

        /// <summary>
        /// Sets scenario and player calibration K factors to its default values.
        /// </summary>
        internal void setDefaultCalK() {
            this.PlayerCalK = DifficultyAdapter.DEF_PLAYER_CAL_K;
            this.ScenarioCalK = DifficultyAdapter.DEF_SCENARIO_CAL_K;
        }

        #endregion const, fields, and properties for the calibration phase
        ////// END: const, fields, and properties for the calibration phase
        //////////////////////////////////////////////////////////////////////////////////////

        #endregion Consts, Fields, Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TwoA.DifficultyAdapter class.
        /// </summary>
        ///
        /// <param name="asset"> The asset. </param>
        internal DifficultyAdapter(TwoA asset)
            : base(asset) {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Updates the ratings.
        /// </summary>
        /// <param name="playerNode">               Player node to be updated. </param>
        /// <param name="scenarioNode">             Scenario node to be updated. </param>
        /// <param name="rt">                       Player's response time. </param>
        /// <param name="correctAnswer">            Player's accuracy. </param>
        /// <param name="updateScenarioRating">     Set to false to avoid updating scenario node. </param>
        /// <param name="customPlayerKfct">         If non-0 value is provided then it is used as a weight to scale change in player's rating. Otherwise, adapter calculates its own K factor. </param>
        /// <param name="customScenarioKfct">       If non-0 value is provided then it is used as a weight to scale change in scenario's rating. Otherwise, adapter calculates its own K factor. </param>
        /// <returns>True if updates are successfull, and false otherwise.</returns>
        internal bool UpdateRatings(PlayerNode playerNode, ScenarioNode scenarioNode
                                        , double rt, double correctAnswer, bool updateScenarioRating
                                        , double customPlayerKfct, double customScenarioKfct) {
            if (this.asset == null) {
                log(AssetPackage.Severity.Error, "In DifficultyAdapter.UpdateRatings: Unable to update ratings. Asset instance is not detected.");
                return false;
            }

            if (playerNode == null) {
                log(AssetPackage.Severity.Error, String.Format("In DifficultyAdapter.UpdateRatings: Null player node."));
                return false;
            }

            if (scenarioNode == null) {
                log(AssetPackage.Severity.Error, String.Format("In DifficultyAdapter.UpdateRatings: Null scenario node."));
                return false;
            }

            if (!(validateCorrectAnswer(correctAnswer) && validateResponseTime(rt))) {
                log(AssetPackage.Severity.Error, "In DifficultyAdapter.UpdateRatings: Unable to update ratings. Invalid response time and/or accuracy detected.");
                return false;
            }

            // [TODO] should check for valid adaptation IDs in the player and scenarios?

            // [SC] getting player data
            double playerRating = playerNode.Rating;
            double playerPlayCount = playerNode.PlayCount;
            double playerUncertainty = playerNode.Uncertainty;
            DateTime playerLastPlayed = playerNode.LastPlayed;

            // [SC] getting scenario data
            double scenarioRating = scenarioNode.Rating;
            double scenarioPlayCount = scenarioNode.PlayCount;
            double scenarioUncertainty = scenarioNode.Uncertainty;
            double scenarioTimeLimit = scenarioNode.TimeLimit;
            DateTime scenarioLastPlayed = scenarioNode.LastPlayed;

            // [SC] current datetime
            DateTime currDateTime = DateTime.UtcNow;

            // [SC] parsing player data
            double playerLastPlayedDays = (currDateTime - playerLastPlayed).Days;
            if (playerLastPlayedDays > this.MaxDelay) {
                playerLastPlayedDays = this.MaxDelay;
            }

            // [SC] parsing scenario data
            double scenarioLastPlayedDays = (currDateTime - scenarioLastPlayed).Days;
            if (scenarioLastPlayedDays > this.MaxDelay) {
                scenarioLastPlayedDays = this.MaxDelay;
            }

            // [SC] calculating actual and expected scores
            double actualScore = calcActualScore(correctAnswer, rt, scenarioTimeLimit);
            double expectScore = calcExpectedScore(playerRating, scenarioRating, scenarioTimeLimit);

            // [SC] calculating player and scenario uncertainties
            double playerNewUncertainty = calcThetaUncertainty(playerUncertainty, playerLastPlayedDays);
            double scenarioNewUncertainty = calcBetaUncertainty(scenarioUncertainty, scenarioLastPlayedDays);

            double playerNewKFct;
            double scenarioNewKFct;

            if (customPlayerKfct > 0) {
                playerNewKFct = customPlayerKfct;
            }
            else {
                // [SC] calculating player K factors
                playerNewKFct = calcThetaKFctr(playerNewUncertainty, scenarioNewUncertainty, playerPlayCount);
            }

            if (customScenarioKfct > 0) {
                scenarioNewKFct = customScenarioKfct;
            }
            else {
                // [SC] calculating scenario K factor
                scenarioNewKFct = calcBetaKFctr(playerNewUncertainty, scenarioNewUncertainty, scenarioPlayCount);
            }

            // [SC] calculating player and scenario ratings
            double playerNewRating = calcTheta(playerRating, playerNewKFct, actualScore, expectScore);
            double scenarioNewRating = calcBeta(scenarioRating, scenarioNewKFct, actualScore, expectScore);

            // [SC] updating player and scenario play counts
            double playerNewPlayCount = playerPlayCount + 1.0d;
            double scenarioNewPlayCount = scenarioPlayCount + 1.0d;

            // [SC] storing updated player data
            playerNode.Rating = playerNewRating;
            playerNode.PlayCount = playerNewPlayCount;
            playerNode.KFactor = playerNewKFct;
            playerNode.Uncertainty = playerNewUncertainty;
            playerNode.LastPlayed = currDateTime;

            // [SC] storing updated scenario data
            if (updateScenarioRating) {
                scenarioNode.Rating = scenarioNewRating;
                scenarioNode.PlayCount = scenarioNewPlayCount;
                scenarioNode.KFactor = scenarioNewKFct;
                scenarioNode.Uncertainty = scenarioNewUncertainty;
                scenarioNode.LastPlayed = currDateTime;
            }

            // [SC] creating game log
            this.asset.CreateNewRecord(DifficultyAdapter.Type, playerNode.GameID, playerNode.PlayerID, scenarioNode.ScenarioID
                , rt, correctAnswer, playerNewRating, scenarioNewRating, currDateTime);

            return true;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: functions for calculating matching scenario
        #region functions for calculating matching scenario

        /// <summary>
        /// Calculates expected beta for target scenario. Returns ScenarioNode object of a scenario with beta closest to the target beta.
        /// If two more scenarios match then scenario that was least played is chosen.  
        /// </summary>
        ///
        /// <param name="playerNode">       Player node containing player parameters. </param>
        /// <param name="scenarioList">     A list of scenarios from which the target scenario is chosen. </param>
        ///
        /// <returns>
        /// ScenarioNode instance.
        /// </returns>
        internal ScenarioNode TargetScenario(PlayerNode playerNode, List<ScenarioNode> scenarioList) {
            if (this.asset == null) {
                log(AssetPackage.Severity.Error, "In DifficultyAdapter.TargetScenario: Unable to recommend a scenario. Asset instance is not detected.");
                return null;
            }

            // [TODO] should check for valid adaptation IDs in the player and scenarios?

            if (playerNode == null) {
                log(AssetPackage.Severity.Error, String.Format("In DifficultyAdapter.TargetScenario: Null player node. Returning null."));
                return null;
            }

            if (scenarioList == null || scenarioList.Count == 0) {
                log(AssetPackage.Severity.Error, String.Format("In DifficultyAdapter.TargetScenario: Null or empty scenario node list. Returning null."));
                return null;
            }

            // [SC] calculate min and max possible ratings for candidate scenarios
            double[] ratingFI = calcTargetBetas(playerNode.Rating); // [SC][2016.12.14] fuzzy interval for rating

            // [SC] info for the scenarios within the core rating range and with the lowest play count
            List<ScenarioNode> coreScenarios = new List<ScenarioNode>();
            double coreMinPlayCount = 0;

            // [SC] info for the scenarios within the support rating range and with the lowest play count
            List<ScenarioNode> supportScenarios = new List<ScenarioNode>();
            double supportMinPlayCount = 0;

            // [SC] info for the closest scenarios outside of the fuzzy interval and the lowest play count
            List<ScenarioNode> outScenarios = new List<ScenarioNode>();
            double outMinPlayCount = 0;
            double outMinDistance = 0;

            // [SC] iterate through the list of all scenarios
            foreach (ScenarioNode scenario in scenarioList) {
                double scenarioRating = scenario.Rating;
                double scenarioPlayCount = scenario.PlayCount;

                // [SC] the scenario rating is within the core rating range
                if (scenarioRating >= ratingFI[1] && scenarioRating <= ratingFI[2]) {
                    if (coreScenarios.Count == 0 || scenarioPlayCount < coreMinPlayCount) {
                        coreScenarios.Clear();
                        coreScenarios.Add(scenario);
                        coreMinPlayCount = scenarioPlayCount;
                    }
                    else if (scenarioPlayCount == coreMinPlayCount) {
                        coreScenarios.Add(scenario);
                    }
                }
                // [SC] the scenario rating is outside of the core rating range but within the support range
                else if (scenarioRating >= ratingFI[0] && scenarioRating <= ratingFI[3]) {
                    if (supportScenarios.Count == 0 || scenarioPlayCount < supportMinPlayCount) {
                        supportScenarios.Clear();
                        supportScenarios.Add(scenario);
                        supportMinPlayCount = scenarioPlayCount;
                    }
                    else if (scenarioPlayCount == supportMinPlayCount) {
                        supportScenarios.Add(scenario);
                    }
                }
                // [SC] the scenario rating is outside of the support rating range
                else {
                    double distance = Math.Min(Math.Abs(scenarioRating - ratingFI[1]), Math.Abs(scenarioRating - ratingFI[2]));
                    if (outScenarios.Count == 0 || distance < outMinDistance) {
                        outScenarios.Clear();
                        outScenarios.Add(scenario);
                        outMinDistance = distance;
                        outMinPlayCount = scenarioPlayCount;
                    }
                    else if (distance == outMinDistance && scenarioPlayCount < outMinPlayCount) {
                        outScenarios.Clear();
                        outScenarios.Add(scenario);
                        outMinPlayCount = scenarioPlayCount;
                    }
                    else if (distance == outMinDistance && scenarioPlayCount == outMinPlayCount) {
                        outScenarios.Add(scenario);
                    }
                }
            }

            if (coreScenarios.Count() > 0) {
                return coreScenarios[SimpleRNG.Next(coreScenarios.Count())];
            }
            else if (supportScenarios.Count() > 0) {
                return supportScenarios[SimpleRNG.Next(supportScenarios.Count())];
            }
            return outScenarios[SimpleRNG.Next(outScenarios.Count())];
        }

        /// <summary>
        /// Calculates a fuzzy interval for a target beta.
        /// </summary>
        ///
        /// <param name="theta"> The theta. </param>
        ///
        /// <returns>
        /// A four-element array of ratings (in an ascending order) representing lower and upper bounds of the support and core
        /// </returns>
        internal double[] calcTargetBetas(double theta) {
            // [SC] mean of one-sided normal distribution from which to derive the lower bound of the support in a fuzzy interval
            double lower_distr_mean = this.TargetDistrMean - (this.FiSDMultiplier * this.TargetDistrSD);
            if (lower_distr_mean < BaseAdapter.DistrLowerLimit) {
                lower_distr_mean = BaseAdapter.DistrLowerLimit;
            }
            // [SC] mean of one-sided normal distribution from which to derive the upper bound of the support in a fuzzy interval
            double upper_distr_mean = this.TargetDistrMean + (this.FiSDMultiplier * this.TargetDistrSD);
            if (upper_distr_mean > BaseAdapter.DistrUpperLimit) {
                upper_distr_mean = BaseAdapter.DistrUpperLimit;
            }

            // [SC] the array stores four probabilities (in an ascending order) that represent lower and upper bounds of the support and core 
            double[] randNums = new double[4];

            // [SC] calculating two probabilities as the lower and upper bounds of the core in a fuzzy interval
            double rndNum;
            for (int index = 1; index < 3; index++) {
                while (true) {
                    SimpleRNG.SetSeedFromRandom();
                    rndNum = SimpleRNG.GetNormal(this.TargetDistrMean, this.TargetDistrSD);

                    if (rndNum > this.TargetLowerLimit || rndNum < this.TargetUpperLimit) {
                        if (rndNum < BaseAdapter.DistrLowerLimit) {
                            rndNum = BaseAdapter.DistrLowerLimit;
                        }
                        else if (rndNum > BaseAdapter.DistrUpperLimit) {
                            rndNum = BaseAdapter.DistrUpperLimit;
                        }
                        break;
                    }
                }
                randNums[index] = rndNum;
            }
            // [SC] sorting lower and upper bounds of the core in an ascending order
            if (randNums[1] > randNums[2]) {
                double temp = randNums[1];
                randNums[1] = randNums[2];
                randNums[2] = temp;
            }

            // [SC] calculating probability that is the lower bound of the support in a fuzzy interval
            while (true) {
                SimpleRNG.SetSeedFromRandom();
                rndNum = SimpleRNG.GetNormal(lower_distr_mean, this.TargetDistrSD, true);

                if (rndNum < randNums[1]) {
                    if (rndNum < BaseAdapter.DistrLowerLimit) {
                        rndNum = BaseAdapter.DistrLowerLimit;
                    }
                    break;
                }
            }
            randNums[0] = rndNum;

            // [SC] calculating probability that is the upper bound of the support in a fuzzy interval
            while (true) {
                SimpleRNG.SetSeedFromRandom();
                rndNum = SimpleRNG.GetNormal(upper_distr_mean, this.TargetDistrSD, false);

                if (rndNum > randNums[2]) {
                    if (rndNum > BaseAdapter.DistrUpperLimit) {
                        rndNum = BaseAdapter.DistrUpperLimit;
                    }
                    break;
                }
            }
            randNums[3] = rndNum;

            // [SC][2016.10.07] this is the old equation to calculate target beta
            // theta + Math.Log(randomNum / (1 - randomNum));

            // [SC] tralsating probability bounds of a fuzzy interval into a beta values
            double lowerLimitBeta = theta + Math.Log((1.0d - randNums[3]) / randNums[3]);
            double minBeta = theta + Math.Log((1.0d - randNums[2]) / randNums[2]); // [SC][2016.10.07] a modified version of the equation from the original data; better suits the data
            double maxBeta = theta + Math.Log((1.0d - randNums[1]) / randNums[1]);
            double upperLimitBeta = theta + Math.Log((1.0d - randNums[0]) / randNums[0]);

            return new double[] { lowerLimitBeta, minBeta, maxBeta, upperLimitBeta };
        }

        /// <summary>
        /// Returns target difficulty rating given a skill rating.
        /// </summary>
        /// <param name="theta">Skill rating.</param>
        /// <returns>Difficulty rating.</returns>
        internal double TargetDifficultyRating(double theta) {
            return theta + Math.Log((1.0d - this.TargetDistrMean) / this.TargetDistrMean);
        }

        #endregion functions for calculating matching scenario
        ////// END: functions for calculating matching scenario
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: functions for calculating expected and actual scores
        #region functions for calculating expected and actual scores

        /// <summary>
        /// Calculates actual score given success/failure outcome and response time.
        /// </summary>
        ///
        /// <param name="correctAnswer">   should be either 0, for failure,
        ///                                         or 1 for success. </param>
        /// <param name="responseTime">    a response time in milliseconds. </param>validateResponseTime
        /// <param name="itemMaxDuration">  maximum duration of time given to a
        ///                                 player to provide an answer. </param>
        ///
        /// <returns>
        /// actual score as a double.
        /// </returns>
        internal double calcActualScore(double correctAnswer, double responseTime, double itemMaxDuration) {
            if (!(validateCorrectAnswer(correctAnswer)
                && validateResponseTime(responseTime)
                && validateItemMaxDuration(itemMaxDuration))) {

                log(AssetPackage.Severity.Error
                    , String.Format("In DifficultyAdapter.calcActualScore: Cannot calculate score. Invalid parameter detected. Returning error code '{0}'."
                        , BaseAdapter.ErrorCode));

                return BaseAdapter.ErrorCode;
            }

            // [SC][2017.01.03]
            if (responseTime > itemMaxDuration) {
                responseTime = itemMaxDuration;

                log(AssetPackage.Severity.Warning
                    , String.Format("In DifficultyAdapter.calcActualScore: Response time '{0}' exceeds the item's max time duration '{1}'. Setting the response time to item's max duration."
                        , responseTime, itemMaxDuration));
            }

            double discrParam = getDiscriminationParam(itemMaxDuration);
            return (double)(((2.0d * correctAnswer) - 1.0d) * ((discrParam * itemMaxDuration) - (discrParam * responseTime)));
        }

        /// <summary>
        /// Calculates expected score given player's skill rating and item's
        /// difficulty rating.
        /// </summary>
        ///
        /// <param name="playerTheta">     player's skill rating. </param>
        /// <param name="itemBeta">        item's difficulty rating. </param>
        /// <param name="itemMaxDuration">  maximum duration of time given to a
        ///                                 player to provide an answer. </param>
        ///
        /// <returns>
        /// expected score as a double.
        /// </returns>
        internal double calcExpectedScore(double playerTheta, double itemBeta, double itemMaxDuration) {
            if (!validateItemMaxDuration(itemMaxDuration)) {

                log(AssetPackage.Severity.Error
                    , String.Format("In DifficultyAdapter.calcExpectedScore: Cannot calculate score. Invalid parameter detected. Returning error code '{0}'."
                        , BaseAdapter.ErrorCode));

                return BaseAdapter.ErrorCode;
            }

            double weight = getDiscriminationParam(itemMaxDuration) * itemMaxDuration;

            double ratingDifference = playerTheta - itemBeta; // [SC][2016.01.07]
            if (ratingDifference == 0) { // [SC][2016.01.07]
                ratingDifference = 0.001d;
            }

            double expFctr = (double)Math.Exp(2.0d * weight * ratingDifference); // [SC][2016.01.07]

            return (weight * ((expFctr + 1.0d) / (expFctr - 1.0d))) - (1.0d / ratingDifference); // [SC][2016.01.07]
        }

        /// <summary>
        /// Calculates discrimination parameter a_i necessary to calculate expected
        /// and actual scores.
        /// </summary>
        ///
        /// <param name="itemMaxDuration">  maximum duration of time given to a
        ///                                 player to provide an answer; should be
        ///                                 player. </param>
        ///
        /// <returns>
        /// discrimination parameter a_i as double number.
        /// </returns>
        internal double getDiscriminationParam(double itemMaxDuration) {
            return (double)(1.0d / itemMaxDuration);
        }

        #endregion functions for calculating expected and actual scores
        ////// END: functions for calculating expected and actual scores
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: functions for calculating rating uncertainties
        #region functions for calculating rating uncertainties

        /// <summary>
        /// Calculates a new uncertainty for the theta rating.
        /// </summary>
        ///
        /// <param name="currThetaU">       current uncertainty value for theta
        ///                                 rating. </param>
        /// <param name="currDelayCount">   the current number of consecutive days
        ///                                 the player has not played. </param>
        ///
        /// <returns>
        /// a new uncertainty value for theta rating.
        /// </returns>
        internal double calcThetaUncertainty(double currThetaU, double currDelayCount) {
            double newThetaU = currThetaU - (1.0d / this.MaxPlay) + (currDelayCount / this.MaxDelay);
            if (newThetaU < 0) {
                newThetaU = 0.0d;
            }
            else if (newThetaU > 1) {
                newThetaU = 1.0d;
            }
            return newThetaU;
        }

        /// <summary>
        /// Calculates a new uncertainty for the beta rating.
        /// </summary>
        ///
        /// <param name="currBetaU">        current uncertainty value for the beta
        ///                                 rating. </param>
        /// <param name="currDelayCount">   the current number of consecutive days
        ///                                 the item has not beein played. </param>
        ///
        /// <returns>
        /// a new uncertainty value for the beta rating.
        /// </returns>
        internal double calcBetaUncertainty(double currBetaU, double currDelayCount) {
            double newBetaU = currBetaU - (1.0d / this.MaxPlay) + (currDelayCount / this.MaxDelay);
            if (newBetaU < 0) {
                newBetaU = 0.0d;
            }
            else if (newBetaU > 1) {
                newBetaU = 1.0d;
            }
            return newBetaU;
        }

        #endregion functions for calculating rating uncertainties
        ////// END: functions for calculating rating uncertainties
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: functions for calculating k factors
        #region functions for calculating k factors

        /// <summary>
        /// Calculates a new K factor for theta rating
        /// </summary>
        ///
        /// <param name="currThetaU">           current uncertainty for the theta rating</param>
        /// <param name="currBetaU">            current uncertainty for the beta rating</param>
        /// <param name="playerPlayCount">      a number of past games played by the player</param>
        /// 
        /// <returns>a double value of a new K factor for the theta rating</returns>
        internal double calcThetaKFctr(double currThetaU, double currBetaU, double playerPlayCount) {
            // [SC] calculate K based on uncertainty
            double playerK = this.KConst * (1.0d + (this.KUp * currThetaU) - (this.KDown * currBetaU));

            // [SC] check if the player is in calibration phase
            if (this.PlayerCalLength > playerPlayCount) {
                playerK += this.PlayerCalK;
            }

            return playerK;
        }

        /// <summary>
        /// Calculates a new K factor for the beta rating
        /// </summary>
        /// 
        /// <param name="currThetaU">           current uncertainty fot the theta rating</param>
        /// <param name="currBetaU">            current uncertainty for the beta rating</param>
        /// <param name="scenarioPlayCount">    a number of past games played with this scenario</param>
        /// 
        /// <returns>a double value of a new K factor for the beta rating</returns>
        internal double calcBetaKFctr(double currThetaU, double currBetaU, double scenarioPlayCount) {
            // [SC] calculate K based on uncertainty
            double scenarioK = this.KConst * (1.0d + (this.KUp * currBetaU) - (this.KDown * currThetaU));
            
            // [SC] check if the scenario is in calibration phase
            if (this.ScenarioCalLength > scenarioPlayCount) {
                scenarioK += this.ScenarioCalK;
            }

            return scenarioK;
        }

        #endregion functions for calculating k factors
        ////// END: functions for calculating k factors
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: functions for calculating theta and beta ratings
        #region functions for calculating theta and beta ratings

        /// <summary>
        /// Calculates a new theta rating.
        /// </summary>
        ///
        /// <param name="currTheta">   current theta rating. </param>
        /// <param name="thetaKFctr">  K factor for the theta rating. </param>
        /// <param name="actualScore"> actual performance score. </param>
        /// <param name="expectScore"> expected performance score. </param>
        ///
        /// <returns>
        /// a double value for the new theta rating.
        /// </returns>
        internal double calcTheta(double currTheta, double thetaKFctr, double actualScore, double expectScore) {
            return currTheta + (thetaKFctr * (actualScore - expectScore));
        }

        /// <summary>
        /// Calculates a new beta rating.
        /// </summary>
        ///
        /// <param name="currBeta">    current beta rating. </param>
        /// <param name="betaKFctr">   K factor for the beta rating. </param>
        /// <param name="actualScore"> actual performance score. </param>
        /// <param name="expectScore"> expected performance score. </param>
        ///
        /// <returns>
        /// a double value for new beta rating.
        /// </returns>
        internal double calcBeta(double currBeta, double betaKFctr, double actualScore, double expectScore) {
            return currBeta + (betaKFctr * (expectScore - actualScore));
        }

        #endregion functions for calculating theta and beta ratings
        ////// END: functions for calculating theta and beta ratings
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: tester functions
        #region value tester functions

        /// <summary>
        /// Tests the validity of the value representing correctness of player's answer.
        /// </summary>
        /// 
        /// <param name="correctAnswer"> Player's answer. </param>
        /// 
        /// <returns>True if the value is valid</returns>
        internal bool validateCorrectAnswer(double correctAnswer) { // [SC][2017.01.03]
            if (correctAnswer != 0 && correctAnswer != 1) {
                log(AssetPackage.Severity.Error
                    , String.Format("In DifficultyAdapter.validateCorrectAnswer: Accuracy should be either 0 or 1. Current value is '{0}'.", correctAnswer));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests the validity of the value representing the response time.
        /// </summary>
        /// 
        /// <param name="responseTime">Response time in milliseconds</param>
        /// 
        /// <returns>True if the value is valid</returns>
        internal bool validateResponseTime(double responseTime) {
            if (responseTime <= 0) {
                log(AssetPackage.Severity.Error
                    , String.Format("In DifficultyAdapter.validateResponseTime: Response time cannot be 0 or negative. Current value is '{0}'.", responseTime));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests the validity of the value representing the max amount of time to respond.
        /// </summary>
        /// 
        /// <param name="itemMaxDuration">Time duration in mulliseconds</param>
        /// 
        /// <returns>True if the value is valid</returns>
        internal bool validateItemMaxDuration(double itemMaxDuration) {
            if (itemMaxDuration <= 0) {
                log(AssetPackage.Severity.Error
                    , String.Format("In DifficultyAdapter.validateItemMaxDuration: Max playable duration cannot be 0 or negative. Current value is '{0}'."
                        , itemMaxDuration));

                return false;
            }

            return true;
        }

        #endregion value tester functions
        ////// END: tester functions
        //////////////////////////////////////////////////////////////////////////////////////

        #endregion Methods
    }
}