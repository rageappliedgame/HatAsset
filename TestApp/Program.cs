#region Header

/*
Copyright 2015 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Namespace: TestApp
Filename: Program.cs
*/

// Change history:
// [2016.10.06]
//      - [SC] updated used namespace to TwoA
//      - [SC] renamed variable 'hat' to 'twoA'
// [2016.11.14]
//      - [SC] deleted 'using Swiss';
// [2016.11.29]
//      - [SC] "gameplaylogs.xml" and "TwoAAppSettings.xml" are converted to embedded resources
//      - [SC] updated description of example output for 'UpdateRatings' annd 'TargetScenarioID' methods
//      - [SC] added 'testKnowledgeSpaceGeneration' method with examples of using knowledge space generator

#endregion Header

namespace TestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using System.Diagnostics;

    using TwoA;
    using AssetPackage;

    class Program
    {
        static void Main (string[] args) {
            testAdaptationAndAssessment();
            printMsg("############################################################################");
            testKnowledgeSpaceGeneration();

            Console.ReadKey();
        }

        static void testKnowledgeSpaceGeneration() {
            // [SC] creating an instance of knowledge structure generator
            KSGenerator ksg = new KSGenerator(null, 0.4);

            // [SC] creating a list of rated categories that will be used to generate a knowledge structure
            // [SC] a category represents a set of problems/scenarios that have same difficulty and structure
            List<PCategory> categories = new List<PCategory>();
            categories.Add(new PCategory("a", 3.391156));
            categories.Add(new PCategory("b", 24.182423));
            categories.Add(new PCategory("c", 24.313351));
            categories.Add(new PCategory("d", 32.193103));
            categories.Add(new PCategory("e", 35.618040));
            categories.Add(new PCategory("f", 37.046992));
            categories.Add(new PCategory("g", 45.166948));

            // [SC] first step is to create a rank order from the list of categories
            RankOrder ro = ksg.createRankOrder(categories);

            // [SC] second step is to create a knowledge structure using previsouly created rank order
            KStructure ks = ksg.createKStructure(ro);

            // [SC] third optional step is to create an expanded knowledge structure by identifying additional knowledge states
            KStructure ksExpand = ksg.createKStructure(ro);
            ksExpand = ksg.createExpandedKStructure(ksExpand);

            //////////////////////////////////////////////////////////////////////////////////////////////
            // [SC] note that KStructure object can be serialized into an XML format
            
            // [SC] first, create XML factory singleton
            XMLFactory xmlFct = XMLFactory.Instance;
            // [SC] next, creating XML document from KStructue object
            XDocument xmlDoc = xmlFct.createXml(ks);
            // [SC] XML document can be further serialized into a string (e.g., to store in a file)
            string xmlTxt = xmlFct.serialize(xmlDoc);
            // [SC] finally, XML document can be deserialized into a KStructure object
            KStructure deserKS = xmlFct.createKStructure(xmlDoc);

            //////////////////////////////////////////////////////////////////////////////////////////////
            // [SC] to visualize results, printing RankOrder, KStructure and XML objects into console diagnostic window

            // [SC] printing the rank order
            printMsg("======================================");
            printMsg("Traversing ranks in rank order:\n");
            for (int rankCounter = 0; rankCounter < ro.getRankCount(); rankCounter++) {
                Rank rank = ro.getRankAt(rankCounter);

                printMsg("Current rank: " + rank.RankIndex);

                for(int catCounter = 0; catCounter<rank.getCategoryCount(); catCounter++) {
                    PCategory category = rank.getCategoryAt(catCounter);

                    printMsg("   Category: " + category.Id + "; Rating: " + category.Rating);
                }
            }

            // [SC] printing the knowledge structure
            // [SC] note that all knowledge states in unexpanded knowledge structure have the type of 'core'
            printMsg("\n======================================");
            printMsg("Traversing knowledge states in the unexpanded knowledge structure:\n");
            foreach (KSRank rank in ks.getRanks()) {
                printMsg("Rank " + rank.RankIndex);
                foreach (KState state in rank.getStates()) {
                    printMsg("    Current state: " + state.ToString() + "; State type: " + state.StateType + "; State ID: " + state.Id);
                    foreach (KState prevState in state.getPrevStates()) {
                        printMsg("        Prev state: " + prevState.ToString());
                    }
                    foreach (KState nextState in state.getNextStates()) {
                        printMsg("        Next state: " + nextState.ToString());
                    }
                }
            }

            // [SC] printing the expanded knowledge structure
            // [SC] note that all new states added to the expanded structure have the type of 'expanded'
            printMsg("\n======================================");
            printMsg("Traversing knowledge states in the expanded knowledge structure:\n");
            foreach (KSRank rank in ksExpand.getRanks()) {
                printMsg("Rank " + rank.RankIndex);
                foreach (KState state in rank.getStates()) {
                    printMsg("    Current state: " + state.ToString() + "; State type: " + state.StateType + "; State ID: " + state.Id);
                    foreach (KState prevState in state.getPrevStates()) {
                        printMsg("        Prev state: " + prevState.ToString());
                    }
                    foreach (KState nextState in state.getNextStates()) {
                        printMsg("        Next state: " + nextState.ToString());
                    }
                }
            }

            // [SC] printing the xml document
            // [SC] the XML document consists of three main elements:
            //  - PCategories: this element contains a list of rated categories
            //  - RankOrder: this element contains deserialization of the RankOrder object that was used to create the knowledge structure
            //  - KStructure: this element contains the deserialization of the knowledge structure
            printMsg("\n======================================");
            printMsg("The XML document:\n");
            printMsg(xmlTxt);

            #region example output
            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console/Debug output should resemble (not exactly the same since there is some randomness in the asset) the output below
            //
            //======================================
            //Traversing ranks in rank order:
            //
            //Current rank: 1
            //   Category: a; Rating: 3.391156
            //Current rank: 2
            //   Category: b; Rating: 24.182423
            //   Category: c; Rating: 24.313351
            //Current rank: 3
            //   Category: d; Rating: 32.193103
            //Current rank: 4
            //   Category: e; Rating: 35.61804
            //   Category: f; Rating: 37.046992
            //Current rank: 5
            //   Category: g; Rating: 45.166948
            //
            //======================================
            //Traversing knowledge states in the unexpanded knowledge structure:
            //
            //Rank 0
            //    Current state: (); State type: root; State ID: S0.1
            //        Next state: (a)
            //Rank 1
            //    Current state: (a); State type: core; State ID: S1.1
            //        Prev state: ()
            //        Next state: (a,c)
            //        Next state: (a,b)
            //Rank 2
            //    Current state: (a,c); State type: core; State ID: S2.1
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //    Current state: (a,b); State type: core; State ID: S2.2
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //Rank 3
            //    Current state: (a,b,c); State type: core; State ID: S3.1
            //        Prev state: (a,c)
            //        Prev state: (a,b)
            //        Next state: (a,b,c,d)
            //Rank 4
            //    Current state: (a,b,c,d); State type: core; State ID: S4.1
            //        Prev state: (a,b,c)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,b,c,d,e)
            //Rank 5
            //    Current state: (a,b,c,d,f); State type: core; State ID: S5.1
            //        Prev state: (a,b,c,d)
            //        Next state: (a,b,c,d,e,f)
            //    Current state: (a,b,c,d,e); State type: core; State ID: S5.2
            //        Prev state: (a,b,c,d)
            //        Next state: (a,b,c,d,e,f)
            //Rank 6
            //    Current state: (a,b,c,d,e,f); State type: core; State ID: S6.1
            //        Prev state: (a,b,c,d,f)
            //        Prev state: (a,b,c,d,e)
            //        Next state: (a,b,c,d,e,f,g)
            //Rank 7
            //    Current state: (a,b,c,d,e,f,g); State type: core; State ID: S7.1
            //        Prev state: (a,b,c,d,e,f)
            //
            //======================================
            //Traversing knowledge states in the expanded knowledge structure:
            //
            //Rank 0
            //    Current state: (); State type: root; State ID: S0.1
            //        Next state: (a)
            //Rank 1
            //    Current state: (a); State type: core; State ID: S1.1
            //        Prev state: ()
            //        Next state: (a,c)
            //        Next state: (a,b)
            //Rank 2
            //    Current state: (a,c); State type: core; State ID: S2.1
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //        Next state: (a,c,d)
            //    Current state: (a,b); State type: core; State ID: S2.2
            //        Prev state: (a)
            //        Next state: (a,b,c)
            //        Next state: (a,b,d)
            //Rank 3
            //    Current state: (a,b,c); State type: core; State ID: S3.1
            //        Prev state: (a,c)
            //        Prev state: (a,b)
            //        Next state: (a,b,c,d)
            //    Current state: (a,c,d); State type: expanded; State ID: S3.2
            //        Prev state: (a,c)
            //        Next state: (a,b,c,d)
            //        Next state: (a,c,d,e)
            //        Next state: (a,c,d,f)
            //    Current state: (a,b,d); State type: expanded; State ID: S3.3
            //        Prev state: (a,b)
            //        Next state: (a,b,c,d)
            //        Next state: (a,b,d,e)
            //        Next state: (a,b,d,f)
            //Rank 4
            //    Current state: (a,b,c,d); State type: core; State ID: S4.1
            //        Prev state: (a,b,c)
            //        Prev state: (a,c,d)
            //        Prev state: (a,b,d)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,b,c,d,e)
            //    Current state: (a,c,d,e); State type: expanded; State ID: S4.2
            //        Prev state: (a,c,d)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,c,d,e,f)
            //        Next state: (a,c,d,e,g)
            //    Current state: (a,b,d,e); State type: expanded; State ID: S4.3
            //        Prev state: (a,b,d)
            //        Next state: (a,b,c,d,e)
            //        Next state: (a,b,d,e,f)
            //        Next state: (a,b,d,e,g)
            //    Current state: (a,c,d,f); State type: expanded; State ID: S4.4
            //        Prev state: (a,c,d)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,c,d,e,f)
            //        Next state: (a,c,d,f,g)
            //    Current state: (a,b,d,f); State type: expanded; State ID: S4.5
            //        Prev state: (a,b,d)
            //        Next state: (a,b,c,d,f)
            //        Next state: (a,b,d,e,f)
            //        Next state: (a,b,d,f,g)
            //Rank 5
            //    Current state: (a,b,c,d,f); State type: core; State ID: S5.1
            //        Prev state: (a,b,c,d)
            //        Prev state: (a,c,d,f)
            //        Prev state: (a,b,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,c,d,f,g)
            //    Current state: (a,b,c,d,e); State type: core; State ID: S5.2
            //        Prev state: (a,b,c,d)
            //        Prev state: (a,c,d,e)
            //        Prev state: (a,b,d,e)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,c,d,e,g)
            //    Current state: (a,c,d,e,f); State type: expanded; State ID: S5.3
            //        Prev state: (a,c,d,e)
            //        Prev state: (a,c,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,c,d,e,f,g)
            //    Current state: (a,b,d,e,f); State type: expanded; State ID: S5.4
            //        Prev state: (a,b,d,e)
            //        Prev state: (a,b,d,f)
            //        Next state: (a,b,c,d,e,f)
            //        Next state: (a,b,d,e,f,g)
            //    Current state: (a,c,d,e,g); State type: expanded; State ID: S5.5
            //        Prev state: (a,c,d,e)
            //        Next state: (a,b,c,d,e,g)
            //        Next state: (a,c,d,e,f,g)
            //    Current state: (a,b,d,e,g); State type: expanded; State ID: S5.6
            //        Prev state: (a,b,d,e)
            //        Next state: (a,b,c,d,e,g)
            //        Next state: (a,b,d,e,f,g)
            //    Current state: (a,c,d,f,g); State type: expanded; State ID: S5.7
            //        Prev state: (a,c,d,f)
            //        Next state: (a,b,c,d,f,g)
            //        Next state: (a,c,d,e,f,g)
            //    Current state: (a,b,d,f,g); State type: expanded; State ID: S5.8
            //        Prev state: (a,b,d,f)
            //        Next state: (a,b,c,d,f,g)
            //        Next state: (a,b,d,e,f,g)
            //Rank 6
            //    Current state: (a,b,c,d,e,f); State type: core; State ID: S6.1
            //        Prev state: (a,b,c,d,f)
            //        Prev state: (a,b,c,d,e)
            //        Prev state: (a,c,d,e,f)
            //        Prev state: (a,b,d,e,f)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,c,d,f,g); State type: expanded; State ID: S6.2
            //        Prev state: (a,b,c,d,f)
            //        Prev state: (a,c,d,f,g)
            //        Prev state: (a,b,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,c,d,e,g); State type: expanded; State ID: S6.3
            //        Prev state: (a,b,c,d,e)
            //        Prev state: (a,c,d,e,g)
            //        Prev state: (a,b,d,e,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,c,d,e,f,g); State type: expanded; State ID: S6.4
            //        Prev state: (a,c,d,e,f)
            //        Prev state: (a,c,d,e,g)
            //        Prev state: (a,c,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //    Current state: (a,b,d,e,f,g); State type: expanded; State ID: S6.5
            //        Prev state: (a,b,d,e,f)
            //        Prev state: (a,b,d,e,g)
            //        Prev state: (a,b,d,f,g)
            //        Next state: (a,b,c,d,e,f,g)
            //Rank 7
            //    Current state: (a,b,c,d,e,f,g); State type: core; State ID: S7.1
            //        Prev state: (a,b,c,d,e,f)
            //        Prev state: (a,b,c,d,f,g)
            //        Prev state: (a,b,c,d,e,g)
            //        Prev state: (a,c,d,e,f,g)
            //        Prev state: (a,b,d,e,f,g)
            //
            //======================================
            //The XML document:
            //
            //<?xml version="1.0" encoding="utf-16" standalone="yes"?>
            //<TwoA xmlns="https://github.com/rageappliedgame/HatAsset" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
            //  <PCategories>
            //    <PCategory xsd:id="a">
            //      <Rating>3.391156</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="b">
            //      <Rating>24.182423</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="c">
            //      <Rating>24.313351</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="d">
            //      <Rating>32.193103</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="e">
            //      <Rating>35.61804</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="f">
            //      <Rating>37.046992</Rating>
            //    </PCategory>
            //    <PCategory xsd:id="g">
            //      <Rating>45.166948</Rating>
            //    </PCategory>
            //  </PCategories>
            //  <RankOrder>
            //    <Params>
            //      <Threshold>0.4</Threshold>
            //    </Params>
            //    <Ranks>
            //      <Rank Index="1">
            //        <PCategory xsd:idref="a" />
            //      </Rank>
            //      <Rank Index="2">
            //        <PCategory xsd:idref="b" />
            //        <PCategory xsd:idref="c" />
            //      </Rank>
            //      <Rank Index="3">
            //        <PCategory xsd:idref="d" />
            //      </Rank>
            //      <Rank Index="4">
            //        <PCategory xsd:idref="e" />
            //        <PCategory xsd:idref="f" />
            //      </Rank>
            //      <Rank Index="5">
            //        <PCategory xsd:idref="g" />
            //      </Rank>
            //    </Ranks>
            //  </RankOrder>
            //  <KStructure>
            //    <KSRank Index="0">
            //      <KState xsd:id="S0.1" Type="root">
            //        <PCategories />
            //        <PreviousStates />
            //        <NextStates>
            //          <KState xsd:idref="S1.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="1">
            //      <KState xsd:id="S1.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S0.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S2.1" />
            //          <KState xsd:idref="S2.2" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="2">
            //      <KState xsd:id="S2.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="c" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S1.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S3.1" />
            //        </NextStates>
            //      </KState>
            //      <KState xsd:id="S2.2" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S1.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S3.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="3">
            //      <KState xsd:id="S3.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S2.1" />
            //          <KState xsd:idref="S2.2" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S4.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="4">
            //      <KState xsd:id="S4.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S3.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S5.1" />
            //          <KState xsd:idref="S5.2" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="5">
            //      <KState xsd:id="S5.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="f" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S4.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S6.1" />
            //        </NextStates>
            //      </KState>
            //      <KState xsd:id="S5.2" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S4.1" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S6.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="6">
            //      <KState xsd:id="S6.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //          <PCategory xsd:idref="f" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S5.1" />
            //          <KState xsd:idref="S5.2" />
            //        </PreviousStates>
            //        <NextStates>
            //          <KState xsd:idref="S7.1" />
            //        </NextStates>
            //      </KState>
            //    </KSRank>
            //    <KSRank Index="7">
            //      <KState xsd:id="S7.1" Type="core">
            //        <PCategories>
            //          <PCategory xsd:idref="a" />
            //          <PCategory xsd:idref="b" />
            //          <PCategory xsd:idref="c" />
            //          <PCategory xsd:idref="d" />
            //          <PCategory xsd:idref="e" />
            //          <PCategory xsd:idref="f" />
            //          <PCategory xsd:idref="g" />
            //        </PCategories>
            //        <PreviousStates>
            //          <KState xsd:idref="S6.1" />
            //        </PreviousStates>
            //        <NextStates />
            //      </KState>
            //    </KSRank>
            //  </KStructure>
            //</TwoA>
            //
            #endregion example output
        }

        static void testAdaptationAndAssessment () {
            TwoA twoA = new TwoA(new MyBridge());

            string adaptID = "Game difficulty - Player skill";
            string gameID = "TileZero";
            string playerID = "Noob"; // [SC] using this player as an example
            string scenarioID = "Hard AI"; // [SC] using this scenario as an example

            // [SC] set this variable to true if you want changes saved to "TwoAAppSettings.xml" and "gameplaylogs.xml" files
            // [SC] in this example, it is set to false since the two xml files are embedded resources
            bool updateDatafiles = false;

            ////////////////////////////////////////////////////////////////

            printMsg("\nExample player parameters: ");
            printPlayerData(twoA, adaptID, gameID, playerID);

            ////////////////////////////////////////////////////////////////

            printMsg("\nExample scenario parameters: ");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            ////////////////////////////////////////////////////////////////

            // [SC] Asking for the recommendations for the player 'Noob'.
            // [SC] Among 10 requests, the most frequent recommendation should be the scenario 'Hard AI'.
            // [SC] 'Hard AI' scenario is recommended since it has a rating closest to the player's rating
            printMsg(String.Format("\nAsk 10 times for a recommended scenarios for the player {0}: ", playerID));
            for (int count = 0; count < 10; count++) {
                printMsg("    " + twoA.TargetScenarioID(adaptID, gameID, playerID));
            }

            ////////////////////////////////////////////////////////////////

            printMsg("\nFirst simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: ");
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 120000, 1, updateDatafiles);
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("\n");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg("\nSecond simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: ");
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 230000, 1, updateDatafiles);
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("\n");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            printMsg("\nThird simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: ");
            twoA.UpdateRatings(adaptID, gameID, playerID, scenarioID, 12000, 0, updateDatafiles);
            printPlayerData(twoA, adaptID, gameID, playerID);
            printMsg("\n");
            printScenarioData(twoA, adaptID, gameID, scenarioID);

            #region example output
            ///////////////////////////////////////////////////////////////////////
            // [SC] the Console/Debug output should resemble (not exactly the same since there is some randomness in the asset) the output below
            //
            //Example player parameters: 
            //    PlayerID: Noob
            //    Rating: 5.5
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //
            //Example scenario parameters: 
            //    ScenarioID: Hard AI
            //    Rating: 6
            //    PlayCount: 100
            //    KFactor: 0.0075
            //    Uncertainty: 0.01
            //    LastPlayed: 2012-12-31T11:59:59
            //    TimeLimit: 900000
            //
            //Ask 10 times for a recommended scenarios for the player Noob: 
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Hard AI
            //    Medium Shape AI
            //    Hard AI
            //    Hard AI
            //
            //First simulated gameplay. Player performed well. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.53437762105702
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.96562237894298
            //    PlayCount: 101
            //    KFactor: 0.03335625
            //    Uncertainty: 0.985
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            //Second simulated gameplay. Player performed well again. Player rating increases and scenario rating decreases: 
            //    PlayerID: Noob
            //    Rating: 5.56336425733209
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.93663574266791
            //    PlayCount: 102
            //    KFactor: 0.0327
            //    Uncertainty: 0.96
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            //Third simulated gameplay. Player performed poorly. Player rating decreass and scenario rating increases: 
            //    PlayerID: Noob
            //    Rating: 5.5356982136711
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2016-11-29T11:52:55
            //
            //    ScenarioID: Hard AI
            //    Rating: 5.9643017863289
            //    PlayCount: 103
            //    KFactor: 0.03204375
            //    Uncertainty: 0.935
            //    LastPlayed: 2016-11-29T11:52:55
            //    TimeLimit: 900000
            //
            #endregion example output
        }

        // [2016.11.14] modified
        // [SC] just a helper method
        static void printPlayerData(TwoA twoA, string adaptID, string gameID, string playerID) {
            printMsg("    PlayerID: " + playerID);
            printMsg("    Rating: " + twoA.PlayerRating(adaptID, gameID, playerID));
            printMsg("    PlayCount: " + twoA.PlayerPlayCount(adaptID, gameID, playerID));
            printMsg("    KFactor: " + twoA.PlayerKFactor(adaptID, gameID, playerID));
            printMsg("    Uncertainty: " + twoA.PlayerUncertainty(adaptID, gameID, playerID));
            printMsg("    LastPlayed: " + twoA.PlayerLastPlayed(adaptID, gameID, playerID).ToString(TwoA.DATE_FORMAT));
        }

        // [2016.11.14] modified
        // [SC] just a helper method
        static void printScenarioData(TwoA twoA, string adaptID, string gameID, string scenarioID) {
            printMsg("    ScenarioID: " + scenarioID);
            printMsg("    Rating: " + twoA.ScenarioRating(adaptID, gameID, scenarioID));
            printMsg("    PlayCount: " + twoA.ScenarioPlayCount(adaptID, gameID, scenarioID));
            printMsg("    KFactor: " + twoA.ScenarioKFactor(adaptID, gameID, scenarioID));
            printMsg("    Uncertainty: " + twoA.ScenarioUncertainty(adaptID, gameID, scenarioID));
            printMsg("    LastPlayed: " + twoA.ScenarioLastPlayed(adaptID, gameID, scenarioID).ToString(TwoA.DATE_FORMAT));
            printMsg("    TimeLimit: " + twoA.ScenarioTimeLimit(adaptID, gameID, scenarioID));
        }

        static void printMsg(string msg) {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }

    // [SC][2016.11.29] modified
    class MyBridge : IBridge, IDataStorage
    {
        // [SC] "TwoAAppSettings.xml" and "gameplaylogs.xml" are embedded resources
        // [SC] these XML files are for running this test only and contain dummy data
        // [SC] to use the TwoA asset with your game, generate blank XML files with the accompanying widget https://github.com/rageappliedgame/HATWidget
        private const string resourceNS = "TestApp.Resources.";

        public MyBridge() {}

        public bool Exists(string fileId) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            return resourceNames.Contains<string>(resourceNS + fileId);
        }

        public void Save(string fileId, string fileData) {
            // [SC] save is not implemented since the xml files are embedded resources
        }

        public string Load(string fileId) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceNS + fileId)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public String[] Files() {
            return null;
        }

        public bool Delete(string fileId) {
            return false;
        }
    }
}
