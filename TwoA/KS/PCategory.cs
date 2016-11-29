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
Filename: PCategory.cs
Description:
    Represents a single category of similar problems (e.g., problems with same structure and difficulty).
*/

// [TODO]
//
//

// Change history
// [2016.10.06]
//      - [SC] First created

#endregion Header

namespace TwoA
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a category of problems of the same structure and difficulty
    /// </summary>
    public class PCategory
    {
        #region Properties

        /// <summary>
        /// A unique identifier for the problem category.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Rating of the problem category.
        /// </summary>
        public double Rating { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TwoA.KS.PCategory class.
        /// </summary>
        /// 
        /// <param name="id">       A unique identifier for the problem category. </param>
        /// <param name="rating">   Rating of the problem category. </param>
        public PCategory(string id, double rating) {
            this.Id = id;
            this.Rating = rating;
        }

        /// <summary>
        /// Initializes a new instance of the TwoA.KS.PCategory class.
        /// </summary>
        public PCategory() : this(null, KSGenerator.UNASSIGNED_RATING) {
            // [SC] call to overriden constructor
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns true if the Id property was assigned a valid value.
        /// </summary>
        /// 
        /// <returns> boolean value </returns>
        public bool hasId() {
            return !String.IsNullOrEmpty(this.Id);
        }

        /// <summary>
        /// Returns true if the Rating property was assigned a numerical value.
        /// </summary>
        /// 
        /// <returns> boolean value </returns>
        public bool hasRating() {
            return !(this.Rating == KSGenerator.UNASSIGNED_RATING);
        }

        /// <summary>
        /// returns true if the category's Id is same as the specified Id
        /// </summary>
        /// 
        /// <param name="id">ID to compare to</param>
        /// 
        /// <returns>a boolean value</returns>
        public bool isSameId(string id) {
            return this.Id.Equals(id);
        }

        /// <summary>
        /// returns true if this category has the same Id as the category passed as a parameter
        /// </summary>
        /// 
        /// <param name="category">another category to compare to</param>
        /// 
        /// <returns>a boolean value</returns>
        public bool isSameId(PCategory category) {
            return this.isSameId(category.Id);
        }

        #endregion Methods
    }
}