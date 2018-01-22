#region Header

/*
Copyright 2018 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

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
Filename: Gameplay.cs
Description:
    [TODO]
*/


// Change history:
// [2017.12.19]
//      - [SC] changed the namespace from TwoA to TwoANS

#endregion Header

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoANS 
{
    /// <summary>
    /// A TwoA gameplay.
    /// </summary>
    public class Gameplay 
    {
        /// <summary>
        /// Identifier for the Adaptation node.
        /// </summary>
        public String AdaptationID;

        /// <summary>
        /// Identifier for the Game node.
        /// </summary>
        public String GameID;

        /// <summary>
        /// Identifier for the player.
        /// </summary>
        public String PlayerID;

        /// <summary>
        /// Identifier for the scenario.
        /// </summary>
        public String ScenarioID;

        /// <summary>
        /// The timestamp.
        /// </summary>
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
