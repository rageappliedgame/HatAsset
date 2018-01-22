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
Filename: KSRank.cs
Description:
    See the description of the KStructure class.
*/

// [TODO]
// in addState method, add log msg indicating that rank 0 cannot have states other than the root state
// in addState method, add log msg indicating that rank index and state category count are not the same
// in removeStateAt method, add log msg indicating that no state can be removed from rank 0
//

// Change history
// [2016.10.13]
//      - [SC] First created
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
    /// Represents a rank in a knowledge structure
    /// </summary>
    public class KSRank
    {
        #region Fields

        /// <summary>
        /// Index of the rank in the knowledge structure.
        /// </summary>
        private int rankIndex;
        
        /// <summary>
        /// List of state belonging to this rank.
        /// </summary>
        private List<KState> states;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Getter/setter for the index of the rank in the knowledge structure.
        /// </summary>
        public int RankIndex {
            get {
                return this.rankIndex;
            }
            set {
                if (value == KSGenerator.UNASSIGNED_RANK || value >= 0) {
                    this.rankIndex = value;

                    if (value == 0) { // [SC] 0th rank has only the root state
                        this.states.Clear();
                        KState rootState = new KState(KSGenerator.ROOT_STATE);
                        rootState.Id = KSGenerator.getStateID(value, 1);
                        this.states.Add(rootState);
                    }
                }
                else {
                    throw new System.ArgumentException("Rank should be a non-zero positive value.");
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// 
        /// <param name="rankIndex">    Rank index</param>
        /// <param name="states">       List of states of this rank</param>
        private KSRank(int rankIndex, List<KState> states) {
            this.states = states;
            this.RankIndex = rankIndex;
        }

        /// <summary>
        /// Constructor initializes with an empty list of states.
        /// </summary>
        /// 
        /// <param name="rankIndex">Rank index</param>
        public KSRank(int rankIndex) : this(rankIndex, new List<KState>()) {
            // [SC] empty constructor            
        }

        /// <summary>
        /// Constructor initializes with an assigned rank index and an empty list of states.
        /// </summary>
        public KSRank() : this(KSGenerator.UNASSIGNED_RANK) {
            // [SC] empty constructor
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns the number of states in the rank.
        /// </summary>
        /// 
        /// <returns>state count</returns>
        public int getStateCount() {
            return this.states.Count;
        }

        /// <summary>
        /// Add a specified to this rank.
        /// </summary>
        /// 
        /// <param name="state">KState object to add to the rank</param>
        /// 
        /// <returns>True if the state was added successfully</returns>
        public bool addState(KState state) {
            if (this.RankIndex == 0) {
                return false;
            }

            if (this.RankIndex != state.getCategoryCount()) {
                return false;
            }

            // [SC] verify that the same state does not already exist; verification is done at PCategory reference level
            foreach (KState stateOne in this.states) {
                if (state.getCategoryCount() == stateOne.getCategoryCount()
                    && state.getCategories().Intersect(stateOne.getCategories()).Count() == state.getCategoryCount()) {
                    return false;
                }
            }

            this.states.Add(state);

            return true;
        }

        /// <summary>
        /// Removes the specified state from this rank.
        /// </summary>
        /// 
        /// <param name="state">KState object to remove</param>
        /// 
        /// <returns>True if the state was removed successfully</returns>
        public bool removeState(KState state) {
            return this.states.Remove(state);
        }

        /// <summary>
        /// Removes a state at the specified index of the list.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>True if a state was removed successfully</returns>
        public bool removeStateAt(int index) {
            if (this.RankIndex == 0) {
                return false;
            }

            if (this.getStateCount() > index && index >= 0) {
                this.states.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Retrieve state at the specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>KState object, or null if index is out of range</returns>
        public KState getStateAt(int index) {
            if (this.getStateCount() > index && index >= 0) {
                return this.states[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the list of all states
        /// </summary>
        /// 
        /// <returns>List of KState objects</returns>
        public List<KState> getStates() {
            return this.states;
        }

        #endregion Methods
    }
}
