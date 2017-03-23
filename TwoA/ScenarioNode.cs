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

Namespace: TwoA
Filename: ScenarioNode.cs
Description:
    [TODO]
*/


// Change history:
// [2017.03.16]
//      - [SC] first created

#endregion Header

namespace TwoA 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The Scenario node
    /// </summary>
    public class ScenarioNode 
    {
        private string adaptID;
        private string gameID;
        private string scenarioID;
        private double rating = BaseAdapter.INITIAL_RATING;
        private double playCount = 0;
        private double kFct = BaseAdapter.INITIAL_K_FCT;
        private double uncertainty = BaseAdapter.INITIAL_UNCERTAINTY;
        private DateTime lastPlayed = DateTime.UtcNow;
        private double timeLimit = BaseAdapter.DEFAULT_TIME_LIMIT;

        /// <summary>
        /// Identifier for the Adaptation node.
        /// </summary>
        public String AdaptationID {
            get { return this.adaptID; }
            set {
                if (!String.IsNullOrEmpty(value)) {
                    this.adaptID = value;
                }
            }
        }

        /// <summary>
        /// Identifier for the Game node.
        /// </summary>
        public String GameID {
            get { return this.gameID; }
            set {
                if (!String.IsNullOrEmpty(value)) {
                    this.gameID = value;
                }
            }
        }

        /// <summary>
        /// Identifier for the Scenario node.
        /// </summary>
        public String ScenarioID {
            get { return this.scenarioID; }
            set {
                if (!String.IsNullOrEmpty(value)) {
                    this.scenarioID = value;
                }
            }
        }

        /// <summary>
        /// Scenario rating
        /// </summary>
        public Double Rating {
            get { return this.rating; }
            set { this.rating = value; }
        }

        /// <summary>
        /// Number of times the scenario was played.
        /// </summary>
        public Double PlayCount {
            get { return this.playCount; }
            set {
                if (value >= 0) {
                    this.playCount = value;
                }
            }
        }

        /// <summary>
        /// Scenario's K factor.
        /// </summary>
        public Double KFactor {
            get { return this.kFct; }
            set {
                if (value > 0) {
                    this.kFct = value;
                }
            }
        }

        /// <summary>
        /// Uncertainty in scenario's rating.
        /// </summary>
        public Double Uncertainty {
            get { return this.uncertainty; }
            set {
                if (value >= 0 && value <= 1) {
                    this.uncertainty = value;
                }
            }
        }

        /// <summary>
        /// Last time the scenario was played.
        /// </summary>
        public DateTime LastPlayed {
            get { return this.lastPlayed; }
            set {
                if (value != null) {
                    this.lastPlayed = value;
                }
            }
        }

        /// <summary>
        /// Time limit for completing the scenario.
        /// </summary>
        public Double TimeLimit {
            get { return this.timeLimit; }
            set {
                if (value > 0) {
                    this.timeLimit = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ScenarioNode() { 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adaptID">Adaptation ID</param>
        /// <param name="gameID">Game ID</param>
        /// <param name="scenarioID">Scenario ID</param>
        /// <param name="rating">Scenario rating</param>
        /// <param name="playCount">Scenario's play count</param>
        /// <param name="kFct">Scenario's K factor</param>
        /// <param name="uncertainty">Scenario's rating uncertainty</param>
        /// <param name="lastPlayed">Datetime the scenario was last played and assessed</param>
        /// <param name="timeLimit">Time limit to complete the scenario measured in milliseconds</param>
        public ScenarioNode(string adaptID, string gameID, string scenarioID
                            , double rating, double playCount, double kFct, double uncertainty, DateTime lastPlayed, double timeLimit) {
            this.AdaptationID = adaptID;
            this.GameID = gameID;
            this.ScenarioID = scenarioID;

            this.Rating = rating;
            this.PlayCount = playCount;
            this.KFactor = kFct;
            this.Uncertainty = uncertainty;
            this.LastPlayed = lastPlayed;
            this.TimeLimit = timeLimit;
        }

        /// <summary>
        /// Makes a shallow clone of this instance.
        /// </summary>
        /// <returns>New instance of ScenarioNode</returns>
        public ScenarioNode ShallowClone() {
            return new ScenarioNode(this.AdaptationID, this.GameID, this.ScenarioID
                , this.Rating, this.PlayCount, this.KFactor, this.Uncertainty, this.LastPlayed, this.TimeLimit);
        }
    }
}
