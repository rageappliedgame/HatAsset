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
Filename: KStructure.cs
Description:
    KStructure represents a single knowledge structure. 
    It consists of a list of KSRanks. 
    Each KSRank contains knowledged states, a list of KStates, of the same rank. 
*/

// [TODO]
//
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
    /// Represents a knowledge structure
    /// </summary>
    public class KStructure
    {
        #region Fields

        /// <summary>
        /// The list of ranks in the knowledge structure
        /// </summary>
        private List<KSRank> ranks;
        
        #endregion Fields

        #region Properties

        /// <summary>
        /// A rank order from which the knowledge structure is constructed.
        /// </summary>
        public RankOrder rankOrder { get; set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor initializes an empty list of knowledge structure ranks.
        /// </summary>
        /// 
        /// <param name="rankOrder">RankOrder object that is used to construct the knowledge structure</param>
        public KStructure(RankOrder rankOrder) {
            this.rankOrder = rankOrder;
            this.ranks = new List<KSRank>();
        }

        /// <summary>
        /// Constructor initializes an empty list of knowledge structure ranks and initializes the rankOrder property with null.
        /// </summary>
        public KStructure() : this(null) {
            // [SC] empty constructor body
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Returns true if there is a rankOrder with at least one rank.
        /// </summary>
        /// 
        /// <returns>boolean</returns>
        public bool hasRankOrder() {
            return !(this.rankOrder == null || this.rankOrder.getRankCount() == 0);
        }

        /// <summary>
        /// Returns true if the knowledge structure has at least one rank
        /// </summary>
        /// 
        /// <returns>boolean</returns>
        public bool hasRanks() {
            return !(this.ranks == null || this.getRankCount() == 0);
        }

        /// <summary>
        /// Returns the number of ranks in the knowledge structure.
        /// </summary>
        /// 
        /// <returns>number of ranks</returns>
        public int getRankCount() {
            return this.ranks.Count;
        }

        /// <summary>
        /// Adds a specified rank into the knowledge structure. Afterwards, ranks are sorted by ascending order of rank indices.
        /// </summary>
        /// 
        /// <param name="rank">KSRank object to add into the knowledge structure</param>
        public void addRank(KSRank rank) {
            this.addRank(rank, true);
        }

        /// <summary>
        /// Adds a specified rank into the knowledge structure.
        /// </summary>
        /// 
        /// <param name="rank">     KSRank object to add into the knowledge structure</param>
        /// <param name="sortFlag"> If true, ranks are sorted by ascending order of rank indices after the new rank is added.</param>
        public void addRank(KSRank rank, bool sortFlag) {
            this.ranks.Add(rank);
            if (sortFlag) {
                this.sortAscending();
            }
        }

        /// <summary>
        /// Removes the specified rank from the knowledge structure. Afterwards, ranks are sorted by ascending order of rank indices.
        /// </summary>
        /// 
        /// <param name="rank">KSRank object to be removed from the knowledge structure.</param>
        /// 
        /// <returns>True if the rank was removed successfully.</returns>
        public bool removeRank(KSRank rank) {
            return this.removeRank(rank, true);
        }

        /// <summary>
        /// Removes the specified rank from the knowledge structure.
        /// </summary>
        /// 
        /// <param name="rank">     KSRank object to be removed from the knowledge structure.</param>
        /// <param name="sortFlag"> If true, ranks are sorted by ascending order of rank indices after the new rank is removed</param>
        /// 
        /// <returns>True if the rank was removed successfully.</returns>
        public bool removeRank(KSRank rank, bool sortFlag) {
            if (this.ranks.Remove(rank)) {
                if (sortFlag) {
                    this.sortAscending();
                }

                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Removes a rank at the specified list index. Note that rank's list index is not necessarily same as the rank's index in a knowledge structure.
        /// Afterwards, remaining rank are sorted by ascending order of rank indices.
        /// </summary>
        /// 
        /// <param name="index">List index of a rank to be removed.</param>
        /// 
        /// <returns>True if the rank was removed successfully</returns>
        public bool removeRankAt(int index) {
            return removeRankAt(index, true);
        }

        /// <summary>
        /// Removes a rank at the specified list index. Note that rank's list index is not necessarily same as the rank's index in a knowledge structure.
        /// </summary>
        /// 
        /// <param name="index">    List index of a rank to be removed.</param>
        /// <param name="sortFlag"> If true, ranks are sorted by ascending order of rank indices after the new rank is removed.</param>
        /// 
        /// <returns>True if the rank was removed successfully</returns>
        public bool removeRankAt(int index, bool sortFlag) {
            if (this.getRankCount() > index && index >= 0) {
                this.ranks.RemoveAt(index);

                if (sortFlag) {
                    this.sortAscending();
                }

                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Retrieve KSRank object at specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>KSRank object, or null if index is out of range.</returns>
        public KSRank getRankAt(int index) {
            if (this.getRankCount() > index && index >= 0) {
                return this.ranks[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Returns the list of all ranks in the knowledge structure.
        /// </summary>
        /// 
        /// <returns>List of KSRank objects</returns>
        public List<KSRank> getRanks() {
            return this.ranks;
        }

        /// <summary>
        /// Sorts ranks in the knowledge structure by ascending order of rank indices.
        /// </summary>
        public void sortAscending() { 
            this.ranks.Sort((rankOne, rankTwo) => rankOne.RankIndex.CompareTo(rankTwo.RankIndex));
        }

        #endregion Methods
    }
}
