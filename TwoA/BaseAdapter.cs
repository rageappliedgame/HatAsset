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
Filename: BaseAdapter.cs
Description:
    [TODO]
*/


// Change history:
// [2017.02.09]
//      - [SC] first created
// [2017.12.19]
//      - [SC] changed the namespace from TwoA to TwoANS
//      - [SC] changed the access modifier for BaseAdapter class to public from internal
//      - [SC] changed the access modifiers for Type and Description properties to public from internal

#endregion Header

namespace TwoANS 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AssetPackage;

    /// <summary>
    /// Base functionalities for the two modes of adaptation. Do not use this class.
    /// </summary>
    public abstract class BaseAdapter
    {
        #region Constant params

        // [TODO] move these constants to a more appropriate place
        /// <summary>
        /// Minimum value for the K factor.
        /// </summary>
        public const double MIN_K_FCT = 0.0075d;
        /// <summary>
        /// Initial value for the K factor used for adaptation based on accuracy only.
        /// </summary>
        public const double INITIAL_K_FCT = 0.0375d; // [SC] FIDE range for K 40 for new players until 30 completed games, or as long as their rating remains under 2300; K = 20, for players with a rating always under 2400; K = 10, for players with any published rating of at least 2400 and at least 30 games played in previous events. Thereafter it remains permanently at 10.
        /// <summary>
        /// Initial value for the difficulty or skill rating. Should not be equal to 0.
        /// </summary>
        public const double INITIAL_RATING = 0.01d;
        /// <summary>
        /// Initial value for the rating uncertainty.
        /// </summary>
        public const double INITIAL_UNCERTAINTY = 1.0d;
        /// <summary>
        /// Default time limit for finishing a scenario. Measured in milliseconds.
        /// </summary>
        public const double DEFAULT_TIME_LIMIT = 90000; // [SC] in milliseconds

        #endregion Constant params

        #region Fields

        /// <summary>
        /// Reference to the instance of the encapsulating asset class.
        /// </summary>
        protected TwoA asset; // [ASSET]

        /// <summary>
        /// This value is returned if the adaptation module was not given Type property.
        /// </summary>
        public const string UNASSIGNED_TYPE = "UNASSIGNED"; // [SC] any adapter should have a Type unique among adapters oof TwoA
        /// <summary>
        /// An error code as double.
        /// </summary>
        public const double ERROR_CODE = -9999;
        /// <summary>
        /// An error code as integer.
        /// </summary>
        public const int ERROR_CODE_INT = -9999;

        /// <summary>
        /// Lower limit of any probability value.
        /// </summary>
        public const double DISTR_LOWER_LIMIT = 0.001;     // [SC] lower limit of any probability value
        /// <summary>
        /// Upper limit of any probability value.
        /// </summary>
        public const double DISTR_UPPER_LIMIT = 0.999;     // [SC] upper limit of any probability value

        #endregion Fields 

        #region Consts, Fields, Properties

        /// <summary>
        /// Gets the type of the adapter; It needs to be overriden by inheriting classes
        /// </summary>
        internal static string Type {
            get { return UNASSIGNED_TYPE; }
        }

        /// <summary>
        /// Description of this adapter. It needs to be overriden by inheriting classes
        /// </summary>
        internal static string Description {
            get { return UNASSIGNED_TYPE; }
        }

        /// <summary>
        /// Getter for a double code indicating error. 
        /// </summary>
        internal static double ErrorCode {
            get { return ERROR_CODE; }
        }

        /// <summary>
        /// Getter for an integer code indicating error. 
        /// </summary>
        internal static int ErrorCodeInt {
            get { return ERROR_CODE_INT; }
        }

        /// <summary>
        /// Lower limit of a normal distribution with mean in interval (0, 1)
        /// </summary>
        internal static double DistrLowerLimit {
            get { return DISTR_LOWER_LIMIT; }
        }

        /// <summary>
        /// Upper limit of a normal distribution with mean in interval (0,1)
        /// </summary>
        internal static double DistrUpperLimit {
            get { return DISTR_UPPER_LIMIT;  }
        }

        #endregion Const, Fields, Properties

        #region Constructors

        internal BaseAdapter(TwoA asset) {
            this.asset = asset;
        }

        #endregion Constructors

        //////////////////////////////////////////////////////////////////////////////////////
        ////// START: misc methods
        #region misc methods

        /// <summary>
        /// Logs a message by default under a Severity.Information type
        /// </summary>
        /// 
        /// <param name="msg">      A message to be logged</param>
        internal void log(string msg) {
            log(Severity.Information, msg);
        }

        /// <summary>
        /// Logs a message using assets's Log method
        /// </summary>
        /// 
        /// <param name="severity"> Message type</param>
        /// <param name="msg">      A message to be logged</param>
        internal void log(Severity severity, string msg) {
            if (asset != null) {
                asset.Log(severity, msg);
            }
        }

        #endregion misc methods
        ////// END: misc methods
        //////////////////////////////////////////////////////////////////////////////////////
    }
}
