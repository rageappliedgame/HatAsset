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
Filename: KSGenerator.cs
Description:
    The main class for the package for generating a knowledge structure from category difficulty ratings.
*/

// [TODO]
// in createKStructure method, optimization may be required; too many loops
//

// Change history
// [2016.10.06]
//      - [SC] First created
//

#endregion Header

namespace TwoA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AssetPackage;

    /// <summary>
    /// The main class for generating a knowledge structure from diifficulty ratings.
    /// </summary>
    public class KSGenerator
    {
        #region Fields

        private TwoA asset; // [SC] refers to the main asset object

        /// <summary>
        /// The value is used to indicate that a rating was not assigned a valid value.
        /// </summary>
        public const double UNASSIGNED_RATING = -9999.99;
        
        /// <summary>
        /// The value is used to indicate that a rank was not assigned a valid index.
        /// </summary>
        public const int UNASSIGNED_RANK = -1;

        /// <summary>
        /// The value is used to indicate that a threshold was not assigned a valid value.
        /// </summary>
        public const double UNASSIGNED_THRESHOLD = -1;

        /// <summary>
        /// A default value for a threshold.
        /// </summary>
        public const double DEFAULT_THRESHOLD = 0.1;
        /// <summary>
        /// Min valid value of a threshold (inclusive).
        /// </summary>
        public const double MIN_THRESHOLD = 0;
        /// <summary>
        /// Max valid value of a threshold (exclusive).
        /// </summary>
        public const double MAX_THRESHOLD = 1;

        /// <summary>
        /// Default sameness probability.
        /// </summary>
        public const double DEFAULT_SAME_PROBABILITY = 0.5;
        /// <summary>
        /// Min valid sameness probability.
        /// </summary>
        public const double MIN_SAME_PROBABILITY = 0;
        /// <summary>
        /// Max valid sameness probability.
        /// </summary>
        public const double MAX_SAME_PROBABILITY = 1;

        /// <summary>
        /// State type: 'root'
        /// </summary>
        public const string ROOT_STATE = "root";
        /// <summary>
        /// State type: 'core'
        /// </summary>
        public const string CORE_STATE = "core";
        /// <summary>
        /// State type: 'expanded'
        /// </summary>
        public const string EXPANDED_STATE = "expanded";

        private double threshold; // [SC] current threshold value used to construct KS
        private double sameProbability; // [SC] it is strongly advised to use the value DEFAULT_SAME_PROBABILITY

        #endregion Fields

        #region Properties

        /// <summary>
        /// getter/setter for threshold variable
        /// </summary>
        public double Threshold {
            get {
                return this.threshold;
            }
            set {
                if (KSGenerator.validThreshold(value)) {
                    this.threshold = value;
                }
                else {
                    throw new System.ArgumentException("Threshold value should have range [" + KSGenerator.MIN_THRESHOLD + ", " + KSGenerator.MAX_THRESHOLD + ").");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for sameProbability variable.
        /// It is strongly advised not to change it from the default value. The setter is provide for future uses where a different estimation algorithm might be used.
        /// </summary>
        public double SameProbability {
            get {
                return this.sameProbability;
            }
            set {
                if (value >= KSGenerator.MIN_SAME_PROBABILITY && value <= KSGenerator.MAX_SAME_PROBABILITY) {
                    this.sameProbability = value;
                }
                else {
                    throw new System.ArgumentException("Same probability should have range ["
                                                        + KSGenerator.MIN_SAME_PROBABILITY + ","
                                                        + KSGenerator.MAX_SAME_PROBABILITY + "].");
                }
            }
        }

        /// <summary>
        /// Getter/setter for the instance of the TwoA asset
        /// </summary>
        public TwoA Asset {
            get { return asset; }
            set { this.asset = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the TwoA.KSGenerator class with a custom threshold.
        /// </summary>
        /// 
        /// <param name="asset">        The asset. </param>
        /// <param name="threshold">    A custom threshold value. </param>
        public KSGenerator(TwoA asset, double threshold) {
            this.asset = asset;
            this.Threshold = threshold;
            this.SameProbability = KSGenerator.DEFAULT_SAME_PROBABILITY;
        }

        /// <summary>
        /// Initializes a new instance of the TwoA.KSGenerator class with a default threshold value.
        /// </summary>
        /// 
        /// <param name="asset">The asset.</param>
        public KSGenerator(TwoA asset) : this (asset, KSGenerator.DEFAULT_THRESHOLD) {
            // [SC] blank constructor body
        }

        #endregion Constructors

        #region Methods for creating a knowledge structure

        /// <summary>
        /// Expands the specified knowledge structure with new states by applying Rule 2.
        /// </summary>
        /// 
        /// <param name="ks">Knowledge structure to be expanded with new states.</param>
        /// 
        /// <returns>Expanded knowledge structure</returns>
        public void createExpandedKStructure(KStructure ks) {
            // Implements Rule 2:  Given a set GR of problems with a rank R and a set GR-1 of problems with rank R-1, 
            // a union of any subset of GR with any knowledge state KR-1 containing at least one problem from GR-1 is a state.

            // [SC] make sure the knowledge structure object is not null
            if (ks == null) {
                Log(Severity.Error, "createExpandedKStructure: KStructure object is null. Returning from method.");
                return;
            }

            // [SC] make sure the rank order of categories is available
            if (!ks.hasRankOrder()) {
                Log(Severity.Error, "createExpandedKStructure: KStructure object contains no rank order. Returning from method.");
                return;
            }

            // [SC] make sure the knowledge structure has ranks
            if (!ks.hasRanks()) {
                Log(Severity.Error, "createExpandedKStructure: KStructure object contains no ranks with states. Returning from method.");
                return;
            }

            Rank prevRank = null;
            foreach (Rank rank in ks.rankOrder.getRanks()) {
                if (prevRank != null) {
                    // [SC] getting all unique subsets of categories in this rank
                    List<List<PCategory>> subsets = rank.getSubsets();

                    // [SC] retrieve all KS ranks with minimum required state size
                    List<KSRank> ksRanks = ks.getRanks().FindAll(rankOne => rankOne.RankIndex >= prevRank.RankIndex);

                    if (ksRanks == null || ksRanks.Count == 0) {
                        continue;
                    }

                    // [SC] this list contains all relevant states that contain any category from GR-1
                    List<KState> states = new List<KState>();
                    foreach (KSRank ksRank in ksRanks) {
                        // [SC] From given KS rank, retrieve all states that contain at least one problem from GR-1 and add them to the common list
                        states.AddRange(ksRank.getStates().FindAll(stateOne => stateOne.getCategories().Intersect(prevRank.getCategories()).Any()));
                    }

                    if (states.Count == 0) {
                        continue;
                    }

                    // [SC] iterate through subsets of GR
                    foreach (List<PCategory> subset in subsets) {
                        foreach (KState state in states) {
                            // [SC] if state already contains the entire subset then skip it
                            if (state.getCategories().Intersect(subset).Count() == subset.Count) {
                                continue;
                            }

                            // [SC] creating a new state
                            KState newState = new KState(KSGenerator.EXPANDED_STATE);
                            foreach (PCategory category in state.getCategories()) {
                                newState.addCategory(category);
                            }
                            foreach (PCategory category in subset) {
                                newState.addCategory(category);
                            }

                            // [SC] add the new state to the respective rank
                            KSRank newStateRank = ks.getRanks().Find(rankOne => rankOne.RankIndex == newState.getCategoryCount());
                            if (newStateRank.addState(newState)) {
                                newState.Id = KSGenerator.getStateID(newStateRank.RankIndex, newStateRank.getStateCount());

                                // [SC] link the new state with previous states of lower rank
                                KSRank prevStateRank = ks.getRanks().Find(rankOne => rankOne.RankIndex == newState.getCategoryCount() - 1);
                                if (prevStateRank != null) {
                                    foreach (KState prevState in prevStateRank.getStates()) {
                                        if (prevState.isSubsetOf(newState)) {
                                            prevState.addNextState(newState);
                                            newState.addPrevState(prevState);
                                        }
                                    }
                                }

                                // [SC] link the new state with next states of higher rank
                                KSRank nextStateRank = ks.getRanks().Find(rankOne => rankOne.RankIndex == newState.getCategoryCount() + 1);
                                if (nextStateRank != null) {
                                    foreach (KState nextState in nextStateRank.getStates()) {
                                        if (newState.isSubsetOf(nextState)) {
                                            nextState.addPrevState(newState);
                                            newState.addNextState(nextState);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                prevRank = rank;
            }
        }
        
        /// <summary>
        /// Generates a knowledge structure (KStructure object) from a ranked order (RankOrder object) by applying Rule 1.
        /// </summary>
        /// 
        /// <param name="rankOrder">RankOrder object that is used to generate a knowledge structure.</param>
        /// 
        /// <returns>KStructure object, or null if error occured.</returns>
        public KStructure createKStructure(RankOrder rankOrder) {
            // Implements Rule 1: Given a set GR of problems with a rank R and a set G<R of problems of lower ranks, 
            // a union of any subset of GR with G<R is a knowledge state.

            // [SC] make sure the rankOrder object is not null
            if (rankOrder == null) {
                Log(Severity.Error, "createKStructure: Null object is passed as RankOrder parameter. Returning null.");
                return null;
            }

            // [SC] make sure there is at least one rank in the rank order
            if (rankOrder.getRankCount() == 0) {
                Log(Severity.Error, "createKStructure: rank order has no ranks. Returning null.");
                return null;
            }

            // [SC] make sure the ranks are sorted in an ascending order
            rankOrder.sortAscending();
            
            // [SC] creating knowledge states
            List<KState> allStates = new List<KState>();
            List<PCategory> prevCategories = new List<PCategory>();
            foreach (Rank rank in rankOrder.getRanks()) {
                // [SC] getting all unique subsets of categories in this rank
                List<List<PCategory>> subsets = rank.getSubsets();

                // [SC] for each subset, create a knowledge state by combining with all categories of lower ranks
                foreach(List<PCategory> subset in subsets) {
                    KState state = new KState();
                    foreach (PCategory category in prevCategories) {
                        state.addCategory(category);
                    }
                    foreach (PCategory category in subset) {
                        state.addCategory(category);
                    }
                    allStates.Add(state);
                }

                prevCategories.AddRange(rank.getCategories());
            }

            // [SC] sort states by their sizes in an ascending order
            allStates.Sort((stateOne, stateTwo) => stateOne.getCategoryCount().CompareTo(stateTwo.getCategoryCount()));

            // [SC] creating an empty knowledge structure object
            KStructure ks = new KStructure(rankOrder);

            // [SC] creating 0th rank with an empty root state
            int rankIndex = 0;
            int stateCounter = 0; // [SC] used to generate an ID for each state
            KSRank prevRank = null;
            KSRank currRank = new KSRank(rankIndex);  // [SC] the root rank will automatically add an empty root state
            ks.addRank(currRank);

            // [SC] adding all states in respective ranks
            foreach (KState state in allStates) {
                if (state.getCategoryCount() > rankIndex) {
                    stateCounter = 0;
                    prevRank = currRank;
                    currRank = new KSRank(++rankIndex);
                    ks.addRank(currRank);
                }

                if (currRank.addState(state)) {
                    state.Id = KSGenerator.getStateID(rankIndex, ++stateCounter);

                    foreach (KState prevState in prevRank.getStates()) {
                        if (prevState.isSubsetOf(state)) {
                            prevState.addNextState(state);
                            state.addPrevState(prevState);
                        }
                    }
                }
            }

            return ks;
        }

        #endregion Methods for creating a knowledge structure

        #region Methods for creating a rank order

        /// <summary>
        /// Creates a rank order from an array of difficulty ratings. Category IDs are auto generated.
        /// </summary>
        /// 
        /// <param name="ratings">Array of difficulty ratings to be used for generating a rank order.</param>
        /// 
        /// <returns>RankOrder object</returns>
        public RankOrder createRankOrder(double[] ratings) {
            List<PCategory> categories = new List<PCategory>();

            long counter = 0;
            foreach (double betaVal in ratings) {
                categories.Add(new PCategory("" + (++counter), betaVal));
            }

            return this.createRankOrder(categories);
        }

        /// <summary>
        /// Creates a rank order from a list of difficulty ratings. Category IDs are auto generated.
        /// </summary>
        /// 
        /// <param name="ratings">List of difficulty ratings to be used for generating a rank order.</param>
        /// 
        /// <returns>RankObject object</returns>
        public RankOrder createRankOrder(List<double> ratings) {
            List<PCategory> categories = new List<PCategory>();

            long counter = 0;
            foreach (double betaVal in ratings) {
                categories.Add(new PCategory("" + (++counter), betaVal));
            }

            return this.createRankOrder(categories);
        }

        /// <summary>
        /// Creates a rank order from a list of categories with difficulty ratings.
        /// </summary>
        /// 
        /// <param name="categories">List of categories.</param>
        /// 
        /// <returns>RankOrder object</returns>
        public RankOrder createRankOrder(List<PCategory> categories) {
            if (categories.Find(category => !category.hasId()) != null) {
                Log(Severity.Error, "createRankOrder: Cannot create a rank order. Category ID  is missing. Returning null.");
                return null;
            }

            // [SC] sorting by an ascending order of ID
            categories.Sort((catOne, catTwo) => catOne.Id.CompareTo(catTwo.Id));
            
            PCategory prevCat = null;
            foreach (PCategory category in categories) {
                if (!category.hasRating()) {
                    Log(Severity.Error, String.Format("createRankOrder: Cannot create a rank order. Rating for category '{0}' is missing. Returning null.", category.Id));
                    return null;
                }

                if (prevCat != null && prevCat.isSameId(category)) {
                    Log(Severity.Error, String.Format("createRankOrder: Cannot create a rank order. Duplicate category ID is found: '{0}'. Returning null.", category.Id));
                    return null;
                }

                prevCat = category;
            }

            // [SC] sorting by an ascending order of ratings
            categories.Sort((catOne, catTwo) => catOne.Rating.CompareTo(catTwo.Rating));

            // [SC] building ranks
            RankOrder rankOrder = new RankOrder(this.Threshold);
            int rankIndex = 0;
            Rank rank = null;
            PCategory firstCat = null;
            while (categories.Count > 0) {
                PCategory nextCat = categories[0];

                if (firstCat == null || this.isSignificantlyDifferent(firstCat.Rating, nextCat.Rating)) {
                    rank = new Rank(++rankIndex);
                    rankOrder.addRank(rank);

                    firstCat = nextCat;
                }

                rank.addCategory(nextCat);
                categories.Remove(nextCat);
            }

            return rankOrder;
        }

        /// <summary>
        /// returns true if two difficulty ratings are significantly diffferent
        /// </summary>
        /// 
        /// <param name="betaOne">first difficulty rating</param>
        /// <param name="betaTwo">second difficulty rating</param>
        /// 
        /// <returns>boolean</returns>
        private bool isSignificantlyDifferent(double betaOne, double betaTwo) {
            double observedProbability = this.calcDifferenceProbability(betaOne, betaTwo);

            return Math.Abs(this.SameProbability - observedProbability) >= this.Threshold;
        }

        /// <summary>
        /// Calculates probability of difference of two difficulty ratings.
        /// </summary>
        /// 
        /// <param name="betaOne">first difficulty rating</param>
        /// <param name="betaTwo">second difficulty rating</param>
        /// 
        /// <returns>a value in range [0, 0.5) indicating probability in difficulty difference</returns>
        private double calcDifferenceProbability(double betaOne, double betaTwo) {
            return 1 / (Math.Exp(betaOne - betaTwo) + 1);
        }

        #endregion Methods for creating a rank order

        #region Helper methods
        
        /// <summary>
        /// Generates a knowledge state ID based on its rank index and the number of existing states in the same rank
        /// </summary>
        /// 
        /// <param name="rankIndex">states rank index</param>
        /// <param name="stateCounter">the number of existing states in the same rank</param>
        /// 
        /// <returns>state ID as string</returns>
        public static string getStateID(int rankIndex, int stateCounter){
            return "S" + rankIndex + "." + stateCounter;
        }

        /// <summary>
        /// Returns true if threshold value has a valid range, and false otherwise.
        /// </summary>
        /// 
        /// <param name="threshold">threshold value to be verified</param>
        /// 
        /// <returns>boolean</returns>
        public static bool validThreshold(double threshold) {
            if ((threshold >= KSGenerator.MIN_THRESHOLD && threshold < KSGenerator.MAX_THRESHOLD)
                || threshold == KSGenerator.UNASSIGNED_THRESHOLD) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sends a log message to the asset
        /// </summary>
        /// 
        /// <param name="severity"> Message severity type</param>
        /// <param name="logStr">   Log message</param>
        public void Log(Severity severity, string logStr) {
            if (this.asset != null) {
                this.asset.Log(severity, logStr);
            }
        }

        #endregion Helper methods
    }
}