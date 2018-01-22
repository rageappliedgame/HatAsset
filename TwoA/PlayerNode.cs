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
Filename: PlayerNode.cs
Description:
    [TODO]
*/


// Change history:
// [2017.03.16]
//      - [SC] first created
// [2017.11.03]
//      - [SC] set the private access modifier to the constructor with zero params
//      - [SC] added a constructor with three ID params 
// [2017.12.19]
//      - [SC] changed the namespace from TwoA to TwoANS

#endregion Header

namespace TwoANS 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The Player node
    /// </summary>
    public class PlayerNode 
    {
        private string adaptID;
        private string gameID;
        private string playerID;
        private double rating = BaseAdapter.INITIAL_RATING;
        private double playCount = 0;
        private double kFct = BaseAdapter.INITIAL_K_FCT;
        private double uncertainty = BaseAdapter.INITIAL_UNCERTAINTY;
        private DateTime lastPlayed = DateTime.UtcNow;

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
        /// Identifier for the Player node.
        /// </summary>
        public String PlayerID {
            get { return this.playerID; }
            set {
                if (!String.IsNullOrEmpty(value)) {
                    this.playerID = value;
                }
            }
        }

        /// <summary>
        /// Player rating
        /// </summary>
        public Double Rating {
            get { return this.rating; }
            set { this.rating = value; }
        }

        /// <summary>
        /// Number of times the player played any scenario.
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
        /// Player's K factor.
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
        /// Uncertainty in player's rating.
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
        /// Last time the player played.
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
        /// Constructor
        /// </summary>
        private PlayerNode() {}
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adaptID">Adaptation ID</param>
        /// <param name="gameID">Game ID</param>
        /// <param name="playerID">Player ID</param>
        public PlayerNode(string adaptID, string gameID, string playerID) {
            this.AdaptationID = adaptID;
            this.GameID = gameID;
            this.PlayerID = playerID;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adaptID">Adaptation ID</param>
        /// <param name="gameID">Game ID</param>
        /// <param name="playerID">Player ID</param>
        /// <param name="rating">Player rating</param>
        /// <param name="playCount">Player's play count</param>
        /// <param name="kFct">Player's K factor</param>
        /// <param name="uncertainty">Player's rating uncertainty</param>
        /// <param name="lastPlayed">Datetime player last played a game</param>
        public PlayerNode(string adaptID, string gameID, string playerID
                            , double rating, double playCount, double kFct, double uncertainty, DateTime lastPlayed) {
            this.AdaptationID = adaptID;
            this.GameID = gameID;
            this.PlayerID = playerID;

            this.Rating = rating;
            this.PlayCount = playCount;
            this.KFactor = kFct;
            this.Uncertainty = uncertainty;
            this.LastPlayed = lastPlayed;
        }

        /// <summary>
        /// Makes a shallow clone of this instance.
        /// </summary>
        /// <returns>New instance of PlayerNode</returns>
        public PlayerNode ShallowClone() {
            return new PlayerNode(this.AdaptationID, this.GameID, this.PlayerID
                , this.Rating, this.PlayCount, this.KFactor, this.Uncertainty, this.LastPlayed);
        }
    }
}
