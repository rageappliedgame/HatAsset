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

Namespace: HAT
Filename: HATAsset.cs
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
//
// TODO:
//      - InitSettings need total rewriting
//      - Different adaptation algorithms may need different XML nodes; need a XML reading method independent of XML structure
//      - Validate XML files against schema


#endregion Header

namespace HAT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;
    using AssetManagerPackage;
    using AssetPackage;

    // using Swiss;
    /// <summary>
    /// A hat asset.
    /// </summary>
    public class HATAsset : BaseAsset
    {
        #region Fields

        internal const string DATE_FORMAT = "yyyy-MM-ddThh:mm:ss";

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
        private string adaptFile = "HATAssetAppSettings.xml";

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
        /// Initializes a new instance of the HAT.HATAsset class.
        /// </summary>
        ///
        /// <remarks>
        /// Please use HATAsset(IBridge bridge) and a Bridge implementing at least
        /// IDataStorage instead as this Asset needs to load data <br/>
        /// OR <br/>
        /// call the InitSettings() method after creation and configuring a Bridge.
        /// </remarks>
        public HATAsset() : this(null) {
            //
        }

        /// <summary>
        /// Initializes a new instance of the HAT.HATAsset class.
        /// </summary>
        ///
        /// <param name="bridge"> The bridge. </param>
        public HATAsset(IBridge bridge) : base(bridge) {
            InitSettings();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initialises the settings.
        /// </summary>
        public void InitSettings() {
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
            /*Log(Severity.Verbose
                , @"HATAsset.TargetScenarioID('{0}','{1}','{2}') -> {3}"
                , adaptID, gameID, playerID);*/

            if (adaptID.Equals(adapter.Type))
                return adapter.TargetScenarioID(gameID, playerID);
            else
                return null; // [TODO]
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
            /*Log(Severity.Verbose
                , @"HATAsset.UpdateRatings('{0}','{1}','{2}','{3}',{4:0.0},{5:0.0})"
                , adaptID, gameID, playerID, scenarioID, rt, correctAnswer);*/

            if (adaptID.Equals(adapter.Type))
                adapter.UpdateRatings(gameID, playerID, scenarioID, rt, correctAnswer, updateDatafiles);
            else
                return; // [TODO]
        }

        /// <summary>
        /// Creates new record to the game log.
        /// </summary>
        ///
        /// <param name="adaptID">        Identifier for the adapt. </param>
        /// <param name="gameID">         Identifier for the game. </param>
        /// <param name="playerID">       Identifier for the player. </param>
        /// <param name="scenarioID">     Identifier for the scenario. </param>
        /// <param name="rt">             The right. </param>
        /// <param name="accuracy">       The correct answer. </param>
        /// <param name="playerRating">     The player new rating. </param>
        /// <param name="scenarioRating"> The scenario new rating. </param>
        /// <param name="timestamp">      The current date time. </param>
        /// <param name="updateDatafiles">  Set to true to update adaptation and gameplay logs files. </param>
        public void CreateNewRecord(string adaptID, string gameID, string playerID, string scenarioID
                                        , double rt, double accuracy
                                        , double playerRating, double scenarioRating, DateTime timestamp, bool updateDatafiles) {

            // Check if Adaption is there.
            if (gameplaydata.Adaptation.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                HatAdaptation adaptNode = gameplaydata.Adaptation.First(p => p.AdaptationID.Equals(adaptID));

                // Check if Game is there
                if (adaptNode.Game.Count(p => p.GameID.Equals(gameID)) == 1) {
                    HatGame gameNode = adaptNode.Game.First(p => p.GameID.Equals(gameID));

                    gameNode.Gameplay.Add(
                        new HatGameplay() {
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
                    if (updateDatafiles) SaveGameplayData();

                    return;
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to log a gameplay record for player {0} playing scenario {1} with adaptation {2} in game {3}."
                                                            , playerID, scenarioID, adaptID, gameID));
        }

        /// <summary>
        /// Get a Property of the Player setting.
        /// </summary>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments
        ///                                         have unsupported or illegal values. </exception>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="adaptID">  Identifier for the adapt. </param>
        /// <param name="gameID">   Identifier for the game. </param>
        /// <param name="playerID"> Identifier for the player. </param>
        /// <param name="item">     The item. </param>
        ///
        /// <returns>
        /// A T.
        /// </returns>
        public T PlayerParam<T>(string adaptID, string gameID, string playerID, string item) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    PlayerDataNode playerDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).PlayerData;

                    // [SC] verify that the player exists
                    if (playerDataNode.PlayerList.Count(p => p.PlayerID.Equals(playerID)) == 1) {
                        PlayerNode playerNode = playerDataNode.PlayerList.First(p => p.PlayerID.Equals(playerID));

                        // [SC] make sure the property exists and is readable
                        PropertyInfo pi = typeof(PlayerNode).GetProperty(item);

                        if (pi != null && pi.CanRead) {
                            return (T)pi.GetValue(playerNode, new Object[] { });
                        }
                    }
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to get {0} for player {1} for adaptation {2} in game {3}."
                                        , item, playerID, adaptID, gameID));
        }

        /// <summary>
        /// Set a Property of the Player setting.
        /// </summary>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="adaptID">  Identifier for the adapt. </param>
        /// <param name="gameID">   Identifier for the game. </param>
        /// <param name="playerID"> Identifier for the player. </param>
        /// <param name="item">     The item. </param>
        /// <param name="value">    The value. </param>
        public void PlayerParam<T>(string adaptID, string gameID, string playerID, string item, T value) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    PlayerDataNode playerDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).PlayerData;

                    // [SC] verify that the player exists
                    if (playerDataNode.PlayerList.Count(p => p.PlayerID.Equals(playerID)) == 1) {
                        PlayerNode playerNode = playerDataNode.PlayerList.First(p => p.PlayerID.Equals(playerID));

                        // [SC] make sure the property exists and is readable
                        PropertyInfo pi = typeof(PlayerNode).GetProperty(item);

                        if (pi != null && pi.CanWrite) {
                            pi.SetValue(playerNode, value, new Object[] { });
                            return;
                        }
                    }
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to set value {0} for {1} for player {2} for adaptation {3} in game {4}."
                                                        , value, item, playerID, adaptID, gameID));
        }

        /// <summary>
        /// Get a Property of the Scenario setting.
        /// </summary>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments
        ///                                         have unsupported or illegal values. </exception>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="adaptID">    Identifier for the adapt. </param>
        /// <param name="gameID">     Identifier for the game. </param>
        /// <param name="scenarioID"> Identifier for the scenario. </param>
        /// <param name="item">       The item. </param>
        ///
        /// <returns>
        /// A T.
        /// </returns>
        public T ScenarioParam<T>(string adaptID, string gameID, string scenarioID, string item) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    ScenarioDataNode scenarioDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).ScenarioData;

                    // [SC] verify that the scenario exists
                    if (scenarioDataNode.ScenarioList.Count(p => p.ScenarioID.Equals(scenarioID)) == 1) {
                        ScenarioNode scenarioNode = scenarioDataNode.ScenarioList.First(p => p.ScenarioID.Equals(scenarioID));

                        // [SC] make sure the property exists and is readable
                        PropertyInfo pi = typeof(ScenarioNode).GetProperty(item);

                        if (pi != null && pi.CanRead) {
                            return (T)pi.GetValue(scenarioNode, new Object[] { });
                        }
                    }
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to get {0} for scenario {1} for adaptation {2} in game {3}."
                                        , item, scenarioID, adaptID, gameID));
        }

        /// <summary>
        /// Set a Property of the Scenario setting.
        /// </summary>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="adaptID">    Identifier for the adapt. </param>
        /// <param name="gameID">     Identifier for the game. </param>
        /// <param name="scenarioID"> Identifier for the scenario. </param>
        /// <param name="item">       The item. </param>
        /// <param name="value">      The value. </param>
        public void ScenarioParam<T>(string adaptID, string gameID, string scenarioID, string item, T value) {
            // [SC] verify that the adaptation exists
            if (adaptData.AdaptationList.Count(p => p.AdaptationID.Equals(adaptID)) == 1) {
                AdaptationNode adaptNode = adaptData.AdaptationList.First(p => p.AdaptationID.Equals(adaptID));

                // [SC] verify that the game exists
                if (adaptNode.GameList.Count(p => p.GameID.Equals(gameID)) == 1) {
                    ScenarioDataNode scenarioDataNode = adaptNode.GameList.First(p => p.GameID.Equals(gameID)).ScenarioData;

                    // [SC] verify that the scenario exists
                    if (scenarioDataNode.ScenarioList.Count(p => p.ScenarioID.Equals(scenarioID)) == 1) {
                        ScenarioNode scenarioNode = scenarioDataNode.ScenarioList.First(p => p.ScenarioID.Equals(scenarioID));

                        // [SC] make sure the property exists and is readable
                        PropertyInfo pi = typeof(ScenarioNode).GetProperty(item);

                        if (pi != null && pi.CanWrite) {
                            pi.SetValue(scenarioNode, value, new Object[] { });
                            return;
                        }
                    }
                }
            }

            // [TODO]
            throw new ArgumentException(String.Format("Unable to set value {0} for {1} for scenario {2} for adaptation {3} in game {4}."
                                                        , value, item, scenarioID, adaptID, gameID));
        }

        /// <summary>
        /// Loads adaptation data.
        /// </summary>
        private void LoadAdaptationData() {
            IDataStorage ds = getInterface<IDataStorage>();

            if (ds == null)
                throw new ArgumentException(String.Format("Unable to load the file for adaptation data '{0}'. Cannot access IDataStorage interface.", adaptFile));

            if (!ds.Exists(adaptFile))
                throw new ArgumentException(String.Format("The file for adaptation data '{0}' does not exist.", adaptFile));

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

            if (ds == null)
                throw new ArgumentException(String.Format("Unable to open file for adaptation data '{0}'. Cannot access IDataStorage interface.", adaptFile));

            if (!ds.Exists(adaptFile))
                throw new ArgumentException(String.Format("The file for adaptation data '{0}' does not exist.", adaptFile));

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

            if (ds == null)
                throw new ArgumentException(String.Format("Unable to load the file for gameplay logs '{0}'. Cannot access IDataStorage interface.", gameplayLogsFile));

            if (!ds.Exists(gameplayLogsFile))
                throw new ArgumentException(String.Format("The file for gameplay logs '{0}' does not exist.", gameplayLogsFile));

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

            if (ds == null)
                throw new ArgumentException(String.Format("Unable to open file for gameplay logs {0}. Cannot access IDataStorage interface.", gameplayLogsFile));

            if (!ds.Exists(gameplayLogsFile))
                throw new ArgumentException(String.Format("The file for gameplay logs '{0}' does not exist.", gameplayLogsFile));

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