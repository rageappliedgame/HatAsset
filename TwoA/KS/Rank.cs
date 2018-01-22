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
Filename: Rank.cs
Description:
    Represents a single rank in a rank order of categories.
*/

// [TODO]
// - In addCategory method, should check if category is already in the rank.
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
    /// Represents a rank in a rank order
    /// </summary>
    public class Rank
    {
        #region Fields
        
        /// <summary>
        /// Rank index indicating rank's position in a rank order.
        /// </summary>
        private int rankIndex;

        /// <summary>
        /// List of categories that were assigned this rank.
        /// </summary>
        private List<PCategory> categories;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Getter/setter for rankIndex field.
        /// </summary>
        public int RankIndex {
            get {
                return this.rankIndex;
            }
            set {
                if (value == KSGenerator.UNASSIGNED_RANK || value > 0) {
                    this.rankIndex = value;
                }
                else {
                    throw new System.ArgumentException("Rank should be a non-zero positive value.");
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        /// <param name="rankIndex">    Rank index</param>
        /// <param name="categories">   List of categories assigned to this rank</param>
        private Rank(int rankIndex, List<PCategory> categories) { 
            this.categories = categories;
            this.RankIndex = rankIndex;
        }

        /// <summary>
        /// Constructor automatically creates an empty list of categories.
        /// </summary>
        /// 
        /// <param name="rankIndex">Rank index</param>
        public Rank(int rankIndex) : this(rankIndex, new List<PCategory>()) {
            // [SC] empty constructor
        }

        /// <summary>
        /// Constructor automatically creates an empty list of categories and initializes the rankIndex with unassigned indicator.
        /// </summary>
        public Rank() : this(KSGenerator.UNASSIGNED_RANK) {
            // [SC] empty constructor
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns the number of categories in the rank.
        /// </summary>
        /// 
        /// <returns>Number of categories</returns>
        public int getCategoryCount() {
            return this.categories.Count;
        }

        /// <summary>
        /// Adds the specified category into the rank.
        /// </summary>
        /// 
        /// <param name="category">PCategory object to add to the rank</param>
        public void addCategory(PCategory category) {
            this.categories.Add(category);
        }

        /// <summary>
        /// Removes the specified PCategory object from the rank.
        /// </summary>
        /// 
        /// <param name="category">PCategory object to remove.</param>
        /// 
        /// <returns>True if the category was removed successfully.</returns>
        public bool removeCategory(PCategory category) {
            return this.categories.Remove(category);
        }

        /// <summary>
        /// Removes from the rank the category with specified ID.
        /// </summary>
        /// 
        /// <param name="id">ID of the category to remove</param>
        /// 
        /// <returns>True if the category was removed successfully.</returns>
        public bool removeCategory(String id) {
            PCategory remCat = categories.Find(category => category.isSameId(id));
            if (remCat != null) {
                categories.Remove(remCat);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Removes the category at specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>True if a category was removed successfully.</returns>
        public bool removeCategoryAt(int index) {
            if (this.getCategoryCount() > index && index >= 0) {
                this.categories.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="index"></param>
        /// 
        /// <returns></returns>
        public PCategory getCategoryAt(int index) {
            if (this.getCategoryCount() > index && index >= 0) {
                return this.categories[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Returns the list of all categories in the rank.
        /// </summary>
        /// 
        /// <returns>List of categories</returns>
        public List<PCategory> getCategories() {
            return this.categories;
        }

        /// <summary>
        /// Retrieve category with specified ID.
        /// </summary>
        /// 
        /// <param name="id">Category ID</param>
        /// 
        /// <returns>PCategory object</returns>
        public PCategory getCategory(string id) {
            return this.categories.Find(category => category.isSameId(id));
        }

        /// <summary>
        /// A function that finds all unique subsets categories in the rank.
        /// </summary>
        /// 
        /// <returns>List of lists of categories</returns>
        public List<List<PCategory>> getSubsets() {
            List<List<PCategory>> subsets = new List<List<PCategory>>();

            this.getSubsets(new List<PCategory>(), this.categories, subsets);

            return subsets;
        }

        /// <summary>
        /// A recursive function that finds all unique subsets (2^|List|) from a given sourceList.
        /// </summary>
        /// 
        /// <param name="baseList">     Initially an empty list.</param>
        /// <param name="sourceList">   Initially a list of all items to divided into subsets.</param>
        /// <param name="subsets">      Contains all identified subsets.</param>
        public void getSubsets(List<PCategory> baseList, List<PCategory> sourceList, List<List<PCategory>> subsets) {
            for (int index = 0; index < sourceList.Count; index++) {
                List<PCategory> newBaseList = new List<PCategory>(baseList);
                newBaseList.Add(sourceList[index]);
                subsets.Add(newBaseList);

                if (index + 1 < sourceList.Count) {
                    List<PCategory> newSourceList = sourceList.GetRange(index + 1, sourceList.Count - (index + 1));
                    getSubsets(newBaseList, newSourceList, subsets);
                }
            }
        }

        #endregion Methods
    }
}