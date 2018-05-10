namespace Grom.Tests
{
    using Grom;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    //[TestClass]
    public class DynamicNodeTests
    {
        private dynamic wrapper;

        [TestInitialize]
        public void Initialize()
        {
            var graph = Helper.Load("<http://example.com/subject1> <http://example.com/predicate1> <http://example.com/object1> .");
            graph.NamespaceMap.AddNamespace(string.Empty, new Uri("http://example.com/"));

            this.wrapper = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("http://example.com/"),
                true);
        }

        [TestMethod]
        [DynamicData(nameof(Data.IndexParameters), typeof(Data))]
        public void Get_index(object index)
        {
            Assert.AreEqual(
                new NodeWrapper(new NodeFactory().CreateUriNode(new Uri("http://example.com/object1"))),
                this.wrapper[index]);
        }

        [TestMethod]
        public void Get_index_array()
        {
            var graph = Helper.Load(@"
<http://example.com/subject1> <http://example.com/predicate1> ""0"" .
<http://example.com/subject1> <http://example.com/predicate1> ""1"" .
");

            dynamic wrapper = new NodeWrapper(graph.Nodes.First());

            Assert.AreEqual(
                "0",
                wrapper["http://example.com/predicate1"][0]);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Get_index_fails_invalid_uri()
        {
            var result = this.wrapper["http:///"];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Get_index_fails_multidimensional()
        {
            var result = this.wrapper[0, 0];
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Get_index_fails_relative_uri_without_base()
        {
            dynamic node = new NodeWrapper(
                new NodeFactory().CreateBlankNode(),
                baseUri: null);

            var result = node[new Uri("predicate1", UriKind.Relative)];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Get_index_fails_unsupported_type()
        {
            var result = this.wrapper[0];
        }

        [TestMethod]
        public void Get_index_hash_base()
        {
            var graph = Helper.Load("<http://example.com/x#subject1> <http://example.com/x#predicate1> <http://example.com/x#object1>.");

            dynamic wrapper = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("http://example.com/x#"),
                true);

            Assert.AreEqual(
                new NodeWrapper(graph.CreateUriNode(new Uri("http://example.com/x#object1"))),
                wrapper["predicate1"]);
        }

        [TestMethod]
        public void Get_member()
        {
            var graph = Helper.Load("<http://example.com/subject1> <http://example.com/predicate1> <http://example.com/object1> .");

            dynamic wrapper = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("http://example.com/"),
                true);

            Assert.AreEqual(
                new NodeWrapper(graph.CreateUriNode(new Uri("http://example.com/object1"))),
                wrapper.predicate1);
        }

        [TestMethod]
        [DynamicData(nameof(Data.MemberNames), typeof(Data))]
        public void Get_member_names(string predicate, Uri baseUri, string memberName)
        {
            var graph = Helper.Load($"<http://example.com/1> <{predicate}> <http://example.com/1> .");

            var wrapper = new NodeWrapper(
                graph.Nodes.First(),
                baseUri);

            CollectionAssert.AreEqual(
                new[] { memberName },
                wrapper.GetDynamicMemberNames().ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Get_member_fails_without_base()
        {
            dynamic wrapper = new NodeWrapper(
                new NodeFactory().CreateBlankNode());

            var result = wrapper.predicate1;
        }

        [TestMethod]
        [DynamicData(nameof(Data.TypedLiteralConversions), typeof(Data))]
        public void Typed_literals_create_strongly_typed_values(string value, Uri datatype, Type expected)
        {
            var graph = new Graph();

            var subject = graph.CreateBlankNode();
            var predicate = graph.CreateUriNode("rdf:type");

            graph.Assert(
                subject,
                predicate,
                graph.CreateLiteralNode(value, datatype));

            dynamic node = new NodeWrapper(
                subject,
                collapseSingularArrays: true);

            Assert.IsInstanceOfType(
                node[predicate],
                expected);
        }

        [TestMethod]
        [DynamicData(nameof(Data.TypedValueConversions), typeof(Data))]
        public void Strongly_typed_values_create_typed_literals(object value, Type expectedType)
        {
            var graph = new Graph();

            var subject = graph.CreateBlankNode();
            var predicate = graph.CreateUriNode(new Uri("http://example.com/"));

            graph.Assert(
                subject,
                predicate,
                subject);

            dynamic node = new NodeWrapper(
                subject,
                collapseSingularArrays: true);

            node[predicate] = value;

            var actual = graph.Triples.Single().Object.AsValuedNode();

            Assert.IsInstanceOfType(
                actual,
                expectedType);
        }

        [TestMethod]
        public void Set_member2()
        {
            var graph = Helper.Load(@"
<http://example.com/s1> <http://example.com/p> <http://example.com/o1> .
<http://example.com/s2> <http://example.com/p> <http://example.com/o2> .
");

            dynamic g = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"));

            g.s1.p = g.o2;

            Assert.AreEqual(graph.Triples.First().Object, graph.Triples.Last().Object);
        }

        [TestMethod]
        public void Set_member3()
        {
            var graph = Helper.Load(@"
<http://example.com/s> <http://example.com/p> <http://example.com/o1> .
");

            dynamic g = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"));

            var newObjectUri = new Uri("http://example.com/o2");

            g.s.p = newObjectUri;
            
            Assert.AreEqual(
                graph.CreateUriNode(newObjectUri),
                graph.Triples.Single().Object);
        }

        [TestMethod]
        public void Set_member4()
        {
            var graph = Helper.Load("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            dynamic g = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"));

            g.s.p = null;

            Assert.IsTrue(graph.IsEmpty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Set_index_fails_multidimensional()
        {
            var result = this.wrapper[0, 0] = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Set_member_fails_without_base()
        {
            dynamic wrapper = new NodeWrapper(
                new NodeFactory().CreateBlankNode());

            wrapper.predicate1 = null;
        }

        //[TestMethod]
        public void PersonByIdNode()
        {
            var a = @"
<https://id.parliament.uk/43RHonMf> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/Person> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personGivenName> ""Diane"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personOtherNames> ""Julie"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personFamilyName> ""Abbott"" .
<https://id.parliament.uk/43RHonMf> <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ""Ms Diane Abbott"" .
<https://id.parliament.uk/43RHonMf> <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ""Rt Hon Diane Abbott MP"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasMemberImage> <https://id.parliament.uk/S3bGSTqn> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/UcFeoI5t> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/5b1mxVJ7> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/VHM0rBq1> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/wE8Hq016> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personPimsId> ""3572"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personMnisId> ""172"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasPersonalWebLink> <http://www.dianeabbott.org.uk/> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasTwitterWebLink> <https://twitter.com/HackneyAbbott> .
<https://id.parliament.uk/S3bGSTqn> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/MemberImage> .
<https://id.parliament.uk/HSoMS1VX> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupEndDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2005-05-05+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2001-06-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/L71LLeyL> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/L71LLeyL> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/HSoMS1VX> .
<https://id.parliament.uk/L71LLeyL> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/LEYIBvV9> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/Party> .
<https://id.parliament.uk/LEYIBvV9> <https://id.parliament.uk/schema/partyName> ""Labour"" .
<https://id.parliament.uk/UcFeoI5t> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/UcFeoI5t> <https://id.parliament.uk/schema/partyMembershipStartDate> ""2017-06-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UcFeoI5t> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/1AFu55Hs> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/House> .
<https://id.parliament.uk/1AFu55Hs> <https://id.parliament.uk/schema/houseName> ""House of Commons"" .
<https://id.parliament.uk/VHM0rBq1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1995-11-15+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1997-03-21+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/cLjFRjRt> .
<https://id.parliament.uk/cLjFRjRt> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/cLjFRjRt> <https://id.parliament.uk/schema/formalBodyName> ""Treasury Committee"" .
<https://id.parliament.uk/wE8Hq016> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/incumbencyStartDate> ""2016-06-27+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/incumbencyEndDate> ""2016-10-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/skJwXTDQ> .
<https://id.parliament.uk/skJwXTDQ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/skJwXTDQ> <https://id.parliament.uk/schema/positionName> ""Shadow Secretary of State for Health"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/2SdmzfSs> .
<https://id.parliament.uk/2SdmzfSs> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/2SdmzfSs> <https://id.parliament.uk/schema/incumbencyStartDate> ""2016-10-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/2SdmzfSs> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/yYN7V6yX> .
<https://id.parliament.uk/yYN7V6yX> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/yYN7V6yX> <https://id.parliament.uk/schema/positionName> ""Shadow Home Secretary"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/ZiqjL3DQ> .
<https://id.parliament.uk/ZiqjL3DQ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/incumbencyStartDate> ""2010-10-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/incumbencyEndDate> ""2013-10-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/3YwE0NhM> .
<https://id.parliament.uk/3YwE0NhM> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/3YwE0NhM> <https://id.parliament.uk/schema/positionName> ""Shadow Minister (Public Health)"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/xi9CFNH9> .
<https://id.parliament.uk/xi9CFNH9> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/incumbencyStartDate> ""2015-09-14+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/incumbencyEndDate> ""2016-06-27+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/b8cXZPwI> .
<https://id.parliament.uk/b8cXZPwI> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/b8cXZPwI> <https://id.parliament.uk/schema/positionName> ""Shadow Secretary of State for International Development"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/uzym0vom> .
<https://id.parliament.uk/uzym0vom> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1997-11-25+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1998-11-19+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/HKmf0Ca7> .
<https://id.parliament.uk/HKmf0Ca7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/HKmf0Ca7> <https://id.parliament.uk/schema/formalBodyName> ""Foreign Affairs: Entry Clearance Sub-Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/IsEMoAUF> .
<https://id.parliament.uk/IsEMoAUF> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1989-05-17+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1995-11-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/wXqxbB2N> .
<https://id.parliament.uk/wXqxbB2N> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/wXqxbB2N> <https://id.parliament.uk/schema/formalBodyName> ""Treasury & Civil Service Sub-Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/EYQCxVic> .
<https://id.parliament.uk/EYQCxVic> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1989-05-15+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1995-11-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/ZSJXcWE1> .
<https://id.parliament.uk/ZSJXcWE1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/ZSJXcWE1> <https://id.parliament.uk/schema/formalBodyName> ""Treasury & Civil Service"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/UXpLpmfS> .
<https://id.parliament.uk/UXpLpmfS> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1997-07-16+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""2001-05-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/fYbWHWhk> .
<https://id.parliament.uk/fYbWHWhk> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/fYbWHWhk> <https://id.parliament.uk/schema/formalBodyName> ""Foreign Affairs Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/GmWQzr0b> .
<https://id.parliament.uk/GmWQzr0b> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipStartDate> ""1987-06-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipEndDate> ""2015-03-30+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/JWf4RycJ> .
<https://id.parliament.uk/JWf4RycJ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipStartDate> ""2015-05-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipEndDate> ""2017-05-03+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/P8dV98n8> .
<https://id.parliament.uk/fy1cWNCD> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ContactPoint> .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/email> ""diane.abbott.office@parliament.uk"" .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/phoneNumber> ""020 7219 4426"" .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/contactPointHasPostalAddress> <https://id.parliament.uk/uBuD8ZqA> .
<https://id.parliament.uk/uBuD8ZqA> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PostalAddress> .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/addressLine1> ""House of Commons"" .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/addressLine2> ""London"" .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/postCode> ""SW1A 0AA"" .
<https://id.parliament.uk/5bX5Se0u> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/5bX5Se0u> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/5bX5Se0u> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/P8dV98n8> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2017-06-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/parliamentaryIncumbencyHasContactPoint> <https://id.parliament.uk/fy1cWNCD> .
<https://id.parliament.uk/0BhROnYP> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/0BhROnYP> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/5bX5Se0u> .
<https://id.parliament.uk/0BhROnYP> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/FC1IDBx2> .
<https://id.parliament.uk/FC1IDBx2> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2001-06-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/PfWrzef7> .
<https://id.parliament.uk/PfWrzef7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2005-05-05+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/TuxAdhNO> .
<https://id.parliament.uk/p869pBTf> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""1983-06-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupEndDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1992-04-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/8khvuL6U> .
<https://id.parliament.uk/8khvuL6U> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/8khvuL6U> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/p869pBTf> .
<https://id.parliament.uk/8khvuL6U> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/9jEv9wQr> .
<https://id.parliament.uk/9jEv9wQr> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2015-03-30+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/yVNL0a8Z> .
<https://id.parliament.uk/yVNL0a8Z> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""1992-04-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1987-06-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/8khvuL6U> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/DqWmAFwg> .
<https://id.parliament.uk/DqWmAFwg> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2017-05-03+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2015-05-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
";

            var graph = Helper.Load(a);

            dynamic wrapper = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("https://id.parliament.uk/schema/"));

            var c = wrapper.memberHasMemberImage[0];
        }

        //[TestMethod]
        public void SerializationTest()
        {
            var rdf = @"
<https://id.parliament.uk/43RHonMf> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/Person> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personGivenName> ""Diane"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personOtherNames> ""Julie"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personFamilyName> ""Abbott"" .
<https://id.parliament.uk/43RHonMf> <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ""Ms Diane Abbott"" .
<https://id.parliament.uk/43RHonMf> <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ""Rt Hon Diane Abbott MP"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasMemberImage> <https://id.parliament.uk/S3bGSTqn> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/UcFeoI5t> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/5b1mxVJ7> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/VHM0rBq1> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/wE8Hq016> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personPimsId> ""3572"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personMnisId> ""172"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasPersonalWebLink> <http://www.dianeabbott.org.uk/> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasTwitterWebLink> <https://twitter.com/HackneyAbbott> .
<https://id.parliament.uk/S3bGSTqn> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/MemberImage> .
<https://id.parliament.uk/HSoMS1VX> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/HSoMS1VX> <https://id.parliament.uk/schema/constituencyGroupEndDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2005-05-05+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2001-06-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/5b1mxVJ7> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/L71LLeyL> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/L71LLeyL> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/HSoMS1VX> .
<https://id.parliament.uk/L71LLeyL> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/LEYIBvV9> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/Party> .
<https://id.parliament.uk/LEYIBvV9> <https://id.parliament.uk/schema/partyName> ""Labour"" .
<https://id.parliament.uk/UcFeoI5t> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/UcFeoI5t> <https://id.parliament.uk/schema/partyMembershipStartDate> ""2017-06-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UcFeoI5t> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/1AFu55Hs> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/House> .
<https://id.parliament.uk/1AFu55Hs> <https://id.parliament.uk/schema/houseName> ""House of Commons"" .
<https://id.parliament.uk/VHM0rBq1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1995-11-15+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1997-03-21+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/VHM0rBq1> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/cLjFRjRt> .
<https://id.parliament.uk/cLjFRjRt> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/cLjFRjRt> <https://id.parliament.uk/schema/formalBodyName> ""Treasury Committee"" .
<https://id.parliament.uk/wE8Hq016> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/incumbencyStartDate> ""2016-06-27+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/incumbencyEndDate> ""2016-10-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/wE8Hq016> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/skJwXTDQ> .
<https://id.parliament.uk/skJwXTDQ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/skJwXTDQ> <https://id.parliament.uk/schema/positionName> ""Shadow Secretary of State for Health"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/2SdmzfSs> .
<https://id.parliament.uk/2SdmzfSs> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/2SdmzfSs> <https://id.parliament.uk/schema/incumbencyStartDate> ""2016-10-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/2SdmzfSs> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/yYN7V6yX> .
<https://id.parliament.uk/yYN7V6yX> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/yYN7V6yX> <https://id.parliament.uk/schema/positionName> ""Shadow Home Secretary"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/ZiqjL3DQ> .
<https://id.parliament.uk/ZiqjL3DQ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/incumbencyStartDate> ""2010-10-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/incumbencyEndDate> ""2013-10-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/ZiqjL3DQ> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/3YwE0NhM> .
<https://id.parliament.uk/3YwE0NhM> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/3YwE0NhM> <https://id.parliament.uk/schema/positionName> ""Shadow Minister (Public Health)"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/oppositionPersonHasOppositionIncumbency> <https://id.parliament.uk/xi9CFNH9> .
<https://id.parliament.uk/xi9CFNH9> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionIncumbency> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/incumbencyStartDate> ""2015-09-14+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/incumbencyEndDate> ""2016-06-27+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/xi9CFNH9> <https://id.parliament.uk/schema/oppositionIncumbencyHasOppositionPosition> <https://id.parliament.uk/b8cXZPwI> .
<https://id.parliament.uk/b8cXZPwI> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/OppositionPosition> .
<https://id.parliament.uk/b8cXZPwI> <https://id.parliament.uk/schema/positionName> ""Shadow Secretary of State for International Development"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/uzym0vom> .
<https://id.parliament.uk/uzym0vom> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1997-11-25+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1998-11-19+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/uzym0vom> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/HKmf0Ca7> .
<https://id.parliament.uk/HKmf0Ca7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/HKmf0Ca7> <https://id.parliament.uk/schema/formalBodyName> ""Foreign Affairs: Entry Clearance Sub-Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/IsEMoAUF> .
<https://id.parliament.uk/IsEMoAUF> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1989-05-17+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1995-11-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/IsEMoAUF> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/wXqxbB2N> .
<https://id.parliament.uk/wXqxbB2N> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/wXqxbB2N> <https://id.parliament.uk/schema/formalBodyName> ""Treasury & Civil Service Sub-Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/EYQCxVic> .
<https://id.parliament.uk/EYQCxVic> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1989-05-15+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""1995-11-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/EYQCxVic> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/ZSJXcWE1> .
<https://id.parliament.uk/ZSJXcWE1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/ZSJXcWE1> <https://id.parliament.uk/schema/formalBodyName> ""Treasury & Civil Service"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/personHasFormalBodyMembership> <https://id.parliament.uk/UXpLpmfS> .
<https://id.parliament.uk/UXpLpmfS> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBodyMembership> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipStartDate> ""1997-07-16+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipEndDate> ""2001-05-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/UXpLpmfS> <https://id.parliament.uk/schema/formalBodyMembershipHasFormalBody> <https://id.parliament.uk/fYbWHWhk> .
<https://id.parliament.uk/fYbWHWhk> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/FormalBody> .
<https://id.parliament.uk/fYbWHWhk> <https://id.parliament.uk/schema/formalBodyName> ""Foreign Affairs Committee"" .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/GmWQzr0b> .
<https://id.parliament.uk/GmWQzr0b> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipStartDate> ""1987-06-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipEndDate> ""2015-03-30+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/GmWQzr0b> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/partyMemberHasPartyMembership> <https://id.parliament.uk/JWf4RycJ> .
<https://id.parliament.uk/JWf4RycJ> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PartyMembership> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipStartDate> ""2015-05-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipEndDate> ""2017-05-03+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/JWf4RycJ> <https://id.parliament.uk/schema/partyMembershipHasParty> <https://id.parliament.uk/LEYIBvV9> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/P8dV98n8> .
<https://id.parliament.uk/fy1cWNCD> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ContactPoint> .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/email> ""diane.abbott.office@parliament.uk"" .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/phoneNumber> ""020 7219 4426"" .
<https://id.parliament.uk/fy1cWNCD> <https://id.parliament.uk/schema/contactPointHasPostalAddress> <https://id.parliament.uk/uBuD8ZqA> .
<https://id.parliament.uk/uBuD8ZqA> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/PostalAddress> .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/addressLine1> ""House of Commons"" .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/addressLine2> ""London"" .
<https://id.parliament.uk/uBuD8ZqA> <https://id.parliament.uk/schema/postCode> ""SW1A 0AA"" .
<https://id.parliament.uk/5bX5Se0u> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/5bX5Se0u> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/5bX5Se0u> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/P8dV98n8> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2017-06-08+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
<https://id.parliament.uk/P8dV98n8> <https://id.parliament.uk/schema/parliamentaryIncumbencyHasContactPoint> <https://id.parliament.uk/fy1cWNCD> .
<https://id.parliament.uk/0BhROnYP> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/0BhROnYP> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/5bX5Se0u> .
<https://id.parliament.uk/0BhROnYP> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/FC1IDBx2> .
<https://id.parliament.uk/FC1IDBx2> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2001-06-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/FC1IDBx2> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/PfWrzef7> .
<https://id.parliament.uk/PfWrzef7> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2005-05-05+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/PfWrzef7> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/L71LLeyL> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/TuxAdhNO> .
<https://id.parliament.uk/p869pBTf> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/ConstituencyGroup> .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupName> ""Hackney North and Stoke Newington"" .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupStartDate> ""1983-06-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/p869pBTf> <https://id.parliament.uk/schema/constituencyGroupEndDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""1997-05-01+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1992-04-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/TuxAdhNO> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/8khvuL6U> .
<https://id.parliament.uk/8khvuL6U> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/HouseSeat> .
<https://id.parliament.uk/8khvuL6U> <https://id.parliament.uk/schema/houseSeatHasConstituencyGroup> <https://id.parliament.uk/p869pBTf> .
<https://id.parliament.uk/8khvuL6U> <https://id.parliament.uk/schema/houseSeatHasHouse> <https://id.parliament.uk/1AFu55Hs> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/9jEv9wQr> .
<https://id.parliament.uk/9jEv9wQr> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2015-03-30+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2010-05-06+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/9jEv9wQr> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/yVNL0a8Z> .
<https://id.parliament.uk/yVNL0a8Z> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""1992-04-09+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""1987-06-11+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/yVNL0a8Z> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/8khvuL6U> .
<https://id.parliament.uk/43RHonMf> <https://id.parliament.uk/schema/memberHasParliamentaryIncumbency> <https://id.parliament.uk/DqWmAFwg> .
<https://id.parliament.uk/DqWmAFwg> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://id.parliament.uk/schema/SeatIncumbency> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/parliamentaryIncumbencyEndDate> ""2017-05-03+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/parliamentaryIncumbencyStartDate> ""2015-05-07+00:00""^^<http://www.w3.org/2001/XMLSchema#date> .
<https://id.parliament.uk/DqWmAFwg> <https://id.parliament.uk/schema/seatIncumbencyHasHouseSeat> <https://id.parliament.uk/0BhROnYP> .
";

            var graph = Helper.Load(rdf);

            dynamic wrapper = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("https://id.parliament.uk/schema/"),
                true);

            var c = Newtonsoft.Json.JsonConvert.SerializeObject(wrapper);
        }


        //[TestMethod]
        public void x()
        {
            var graph = Helper.Load(@"
<http://example.com/s1> <http://example.com/p1> <http://example.com/s2> .
<http://example.com/s2> <http://example.com/p1> <http://example.com/s1> .
");

            dynamic s1 = new NodeWrapper(
                graph.Nodes.First(),
                new Uri("http://example.com/"),
                collapseSingularArrays: true);

            dynamic s2 = s1.p1;
            var other_s1 = s2.p1;

            Assert.AreEqual(
                s1,
                other_s1);
        }
    }
}