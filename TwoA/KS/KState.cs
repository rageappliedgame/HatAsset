#region Header

/*
Copyright 2016 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

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
Filename: KState.cs
Description:
    See the description of the KStructure class.
*/

// [TODO]
// in addPrevState method, should check if a state with a similar set of categories already exists
// in addPrevState method, add log msg indicating that root state cannot have previous state
// in addNextState method, should check if a state with a similar set of categories already exists
// in addCategory method, add log msg indicating that root state cannot have categories
// in addCategory method, add log msg indicating that a category with the same id already exists within the state
//

// Change history
// [2016.10.13]
//      - [SC] First created

#endregion Header

namespace TwoA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a knowledge state
    /// </summary>
    public class KState
    {
        #region Fields

        /// <summary>
        /// Type of knowledge state (can be root, core or expanded).
        /// </summary>
        private string stateType;

        /// <summary>
        /// A list of categories that comprise the knowledge state
        /// </summary>
        private List<PCategory> categories;

        /// <summary>
        /// A list of all states that are prerequisite for this state
        /// </summary>
        private List<KState> prevStates;
        /// <summary>
        /// A list of all states this state is prerequisite for
        /// </summary>
        private List<KState> nextStates;

        #endregion Fields

        #region Properties

        /// <summary>
        /// ID for the state
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Getter/setter for the stateType field
        /// </summary>
        public string StateType {
            get {
                return this.stateType;
            }
            set {
                if (!(value.Equals(KSGenerator.ROOT_STATE)
                    || value.Equals(KSGenerator.CORE_STATE)
                    || value.Equals(KSGenerator.EXPANDED_STATE)
                    )) {
                    throw new ArgumentException("Invalid state type.");
                }
                else {
                    this.stateType = value;

                    if (value.Equals(KSGenerator.ROOT_STATE)) {
                        this.prevStates.Clear(); // [SC] root state cannot have prerequisite states
                        this.categories.Clear(); // [SC] root state is always an empty state
                    }
                }
            }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Private constructor
        /// </summary>
        /// 
        /// <param name="id">           A unique ID for the state</param>
        /// <param name="stateType">    Type of the state</param>
        /// <param name="categories">   A list of ategories that comprise the state</param>
        private KState(string id, string stateType, List<PCategory> categories) {
            this.categories = categories;

            this.prevStates = new List<KState>();
            this.nextStates = new List<KState>();

            this.Id = id;
            this.StateType = stateType;
        }

        /// <summary>
        /// Constructor initializes with null ID and an empty list of categories
        /// </summary>
        /// 
        /// <param name="stateType">Type of the state</param>
        public KState(string stateType) : this(null, stateType, new List<PCategory>()) {
            // [SC] empty constructor
        }

        /// <summary>
        /// Constructor initializes with an empty list of categories
        /// </summary>
        /// 
        /// <param name="id">           A unique ID for the state</param>
        /// <param name="stateType">    Type of the state</param>
        public KState(string id, string stateType) : this(id, stateType, new List<PCategory>()) { 
            // [SC] empty constructor
        }

        /// <summary>
        /// Constructor initializes with core state type as default, null ID and an empty list of categories 
        /// </summary>
        public KState() : this(KSGenerator.CORE_STATE) {
            // [SC] empty constructor
        }

        #endregion Constructor

        #region Methods for id

        /// <summary>
        /// Returns true if the Id property was assigned a valid value.
        /// </summary>
        /// 
        /// <returns>boolean</returns>
        public bool hasId() {
            return !String.IsNullOrEmpty(this.Id);
        }

        /// <summary>
        /// returns true if the state's Id is same as the specified Id
        /// </summary>
        /// 
        /// <param name="id">ID to compare to</param>
        /// 
        /// <returns>boolean</returns>
        public bool isSameId(string id) {
            return this.Id.Equals(id);
        }

        /// <summary>
        /// returns true if this state has the same Id as the state passed as a parameter.
        /// </summary>
        /// 
        /// <param name="state">another state to compare to</param>
        /// 
        /// <returns>boolean</returns>
        public bool isSameId(KState state) {
            return this.isSameId(state.Id);
        }

        #endregion Methods for id

        #region Methods for previous states

        /// <summary>
        /// Returns the number of prerequisite states.
        /// </summary>
        /// 
        /// <returns>state count</returns>
        public int getPrevStateCount() {
            return this.prevStates.Count;
        }

        /// <summary>
        /// Adds a specified prerequisite state.
        /// </summary>
        /// 
        /// <param name="prevState">The state to be added to the list of prerequisite states</param>
        public void addPrevState(KState prevState) {
            if (!this.StateType.Equals(KSGenerator.ROOT_STATE)) {
                this.prevStates.Add(prevState);
            }
        }

        /// <summary>
        /// Removes the specified prerequisite state.
        /// </summary>
        /// 
        /// <param name="prevState">A state to be removed from the list of prerequisite states</param>
        /// 
        /// <returns>True if the state was removed successfully.</returns>
        public bool removePrevState(KState prevState) {
            return this.prevStates.Remove(prevState);
        }

        /// <summary>
        /// Removes the prerequisite state at the specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>True if the state was removed successfully</returns>
        public bool removePrevStateAt(int index) {
            if (this.getPrevStateCount() > index && index >= 0) {
                this.prevStates.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the list of all prerequisite states.
        /// </summary>
        /// 
        /// <returns>A list of KState objects</returns>
        public List<KState> getPrevStates() {
            return this.prevStates;
        }

        /// <summary>
        /// Retrieves a prerequisite state at specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>KState object, or null if index is out of range</returns>
        public KState getPrevStateAt(int index) {
            if (this.getPrevStateCount() > index && index >= 0) {
                return this.prevStates[index];
            }
            else {
                return null;
            }
        }

        #endregion Methods for previous states

        #region Methods for next states

        /// <summary>
        /// Returns the number of succeeding states.
        /// </summary>
        /// 
        /// <returns>integer</returns>
        public int getNextStateCount() {
            return this.nextStates.Count;
        }

        /// <summary>
        /// Adds a specified state to the list of succeeding states.
        /// </summary>
        /// 
        /// <param name="nextState">State to be added to the list of succeeding states</param>
        public void addNextState(KState nextState) { 
            this.nextStates.Add(nextState);
        }

        /// <summary>
        /// Removes the specified state from the list of succeeding states.
        /// </summary>
        /// 
        /// <param name="nextState">State to remove</param>
        /// 
        /// <returns>True if the state was removed successfully</returns>
        public bool removeNextState(KState nextState) {
            return this.nextStates.Remove(nextState);
        }

        /// <summary>
        /// Removes a state at specified index in the list of succeeding states.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>True if the state was removed successfully</returns>
        public bool removeNextStateAt(int index) {
            if (this.getNextStateCount() > index && index >= 0) {
                this.nextStates.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the list of all states succeding this state.
        /// </summary>
        /// 
        /// <returns>A list of KState objects</returns>
        public List<KState> getNextStates() {
            return this.nextStates;
        }

        /// <summary>
        /// Retrieve a succeeding state at the specified list index.
        /// </summary>
        /// 
        /// <param name="index">List index</param>
        /// 
        /// <returns>KState object, or null if index is out of range</returns>
        public KState getNextStateAt(int index) {
            if (this.getNextStateCount() > index && index >= 0) {
                return this.nextStates[index];
            }
            else {
                return null;
            }
        }

        #endregion Methods for next states

        #region Methods for categories

        /// <summary>
        /// Get the number of categories in the knowledge state.
        /// </summary>
        /// 
        /// <returns>state count</returns>
        public int getCategoryCount() {
            return this.categories.Count;
        }

        /// <summary>
        /// Add a new category to the knowledge state.
        /// </summary>
        /// 
        /// <param name="newCategory">PCategory objectt</param>
        /// 
        /// <returns>true if category was added; false if a category with the same ID already exists in the list</returns>
        public bool addCategory(PCategory newCategory) {
            // [SC] a root state is always an empty state
            if (this.StateType.Equals(KSGenerator.ROOT_STATE)) {
                return false;
            }

            // [SC] verifying if a category with the same ID already exists
            if (this.categories.Find(category => category.isSameId(newCategory)) == null) {
                this.categories.Add(newCategory);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Remove a give category object.
        /// </summary>
        /// 
        /// <param name="category">category object</param>
        /// 
        /// <returns>true if the category was removed</returns>
        public bool removeCategory(PCategory category) {
            return this.categories.Remove(category);
        }

        /// <summary>
        /// Remove category by its ID.
        /// </summary>
        /// 
        /// <param name="id">category ID</param>
        /// 
        /// <returns>true if a category was removed</returns>
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
        /// Remove category by its list index.
        /// </summary>
        /// 
        /// <param name="index">index in a list</param>
        /// 
        /// <returns>true if a category was removed</returns>
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
        /// Get the list of all categories in the knowledge state.
        /// </summary>
        /// 
        /// <returns>List of PCategory objects</returns>
        public List<PCategory> getCategories() {
            return this.categories;
        }

        /// <summary>
        /// Get a category by its list index.
        /// </summary>
        /// 
        /// <param name="index">index in a list</param>
        /// 
        /// <returns>PCategory object, or null if index is out of range</returns>
        public PCategory getCategoryAt(int index) {
            if (this.getCategoryCount() > index && index >= 0) {
                return categories[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Get a category by it ID.
        /// </summary>
        /// 
        /// <param name="id">category ID</param>
        /// 
        /// <returns>PCategory object or null</returns>
        public PCategory getCategory(string id) {
            return this.categories.Find(category => category.isSameId(id));
        }

        #endregion Methods for categories

        #region Other methods

        /// <summary>
        /// Returns a string representation of this state.
        /// </summary>
        /// 
        /// <returns>string</returns>
        override public string ToString() {
            string name = "(";
            bool sepFlag = false;
            foreach (PCategory cat in this.categories) {
                if (sepFlag) {
                    name += ",";
                }
                else {
                    sepFlag = true;
                }
                name += cat.Id;
            }
            return name + ")";
        }

        /// <summary>
        /// Returns true if the state is subset of a state specified as parameter
        /// </summary>
        /// 
        /// <param name="state">KState object</param>
        /// 
        /// <returns>boolean</returns>
        public bool isSubsetOf(KState state) {
            return this.categories.Intersect(state.getCategories()).Count() == this.categories.Count;
        }

        #endregion Other methods
    }
}