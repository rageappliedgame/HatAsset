using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoA {
    /// <summary>
    /// A TwoA gameplay.
    /// </summary>
    public class Gameplay {

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
