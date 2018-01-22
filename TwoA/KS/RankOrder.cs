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
Filename: RankOrder.cs
Description:
    Represents a rank order of categories. 
*/

// TODO:
// - In addRank method, verify that a rank with the same rank index does not already exist in the list.
// - Allow retrieval and removal of ranks by rank indices.

// Change history
// [2016.10.18]
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
    /// Represents a rank order from which a knowledge structure can be constructed
    /// </summary>
    public class RankOrder
    {
        #region Fields

        /// <summary>
        /// A list of ranks in this rank order.
        /// </summary>
        private List<Rank> ranks;

        /// <summary>
        /// The value of the threshold used to create the ranked order.
        /// </summary>
        private double threshold;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Getter/setter for the threshold field.
        /// </summary>
        public double Threshold {
            get { return this.threshold; }
            set {
                if (KSGenerator.validThreshold(value)) {
                    this.threshold = value;
                }
                else {
                    throw new System.ArgumentException("Cannot set Threshold value in RankOrder. The value is invalid.");
                }
            } 
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        /// <param name="threshold">threshold used to construct this rank order</param>
        public RankOrder(double threshold) {
            this.ranks = new List<Rank>();
            this.Threshold = threshold;
        }

        /// <summary>
        /// Constructor sets the threshold to a value indicating no assignment.
        /// </summary>
        public RankOrder() : this (KSGenerator.UNASSIGNED_THRESHOLD){
            // [SC] empty constructor body
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns true if threshold was explicitly set to a value.
        /// </summary>
        /// 
        /// <returns>boolean</returns>
        public bool hasThreshold() {
            return !(this.Threshold == KSGenerator.UNASSIGNED_THRESHOLD);
        }

        /// <summary>
        /// Returns the number of ranks in the rank order.
        /// </summary>
        /// 
        /// <returns>interger value</returns>
        public int getRankCount() {
            return this.ranks.Count;
        }

        /// <summary>
        /// Adds a new rank to the rank order. Afterwards, ranks are sorted by indices in an ascending order.
        /// </summary>
        /// 
        /// <param name="rank">Rank object to be added to the rank order</param>
        public void addRank(Rank rank) {
            this.addRank(rank, true);
        }

        /// <summary>
        /// Adds a new rank to the rank order.
        /// </summary>
        /// 
        /// <param name="rank">     Rank object to be added to the rank order.</param>
        /// <param name="sortFlag"> If set to true then ranks are sorted by indices in a ascending order after the new rank was added.</param>
        public void addRank(Rank rank, bool sortFlag) {
            this.ranks.Add(rank);
            if (sortFlag) {
                this.sortAscending();
            }
        }

        /// <summary>
        /// Remove a given rank object from the rank order. Afterwards, ranks are sorted by indices in an ascending order.
        /// </summary>
        /// 
        /// <param name="rank">Rank object to be removed.</param>
        /// 
        /// <returns>True if Rank object was removed successfully.</returns>
        public bool removeRank(Rank rank) {
            return this.removeRank(rank, true);
        }

        /// <summary>
        /// Remove a given rank object from the rank order.
        /// </summary>
        /// 
        /// <param name="rank">     Rank object to be removed.</param>
        /// <param name="sortFlag"> If set to true then ranks are sorted by indices in a ascending order after the new rank was removed.</param>
        /// 
        /// <returns>True if Rank object was removed successfully.</returns>
        public bool removeRank(Rank rank, bool sortFlag) {
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
        /// Removes a rank with a specified list index. Note that rank's list index is not necessarily same as the rank's index in rank order.
        /// Afterwards, remaining ranks are sorted by indices in an ascending order.
        /// </summary>
        /// 
        /// <param name="index">List index of a rank to be removed.</param>
        /// 
        /// <returns>True if a rank was successfully removed.</returns>
        public bool removeRankAt(int index) {
            return this.removeRankAt(index, true);
        }

        /// <summary>
        /// Removes a rank with a specified list index. Note that rank's list index is not necessarily same as the rank's index in rank order.
        /// </summary>
        /// 
        /// <param name="index">    List index of a rank to be removed.</param>
        /// <param name="sortFlag"> If set to true then ranks are sorted by indices in a ascending order after the new rank was removed.</param>
        /// 
        /// <returns>True if a rank was successfully removed.</returns>
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
        /// Retrieve Rank object at specified position in a list (list index).
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>Rank object, or null if index is out of range.</returns>
        public Rank getRankAt(int index) {
            if (this.getRankCount() > index && index >= 0) {
                return this.ranks[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Retrieve the list of all ranks in the rank order.
        /// </summary>
        /// 
        /// <returns>List of all rank objects.</returns>
        public List<Rank> getRanks() {
            return this.ranks;
        }

        /// <summary>
        /// Sorts ranks by rank indices in an ascending order.
        /// </summary>
        public void sortAscending() {
            this.ranks.Sort((rankOne, rankTwo) => rankOne.RankIndex.CompareTo(rankTwo.RankIndex));
        }

        #endregion Methods
    }
}
