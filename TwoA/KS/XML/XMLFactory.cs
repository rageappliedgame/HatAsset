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
Filename: XMLFactory.cs
Description:
    A custom serialization library that transforms KStructure object into an XML format using XML.Linq library.
*/

// Change history
// [2016.10.27]
//      - [SC] First created

#endregion Header

namespace TwoA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.IO;

    using AssetPackage;

    /// <summary>
    /// A sealed class to prevent inheritance.
    /// </summary>
    public sealed class XMLFactory
    {
        #region Fields

        /// <summary>
        /// A thread-safe singleton instance.
        /// </summary>
        private static readonly XMLFactory instance = new XMLFactory();

        /// <summary>
        /// Default namespace
        /// </summary>
        public static readonly XNamespace twoa = "https://github.com/rageappliedgame/HatAsset";
        /// <summary>
        /// Standard XSD namespace to be used with 'id' and 'idref' attributes
        /// </summary>
        public static readonly XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// XName for XmlElement("TwoA")
        /// </summary>
        public static readonly XName TWOA_ELEM = XName.Get("TwoA", XMLFactory.twoa.ToString());
        
        /// <summary>
        /// XName for XmlElement("PCategories")
        /// </summary>
        public static readonly XName PCATS_ELEM = XName.Get("PCategories", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("PCategory")
        /// </summary>
        public static readonly XName PCAT_ELEM = XName.Get("PCategory", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("Rating")
        /// </summary>
        public static readonly XName RATING_ELEM = XName.Get("Rating", XMLFactory.twoa.ToString());

        /// <summary>
        /// XName for XmlElement("RankOrder")
        /// </summary>
        public static readonly XName RANKORDER_ELEM = XName.Get("RankOrder", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("Params")
        /// </summary>
        public static readonly XName PARAMS_ELEM = XName.Get("Params", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement(""Threshold"")
        /// </summary>
        public static readonly XName THRESHOLD_ELEM = XName.Get("Threshold", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("Ranks")
        /// </summary>
        public static readonly XName RANKS_ELEM = XName.Get("Ranks", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("Rank")
        /// </summary>
        public static readonly XName RANK_ELEM = XName.Get("Rank", XMLFactory.twoa.ToString());

        /// <summary>
        /// XName for XmlElement("KStructure")
        /// </summary>
        public static readonly XName KSTRUCTURE_ELEM = XName.Get("KStructure", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("KSRank")
        /// </summary>
        public static readonly XName KSRANK_ELEM = XName.Get("KSRank", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("KState")
        /// </summary>
        public static readonly XName KSTATE_ELEM = XName.Get("KState", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("PreviousStates")
        /// </summary>
        public static readonly XName PREV_STATES_ELEM = XName.Get("PreviousStates", XMLFactory.twoa.ToString());
        /// <summary>
        /// XName for XmlElement("NextStates")
        /// </summary>
        public static readonly XName NEXT_STATES_ELEM = XName.Get("NextStates", XMLFactory.twoa.ToString());

        /// <summary>
        /// XName for XmlAttribute("Index")
        /// </summary>
        public static readonly XName INDEX_ATTR = "Index";
        /// <summary>
        /// XName for XmlAttribute("Type")
        /// </summary>
        public static readonly XName TYPE_ATTR = "Type";
        /// <summary>
        /// XName for XmlAttribute("xsd:id")
        /// </summary>
        public static readonly XName ID_ATTR = XName.Get("id", XMLFactory.xsd.ToString());
        /// <summary>
        /// XName for XmlAttribute("xsd:idref")
        /// </summary>
        public static readonly XName IDREF_ATTR = XName.Get("idref", XMLFactory.xsd.ToString());

        /// <summary>
        /// XName for XmlAttribute("xmlns")
        /// </summary>
        public static readonly XName XMLNS_ATTR = "xmlns";
        /// <summary>
        /// XName for XmlAttribute("xmlns:xsd")
        /// </summary>
        public static readonly XName XSD_ATTR = XNamespace.Xmlns + "xsd";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Returns a singleton instance.
        /// </summary>
        public static XMLFactory Instance {
            get {
                return XMLFactory.instance;
            }
        }

        /// <summary>
        /// Instance of the TwoA asset
        /// </summary>
        public TwoA asset {
            get;
            set;
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static XMLFactory() {
            // [SC] empty constructor
        }

        /// <summary>
        /// A private constructor to prevent default instantiation.
        /// </summary>
        private XMLFactory() { 
            // [SC] empty constructor
        }

        #endregion Constructors

        #region Methods
        
        /// <summary>
        /// creates an XML document object from a given KStructure object
        /// </summary>
        /// 
        /// <param name="kStructure">KStructure object with a rank order and knowledge structure</param>
        /// 
        /// <returns>XmlDocument object</returns>
        public XDocument createXml(KStructure kStructure) {

            // [SC] verifying that KStructure object is not null
            if (kStructure == null) {
                Log(Severity.Error, "Unable to transform knowledge structure into XML format. kStructure parameter is null. Returning null.");
                return null;
            }

            // [SC] create xml document object
            XDocument doc = new XDocument();

            // [SC] add xml declaration to the document
            doc.Declaration = new XDeclaration("1.0", "UTF-8", "yes");

            // [SC] add a root element 'TwoA' and declare namespace attributes
            XElement twoAElem = new XElement(XMLFactory.TWOA_ELEM
                , new XAttribute(XMLFactory.XMLNS_ATTR, XMLFactory.twoa)
                , new XAttribute(XMLFactory.XSD_ATTR, XMLFactory.xsd)
            );
            doc.Add(twoAElem);

            // [SC] create a list of categories and a rank order
            if (kStructure.hasRankOrder()) {
                RankOrder rankOrder = kStructure.rankOrder;
                rankOrder.sortAscending();

                // [SC] add 'TwoA/PCategories' list element
                XElement pcatsElem = new XElement(XMLFactory.PCATS_ELEM);
                twoAElem.Add(pcatsElem);

                // [SC] add 'TwoA/RankOrder' element
                XElement rankOrderElem = new XElement(XMLFactory.RANKORDER_ELEM);
                twoAElem.Add(rankOrderElem);

                // [SC] add 'TwoA/RankOrder/Params' list element
                XElement rankParamsElem = new XElement(XMLFactory.PARAMS_ELEM);
                rankOrderElem.Add(rankParamsElem);

                // [SC] add 'TwoA/RankOrder/Params.Threshold' element
                XElement thresholdElem = new XElement(XMLFactory.THRESHOLD_ELEM);
                rankParamsElem.Add(thresholdElem);
                thresholdElem.SetValue("" + rankOrder.Threshold);

                // [SC] create 'TwoA/RankOrder/Ranks' list element
                XElement ranksElem = new XElement(XMLFactory.RANKS_ELEM);
                rankOrderElem.Add(ranksElem);

                // [SC] iterate through ranks and create correspoinding XML elements
                for (int rankCounter = 0; rankCounter < rankOrder.getRankCount(); rankCounter++) {
                    Rank rank = rankOrder.getRankAt(rankCounter);

                    // [SC] add 'TwoA/RankOrder/Ranks/Rank' element
                    XElement rankElem = new XElement(XMLFactory.RANK_ELEM);
                    ranksElem.Add(rankElem);
                    // [SC] add 'TwoA/RankOrder/Ranks/Rank@Index' attribute to the 'Rank' element
                    XAttribute indexAttr = new XAttribute(XMLFactory.INDEX_ATTR, "" + rank.RankIndex);
                    rankElem.Add(indexAttr);

                    // [SC] interate through categories in the rank and create corresponding XML element and reference to it
                    for (int catCounter = 0; catCounter < rank.getCategoryCount(); catCounter++) {
                        PCategory pcat = rank.getCategoryAt(catCounter);

                        // [SC] add 'TwoA/PCategories/PCategory' element with 'xsd:id' attribute
                        XElement pcatElem = new XElement(XMLFactory.PCAT_ELEM);
                        pcatsElem.Add(pcatElem);
                        // [SC] add 'TwoA/PCategories/PCategory@xsd:id' attribute
                        XAttribute idAttr = new XAttribute(XMLFactory.ID_ATTR, "" + pcat.Id);
                        pcatElem.Add(idAttr);
                        // [SC] add 'TwoA/PCategories/PCategory/Rating' element
                        XElement ratingElem = new XElement(XMLFactory.RATING_ELEM);
                        pcatElem.Add(ratingElem);
                        ratingElem.SetValue("" + pcat.Rating);

                        // [SC] add 'TwoA/RankOrder/Ranks/Rank/PCategory' element with 'xsd:idref' attribute
                        XElement pcatRefElem = new XElement(XMLFactory.PCAT_ELEM);
                        rankElem.Add(pcatRefElem);
                        // [SC] add 'TwoA/RankOrder/Ranks/Rank/PCategory@xsd:idref' attribute
                        XAttribute idrefAttr = new XAttribute(XMLFactory.IDREF_ATTR, "" + pcat.Id);
                        pcatRefElem.Add(idrefAttr);
                    }
                }
            }
            else {
                Log(Severity.Warning, "Rank order is missing while transforming KStructure object into XML format.");
            }

            // [SC] creates elements for 'KStructure'
            if (kStructure.hasRanks()) {
                kStructure.sortAscending();

                // [SC] add 'TwoA/KStructure' element
                XElement kStructureElem = new XElement(XMLFactory.KSTRUCTURE_ELEM);
                twoAElem.Add(kStructureElem);

                // [SC] iterate through KSRanks and create corresponding XML elements
                for (int rankCounter = 0; rankCounter < kStructure.getRankCount(); rankCounter++) {
                    KSRank rank = kStructure.getRankAt(rankCounter);

                    // [SC] add 'TwoA/KStructure/KSRank' element
                    XElement ksRankElem = new XElement(XMLFactory.KSRANK_ELEM);
                    kStructureElem.Add(ksRankElem);
                    // [SC] add 'TwoA/KStructure/KSRank@Index' attribute
                    XAttribute indexAttr = new XAttribute(XMLFactory.INDEX_ATTR, "" + rank.RankIndex);
                    ksRankElem.Add(indexAttr);


                    // [SC] iterate through states and add corresponding XML elements
                    for (int stateCounter = 0; stateCounter < rank.getStateCount(); stateCounter++) {
                        KState state = rank.getStateAt(stateCounter);

                        // [SC] add 'TwoA/KStructure/KSRank/KState' element with 'xsd:id' attribute
                        XElement stateElem = new XElement(XMLFactory.KSTATE_ELEM);
                        ksRankElem.Add(stateElem);
                        // [SC] add 'TwoA/KStructure/KSRank/KState@xsd:id' attribute
                        XAttribute idAttr = new XAttribute(XMLFactory.ID_ATTR, "" + state.Id);
                        stateElem.Add(idAttr);
                        // [SC] add 'TwoA/KStructure/KSRank/KState@Type' attribute
                        XAttribute typeAttr = new XAttribute(XMLFactory.TYPE_ATTR, "" + state.StateType);
                        stateElem.Add(typeAttr);

                        // [SC] add 'TwoA/KStructure/KSRank/KState/PCategories' list element
                        XElement pcatsElem = new XElement(XMLFactory.PCATS_ELEM);
                        stateElem.Add(pcatsElem);

                        // [SC] iterate through categories in the state
                        for (int catCounter = 0; catCounter < state.getCategoryCount(); catCounter++) {
                            PCategory pcat = state.getCategoryAt(catCounter);

                            // [SC] add 'TwoA/KStructure/KSRank/KState/PCategories/PCategory' element with 'xsd:idref' attribute
                            XElement pcatElem = new XElement(XMLFactory.PCAT_ELEM);
                            pcatsElem.Add(pcatElem);
                            // [SC] add 'TwoA/KStructure/KSRank/KState/PCategories/PCategory@xsd:idref' attribute
                            XAttribute idrefAttr = new XAttribute(XMLFactory.IDREF_ATTR, "" + pcat.Id);
                            pcatElem.Add(idrefAttr);
                        }

                        // [SC] add 'TwoA/KStructure/KSRank/KState/PreviousStates' list element
                        XElement prevStatesElem = new XElement(XMLFactory.PREV_STATES_ELEM);
                        stateElem.Add(prevStatesElem);

                        // [SC] iterate through immediate prerequisite states in the gradient
                        for (int prevStateCounter = 0; prevStateCounter < state.getPrevStateCount(); prevStateCounter++) {
                            KState prevState = state.getPrevStateAt(prevStateCounter);

                            // [SC] add 'TwoA/KStructure/KSRank/KState/PreviousStates/KState' element with 'xsd:idref' attribute
                            XElement prevStateElem = new XElement(XMLFactory.KSTATE_ELEM);
                            prevStatesElem.Add(prevStateElem);
                            // [SC] add 'TwoA/KStructure/KSRank/KState/PreviousStates/KState@xsd:idref' attribute
                            XAttribute idrefAttr = new XAttribute(XMLFactory.IDREF_ATTR, "" + prevState.Id);
                            prevStateElem.Add(idrefAttr);
                        }

                        // [SC] add 'TwoA/KStructure/KSRank/KState/NextStates' list element
                        XElement nextStatesElem = new XElement(XMLFactory.NEXT_STATES_ELEM);
                        stateElem.Add(nextStatesElem);

                        // [SC] iterate through immediate next states in the gradient
                        for (int nextStateCounter = 0; nextStateCounter < state.getNextStateCount(); nextStateCounter++) {
                            KState nextState = state.getNextStateAt(nextStateCounter);

                            // [SC] add 'TwoA/KStructure/KSRank/KState/NextStates/KState' element with 'xsd:idref' attribute
                            XElement nextStateElem = new XElement(XMLFactory.KSTATE_ELEM);
                            nextStatesElem.Add(nextStateElem);
                            // [SC] add 'TwoA/KStructure/KSRank/KState/NextStates/KState@xsd:idref' attribute
                            XAttribute idrefAttr = new XAttribute(XMLFactory.IDREF_ATTR, "" + nextState.Id);
                            nextStateElem.Add(idrefAttr);
                        }
                    }
                }
            }
            else {
                Log(Severity.Warning, "Knowledge structure is missing while transforming KStructure object into XML format.");
            }

            return doc;
        }

        /// <summary>
        /// Deserializes XML into KStructure object
        /// </summary>
        /// 
        /// <param name="xmlString">XML string</param>
        /// 
        /// <returns>KStructure object</returns>
        public KStructure createKStructure(string xmlString) {
            XDocument doc = XDocument.Parse(xmlString);
            return createKStructure(doc);
        }

        /// <summary>
        /// Deserializes XML into KStructure object
        /// </summary>
        /// 
        /// <param name="doc">XDocument instance</param>
        /// 
        /// <returns>KStructure object</returns>
        public KStructure createKStructure(XDocument doc) {
            // [TODO] validate against schema
            
            XName[] nodeNames;

            // [SC] a hash table of all categories
            Dictionary<string, PCategory> categories = new Dictionary<string, PCategory>();

            // [SC] a hash table of all states
            Dictionary<string, KState> states = new Dictionary<string, KState>();

            // [SC] iterate through 'TwoA/PCategories/PCategory' elements
            nodeNames = new XName[] { XMLFactory.PCATS_ELEM, XMLFactory.PCAT_ELEM };
            foreach (XElement categoryElem in SelectNodes(doc.Root, nodeNames)) {
                // [SC] get the value of 'TwoA/PCategories/PCategory@xsd:id' attribute
                string id = categoryElem.Attribute(XMLFactory.ID_ATTR).Value;

                // [SC] get the value of 'TwoA/PCategories/PCategory/Rating' element
                double rating;
                if (!Double.TryParse(categoryElem.Element(XMLFactory.RATING_ELEM).Value, out rating)) {
                    Log(Severity.Error, String.Format("createKStructure: unable to parse rating for category {0}. Returning null.", id));
                    return null; // [TODO] no need due to schema check?
                }

                PCategory category = new PCategory(id, rating);

                categories.Add(id, category);
            }

            RankOrder rankOrder = new RankOrder();

            // [SC] parse the value of 'TwoA/RankOrder/Params/Threshold' element
            nodeNames = new XName[] { XMLFactory.RANKORDER_ELEM, XMLFactory.PARAMS_ELEM, XMLFactory.THRESHOLD_ELEM };
            double threshold;
            if (Double.TryParse(SelectSingleNode(doc.Root, nodeNames).Value, out threshold)) {
                rankOrder.Threshold = threshold;
            }
            else {
                Log(Severity.Error, "createKStructure: unable to parse the threshold value. Returning null value. Returning null.");
                return null; // [TODO] no need due to schema check?
            }

            // [SC] iterate through 'TwoA/RankOrder/Ranks/Rank' elements
            nodeNames = new XName[] { XMLFactory.RANKORDER_ELEM, XMLFactory.RANKS_ELEM, XMLFactory.RANK_ELEM };
            foreach (XElement rankElem in SelectNodes(doc.Root, nodeNames)) {
                Rank rank = new Rank();

                // [SC] parse the value of 'TwoA/RankOrder/Ranks/Rank@Index' atttribute
                int rankIndex;
                if (Int32.TryParse(rankElem.Attribute(XMLFactory.INDEX_ATTR).Value, out rankIndex)) {
                    rank.RankIndex = rankIndex;
                }
                else {
                    Log(Severity.Error, "createKStructure: unable to parse the index of a rank in the rank order. Returning null.");
                    return null; // [TODO] no need due to schema check?
                }

                // [SC] iterate through 'TwoA/RankOrder/Ranks/Rank/PCategory' elements
                foreach (XElement categoryElem in rankElem.Elements(XMLFactory.PCAT_ELEM)) {
                    // [SC] parse 'TwoA/RankOrder/Ranks/Rank/PCategory@xsd:idref' attribute
                    if (categoryElem.Attribute(XMLFactory.IDREF_ATTR) == null) {
                        Log(Severity.Error, String.Format("createKStructure: unable to parse ID for a category in rank {0} of the rank order. Returning null.", rankIndex));
                        return null; // [TODO] no need due to schema check?
                    }
                    string id = categoryElem.Attribute(XMLFactory.IDREF_ATTR).Value;

                    // [SC] retrieve PCategory object by its id and add it to the rank object
                    PCategory category = categories[id];
                    if (category == null) {
                        Log(Severity.Error
                                , String.Format("createKStructure: category {0} from rank {1} of rank order is not found in the list of categories. Returning null."
                                , id, rankIndex));
                        return null; // [TODO] no need due to schema check?
                    }
                    rank.addCategory(category);
                }

                rankOrder.addRank(rank);
            }

            KStructure kStructure = new KStructure(rankOrder);

            // [SC] iterate through 'TwoA/KStructure/KSRank' elements
            nodeNames = new XName[] { XMLFactory.KSTRUCTURE_ELEM, XMLFactory.KSRANK_ELEM };
            foreach (XElement ksrankElem in SelectNodes(doc.Root, nodeNames)) {
                KSRank ksrank = new KSRank();

                // [SC] parse the value of 'TwoA/KStructure/KSRank@Index' attribute
                int rankIndex;
                if (Int32.TryParse(ksrankElem.Attribute(XMLFactory.INDEX_ATTR).Value, out rankIndex)) {
                    ksrank.RankIndex = rankIndex;
                }
                else {
                    Log(Severity.Error, "createKStructure: unable to parse index of a rank in the knowledge structure. Returning null.");
                    return null; // [TODO] no need due to schema check?
                }


                if (rankIndex == 0) {
                    XElement rootStateElem = ksrankElem.Element(XMLFactory.KSTATE_ELEM);

                    // [SC] parse 'TwoA/KStructure/KSRank/KState@xsd:id' attribute
                    if (rootStateElem.Attribute(XMLFactory.ID_ATTR) == null) {
                        Log(Severity.Error, "createKStructure: unable to parse ID of the root state in the knowledge structure. Returning null.");
                        return null; // [TODO] no need due to schema check?
                    }
                    ksrank.getStateAt(0).Id = rootStateElem.Attribute(XMLFactory.ID_ATTR).Value;

                    states.Add(ksrank.getStateAt(0).Id, ksrank.getStateAt(0));

                    kStructure.addRank(ksrank);

                    continue;
                }

                // [SC] iterate through 'TwoA/KStructure/KSRank/KState' elements
                foreach (XElement stateElem in ksrankElem.Elements(XMLFactory.KSTATE_ELEM)) {
                    KState kstate = new KState();

                    // [SC] parse 'TwoA/KStructure/KSRank/KState@xsd:id' attribute
                    if (stateElem.Attribute(XMLFactory.ID_ATTR) == null) {
                        Log(Severity.Error, String.Format("createKStructure: unable to parse ID of a state in the rank {0} of the knowledge structure. Returning null.", rankIndex));
                        return null; // [TODO] no need due to schema check?
                    }
                    kstate.Id = stateElem.Attribute(XMLFactory.ID_ATTR).Value;

                    // [SC] parse 'TwoA/KStructure/KSRank/KState@Type' attribute
                    if (stateElem.Attribute(XMLFactory.TYPE_ATTR) == null) {
                        Log(Severity.Error, String.Format("createKStructure: unable to parse state type in the rank {0} of the knowledge structure. Returning null.", rankIndex));
                        return null; // [TODO] no need due to schema check?
                    }
                    kstate.StateType = stateElem.Attribute(XMLFactory.TYPE_ATTR).Value;

                    // [SC] iterate through 'TwoA/KStructure/KSRank/KState/PCategories/PCategory' elements
                    nodeNames = new XName[] { XMLFactory.PCATS_ELEM, XMLFactory.PCAT_ELEM };
                    foreach (XElement categoryElem in SelectNodes(stateElem, nodeNames)) {
                        // [SC] parse 'TwoA/KStructure/KSRank/KState/PCategories/PCategory@xsd:idref' attribute
                        if (categoryElem.Attribute(XMLFactory.IDREF_ATTR) == null) {
                            Log(Severity.Error, String.Format("createKStructure: unable to parse ID of a category in the state {0}. Returning null.", kstate.Id));
                            return null; // [TODO] no need due to schema check?
                        }
                        string id = categoryElem.Attribute(XMLFactory.IDREF_ATTR).Value;

                        // [SC] retrieve PCategory object by its id and add it to the rank object
                        PCategory category = categories[id];
                        if (category == null) {
                            Log(Severity.Error
                                , String.Format("createKStructure: category {0} from the state {1} is not found in the list of categories. Returning null."
                                                , id, kstate.Id));
                            return null; // [TODO] no need due to schema check?
                        }
                        kstate.addCategory(category);
                    }

                    // [SC] iterate through 'TwoA/KStructure/KSRank/KState/PreviousStates/KState' elements
                    nodeNames = new XName[] { XMLFactory.PREV_STATES_ELEM, XMLFactory.KSTATE_ELEM };
                    foreach (XElement prevStateElem in SelectNodes(stateElem, nodeNames)) {
                        // [SC] parse 'TwoA/KStructure/KSRank/KState/PreviousStates/KState@xsd:idref' attribute
                        if (prevStateElem.Attribute(XMLFactory.IDREF_ATTR) == null) {
                            Log(Severity.Error, String.Format("createKStructure: unable to parse ID of a previous state for a state {0}. Returning null.", kstate.Id));
                            return null; // [TODO] no need due to schema check?
                        }
                        string id = prevStateElem.Attribute(XMLFactory.IDREF_ATTR).Value;

                        // [SC] retrieve prev state object by its id and add it to the current state object
                        KState prevState = states[id];
                        if (prevState == null) {
                            Log(Severity.Error, String.Format("createKStructure: unable to find previously created state object with id '{0}'. Returning null.", id));
                            return null; // [TODO] no need due to schema check?
                        }
                        kstate.addPrevState(prevState);
                        prevState.addNextState(kstate);
                    }

                    states.Add(kstate.Id, kstate);
                    ksrank.addState(kstate);
                }

                kStructure.addRank(ksrank);
            }

            return kStructure;
        }

        /// <summary>
        /// A helper function that emulates xPath-like method for selecting a list of xml child nodes by name
        /// </summary>
        /// 
        /// <param name="startNode">    Parent node</param>
        /// <param name="nodeNames">    Contains a path to destination child nodes which is the last item in the array</param>
        /// 
        /// <returns>A list of target child nodes, or empty list if child node is not found.</returns>
        public IEnumerable<XElement> SelectNodes(XElement startNode, XName[] nodeNames) {
            IEnumerable<XElement> result = new List<XElement>();
            if (nodeNames == null) { return result; }
            if (nodeNames.Length == 0) { return result; }

            for (int index = 0; index < nodeNames.Length; index++) {
                if (startNode == null) { return result; }

                if (index == nodeNames.Length - 1) {
                    result = startNode.Elements(nodeNames[index]);
                }
                else {
                    startNode = startNode.Element(nodeNames[index]);
                }
            }

            return result;
        }

        /// <summary>
        /// A helper function that emulates xPath-like method for selecting a single xml node by its name
        /// </summary>
        /// 
        /// <param name="startNode">    Parent node</param>
        /// <param name="nodeNames">    Contains a path to destination child nodes which is the last item in the array</param>
        /// 
        /// <returns>Child node, or null if the node is not found</returns>
        public XElement SelectSingleNode(XElement startNode, XName[] nodeNames) {
            IEnumerable<XElement> nodes = SelectNodes(startNode, nodeNames);
            if (nodes == null || nodes.Count() == 0) {
                return null;
            }
            else {
                return nodes.First<XElement>();
            }
        }

        /// <summary>
        /// A helper function that serializes XDocument object into a formatted string.
        /// </summary>
        /// 
        /// <param name="doc">XDocument to be serialized</param>
        /// 
        /// <returns>string</returns>
        public string serialize(XDocument doc) {
            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder)) {
                doc.Save(writer);
            }
            return builder.ToString();
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
        
        #endregion Methods
    }
}
