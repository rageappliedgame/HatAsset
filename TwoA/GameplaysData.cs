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
Filename: GameplaysData.cs
*/

// Change history:
// [2016.10.06]
//      - [SC] renamed namespace 'HAT' to 'TwoA'
//      - [SC] renamed class 'HatAdaptation' to 'TwoAAdaptation'
//      - [SC] renamed class 'HatGame' to 'TwoAGame'
//      - [SC] renamed class 'HatGameplay' to 'TwoAGameplay'

#endregion Header

namespace TwoA 
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// The gameplays data.
    /// </summary>
    public class GameplaysData 
    {
        /// <summary>
        /// The adaptation.
        /// </summary>
        [XmlElement("Adaptation")]
        public List<TwoAAdaptation> Adaptation;
    }

    /// <summary>
    /// A TwoA adaptation.
    /// </summary>
    public class TwoAAdaptation 
    {
        /// <summary>
        /// The game.
        /// </summary>
        [XmlElement("Game")]
        public List<TwoAGame> Game;

        /// <summary>
        /// Identifier for the adaptation.
        /// </summary>
        [XmlAttribute("AdaptationID")]
        public String AdaptationID;

    }

    /// <summary>
    /// A TwoA game.
    /// </summary>
    public class TwoAGame 
    {
        /// <summary>
        /// The gameplay.
        /// </summary>
        [XmlElement("Gameplay")]
        public List<TwoAGameplay> Gameplay;

        /// <summary>
        /// Identifier for the game.
        /// </summary>
        [XmlAttribute("GameID")]
        public String GameID;
    }

    /// <summary>
    /// A TwoA gameplay.
    /// </summary>
    public class TwoAGameplay 
    {
        /// <summary>
        /// Identifier for the player.
        /// </summary>
        [XmlAttribute("PlayerID")]
        public String PlayerID;

        /// <summary>
        /// Identifier for the scenario.
        /// </summary>
        [XmlAttribute("ScenarioID")]
        public String ScenarioID;

        /// <summary>
        /// The timestamp.
        /// </summary>
        [XmlAttribute("Timestamp")]
        public String Timestamp;

        /// <summary>
        /// The RT.
        /// </summary>
        public Double RT;

        /// <summary>
        /// The accuracy.
        /// </summary>
        public Double Accuracy;

        /// <summary>
        /// The player rating.
        /// </summary>
        public Double PlayerRating;

        /// <summary>
        /// The scenario rating.
        /// </summary>
        public Double ScenarioRating;
    }
}