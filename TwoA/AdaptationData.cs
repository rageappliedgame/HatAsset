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
Filename: AdaptationData.cs
*/

// Change history:
// [2016.10.06]
//      - [SC] renamed namespace 'HAT' to 'TwoA'

#endregion Header

namespace TwoA
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.ComponentModel; // [SC] DefaultValue

    /// <summary>
    /// The AdaptationData node.
    /// </summary>
    public class AdaptationData 
    {
        /// <summary>
        /// A list of Adaptation nodes.
        /// </summary>
        [XmlElement("Adaptation")]
        public List<AdaptationNode> AdaptationList;
    }

    /// <summary>
    /// The Adaptation node.
    /// </summary>
    public class AdaptationNode 
    {
        /// <summary>
        /// A list of Game nodes.
        /// </summary>
        [XmlElement("Game")]
        public List<GameNode> GameList;

        /// <summary>
        /// Identifier for the Adaptation node.
        /// </summary>
        [XmlAttribute("AdaptationID")]
        public String AdaptationID { get; set; }
    }

    /// <summary>
    /// The Game node
    /// </summary>
    public class GameNode 
    {
        /// <summary>
        /// The ScenarioData node.
        /// </summary>
        [XmlElement("ScenarioData")]
        public ScenarioDataNode ScenarioData;

        /// <summary>
        /// The PlayerData node.
        /// </summary>
        [XmlElement("PlayerData")]
        public PlayerDataNode PlayerData;

        /// <summary>
        /// Identifier for the Game node.
        /// </summary>
        [XmlAttribute("GameID")]
        public String GameID { get; set; }
    }

    /// <summary>
    /// The ScenarioData node
    /// </summary>
    public class ScenarioDataNode 
    {
        /// <summary>
        /// A list of Scenario nodes
        /// </summary>
        [XmlElement("Scenario")]
        public List<ScenarioNode> ScenarioList;
    }

    /// <summary>
    /// The Scenario node
    /// </summary>
    public class ScenarioNode 
    {
        /// <summary>
        /// Identifier for the Scenario node.
        /// </summary>
        [XmlAttribute("ScenarioID")]
        public String ScenarioID { get; set; }

        /// <summary>
        /// Scenario rating
        /// </summary>
        public Double Rating { get; set; }

        /// <summary>
        /// Number of times the scenario was played.
        /// </summary>
        public Double PlayCount { get; set; }

        /// <summary>
        /// Scenario's K factor.
        /// </summary>
        public Double KFactor { get; set; }

        /// <summary>
        /// Uncertainty in scenario's rating.
        /// </summary>
        public Double Uncertainty { get; set; }

        /// <summary>
        /// Last time the scenario was played.
        /// </summary>
        public String LastPlayed { get; set; }

        /// <summary>
        /// Time limit for completing the scenario.
        /// </summary>
        public Double TimeLimit { get; set; }
    }

    /// <summary>
    /// The PlayerData node
    /// </summary>
    public class PlayerDataNode 
    {
        /// <summary>
        /// A list of Player nodes
        /// </summary>
        [XmlElement("Player")]
        public List<PlayerNode> PlayerList;
    }

    /// <summary>
    /// The Scenario node
    /// </summary>
    public class PlayerNode 
    {
        /// <summary>
        /// Identifier for the Player node.
        /// </summary>
        [XmlAttribute("PlayerID")]
        public String PlayerID { get; set; }

        /// <summary>
        /// Player rating
        /// </summary>
        public Double Rating { get; set; }

        /// <summary>
        /// Number of times the player played any scenario.
        /// </summary>
        public Double PlayCount { get; set; }

        /// <summary>
        /// Player's K factor.
        /// </summary>
        public Double KFactor { get; set; }

        /// <summary>
        /// Uncertainty in player's rating.
        /// </summary>
        public Double Uncertainty { get; set; }

        /// <summary>
        /// Last time the player played.
        /// </summary>
        public String LastPlayed { get; set; }
    }
}