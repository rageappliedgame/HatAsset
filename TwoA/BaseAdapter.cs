#region Header

/*
Copyright 2017 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

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
Filename: BaseAdapter.cs
Description:
    [TODO]
*/


// Change history:
// [2017.02.09]
//      - [SC] first created

#endregion Header



namespace TwoA 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AssetPackage;

    internal abstract class BaseAdapter
    {
        #region Constant params

        // [TODO] move these constants to a more appropriate place
        public const double MIN_K_FCT = 0.0075d;
        public const double INITIAL_K_FCT = 0.0375d; // [SC] FIDE range for K 40 for new players until 30 completed games, or as long as their rating remains under 2300; K = 20, for players with a rating always under 2400; K = 10, for players with any published rating of at least 2400 and at least 30 games played in previous events. Thereafter it remains permanently at 10.
        public const double INITIAL_RATING = 0.01d;
        public const double INITIAL_UNCERTAINTY = 1.0d;
        public const double DEFAULT_TIME_LIMIT = 90000; // [SC] in milliseconds

        #endregion Constant params

        #region Fields

        protected TwoA asset; // [ASSET]

        public const string UNASSIGNED_TYPE = "UNASSIGNED"; // [SC] any adapter should have a Type unique among adapters oof TwoA
        public const double ERROR_CODE = -9999;

        public const double DISTR_LOWER_LIMIT = 0.001;     // [SC] lower limit of any probability value
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
        /// Getter for a code indicating error. 
        /// </summary>
        internal static double ErrorCode {
            get { return ERROR_CODE; }
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
