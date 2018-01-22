		static void demoAdaptationAndAssessmentWithCalibration(string adaptID) {
            // [SC] each row is a gameplay
            // [SC] column 1: scenario; column 2: accuracy; column 3: expected player rating; column 4: scenario rating
            double[,] performance = {
                {1, 1, 0.1153, 0.0117},
                {1, 1, 0.2102, -0.0832},
                {4, 1, 0.3663, 1.3239},
                {1, 1, 0.4442, -0.1611},
                {1, 1, 0.5148, -0.2317},
                {4, 1, 0.6532, 1.1855},
                {4, 1, 0.7792, 1.0595},
                {4, 1, 0.8931, 0.9456},
                {4, 0, 0.7958, 1.0429},
                {4, 1, 0.9081, 0.9306},
                {4, 1, 1.0092, 0.8295},
                {4, 1, 1.1002, 0.7385},
                {4, 0, 0.9823, 0.8564},
                {4, 1, 1.076, 0.7626},
                {4, 1, 1.1605, 0.6782},
                {4, 1, 1.2368, 0.6019},
                {5, 0, 1.1761, 2.1268},
                {4, 0, 1.0481, 0.7298},
                {5, 0, 0.9974, 2.1775},
                {4, 1, 1.0841, 0.6431},
                {4, 0, 0.9624, 0.7648},
                {4, 1, 1.0525, 0.6746},
                {4, 1, 1.1339, 0.5933},
                {4, 1, 1.2075, 0.5197},
                {4, 1, 1.2744, 0.4528},
                {5, 0, 1.2167, 2.2352},
                {4, 1, 1.2803, 0.3892},
                {4, 0, 1.1384, 0.5311},
                {5, 0, 1.0884, 2.2853},
                {4, 1, 1.1612, 0.4582},
                {4, 1, 1.2274, 0.392},
                {4, 0, 1.0879, 0.5315},
                {5, 0, 1.0415, 2.3317},
                {4, 1, 1.1166, 0.4565},
                {5, 1, 1.2708, 2.1774},
                {4, 0, 1.1322, 0.5951},
                {4, 1, 1.206, 0.5213}/*,
                {4, 1, 1.273, 0.4543},
                {5, 0, 1.2154, 2.235},
                {4, 1, 1.2791, 0.3906},
                {4, 0, 1.1374, 0.5323},
                {4, 1, 1.208, 0.4617},
                {4, 1, 1.2723, 0.3973},
                {5, 1, 1.417, 2.0903},
                {5, 0, 1.3495, 2.1579},
                {5, 0, 1.2878, 2.2195},
                {5, 0, 1.2313, 2.276},
                {5, 0, 1.1793, 2.3281},
                {4, 1, 1.2421, 0.3345},
                {4, 0, 1.0996, 0.477 }*/
            };

            string gameID = "TileZero";
            string playerID = "EvolvingAI";             // [SC] using this player as an example
            bool updateBetas = true;          // [SC] alwyas update scenario ratings
            DateTime lastPlayed = DateTime.ParseExact("2012-12-31T11:59:59", TwoA.DATE_FORMAT, null);

            TwoA twoA = new TwoA(new MyBridge());

            twoA.SetTargetDistribution(adaptID, 0.5, 0.1, 0.25, 0.75);
            twoA.SetPlayerCalLength(adaptID, 20);
            twoA.SetScenarioCalLength(adaptID, 10);

            double[] betas = {
                        - 0.384
                        , 0.117
                        , 1.48
                        , 2.066
            };
            string[] scenarios = {
                    "Very Easy AI"
                    , "Easy AI"
                    , "Hard AI"
                    , "Very Hard AI"
            };

            twoA.AddPlayer(adaptID, gameID, playerID, 0.01, 0, 0.0075, 1, lastPlayed);

            for (int index = 0; index < scenarios.Length; index++) {
                twoA.AddScenario(adaptID, gameID, scenarios[index], betas[index], 0, 0.0075, 0, lastPlayed, 900000);
            }

            for (int index = 0; index < performance.GetLength(0); index++) {
                // [SC] retrieving RT, accuracy and expected rating
                string scenarioID = getScenarioByNumId(performance[index, 0]);
                double accuracy = performance[index, 1];
                double expectTheta = performance[index, 2];
                double expectBeta = performance[index, 3];

                PlayerNode playerNode = twoA.Player(adaptID, gameID, playerID);
                ScenarioNode scenarioNode = twoA.Scenario(adaptID, gameID, scenarioID);

                double thetaBefore = Math.Round(playerNode.Rating, 4);
                double betaBefore = Math.Round(scenarioNode.Rating, 4);

                twoA.Log(Severity.Information, "");

                // [SC] update player's and scenario's ratings
                twoA.UpdateRatings(playerNode, scenarioNode, 40000, accuracy, updateBetas, 0);

                double thetaAfter = Math.Round(playerNode.Rating, 4);
                double betaAfter = Math.Round(scenarioNode.Rating, 4);

                // [SC] print update results
                twoA.Log(Severity.Information, "Gameplay " + (index + 1) + " against " + scenarioID + ".");
                twoA.Log(Severity.Information, "    Before theta: " + thetaBefore + "; After theta: " + thetaAfter + "; Diff: " + Math.Abs(thetaBefore - thetaAfter));
                twoA.Log(Severity.Information, "    Before beta: " + betaBefore + "; After beta: " + betaAfter + "; Diff: " + Math.Abs(betaBefore - betaAfter));
            }
        }

        public static string getScenarioByNumId(double scenarioNum) {
            switch (scenarioNum) {
                case 0: return "Very Easy AI";
                case 1: return "Easy AI";
                case 2: return "Medium Color AI";
                case 3: return "Medium Shape AI";
                case 4: return "Hard AI";
                case 5: return "Very Hard AI";
                default: throw new Exception("Unknown scenario ID");
            }
        }

        static void testCal(string type) {
            TwoA twoA = new TwoA();

            /////////////////////////////////////////////////////////////////////

            printCalData(twoA, type);

            twoA.SetPlayerCalK(type, 0.8);

            printMsg("\nPlayer cal K is 0.8");
            printCalData(twoA, type);

            twoA.SetPlayerCalK(type, 0);
            twoA.SetPlayerCalK(type, -0.1);

            printMsg("\nPlayer cal K is 0.8");
            printCalData(twoA, type);

            /////////////////////////////////////////////////////////////////////

            twoA.SetScenarioCalK(type, 0.5);

            printMsg("\nScenario cal K is 0.5");
            printCalData(twoA, type);

            twoA.SetScenarioCalK(type, 0);
            twoA.SetScenarioCalK(type, -0.2);

            printMsg("\nScenario cal K is 0.5");
            printCalData(twoA, type);

            /////////////////////////////////////////////////////////////////////

            twoA.SetPlayerCalLength(type, 10);

            printMsg("\nPlayer cal length is 10");
            printCalData(twoA, type);

            twoA.SetPlayerCalLength(type, 0);
            twoA.SetPlayerCalLength(type, -5);

            printMsg("\nPlayer cal length is 0");
            printCalData(twoA, type);

            /////////////////////////////////////////////////////////////////////

            twoA.SetScenarioCalLength(type, 0);

            printMsg("\nScenario cal length is 0");
            printCalData(twoA, type);

            twoA.SetScenarioCalLength(type, 20);
            twoA.SetScenarioCalLength(type, -8);

            printMsg("\nScenario cal length is 20");
            printCalData(twoA, type);

            /////////////////////////////////////////////////////////////////////

            twoA.SetDefaultPlayerCalK(type);

            printMsg("\nDefault player cal K");
            printCalData(twoA, type);

            twoA.SetDefaultScenarioCalK(type);

            printMsg("\nDefault scenario cal K");
            printCalData(twoA, type);

            twoA.SetCalK(type, 0.5);

            printMsg("\nPlayer and Scenario cal K are 0.5");
            printCalData(twoA, type);

            twoA.SetDefaultCalK(type);

            printMsg("\nPlayer and Scenario cal K are default");
            printCalData(twoA, type);

            /////////////////////////////////////////////////////////////////////

            twoA.SetDefaultPlayerCalLength(type);

            printMsg("\nDefault player cal length");
            printCalData(twoA, type);

            twoA.SetDefaultScenarioCalLength(type);

            printMsg("\nDefault scenario cal length");
            printCalData(twoA, type);

            twoA.SetCalLength(type, 15);

            printMsg("\nPlayer and Scenario cal length are 15");
            printCalData(twoA, type);

            twoA.SetDefaultCalLength(type);

            printMsg("\nPlayer and Scenario cal length are default");
            printCalData(twoA, type);
        }

        static void printCalData(TwoA twoA, string type) {
            printMsg("Player cal K: " + twoA.GetPlayerCalK(type));
            printMsg("Scenario cal K: " + twoA.GetScenarioCalK(type));
            printMsg("Player cal length: " + twoA.GetPlayerCalLength(type));
            printMsg("Scenario cal length: " + twoA.GetScenarioCalLength(type));
        }