namespace Grom.Tests
{
    using Grom;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Writing;

    class C
    {
        public Uri P { get; set; }
    }

    [TestClass]
    public class GraphWrapperTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_fails_null_graph()
        {
            new GraphWrapper(null);
        }

        [TestMethod]
        public void Set1()
        {
            var graph = new Graph();
            dynamic wrapper = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"), true);

            wrapper.job1 = new { name = "job1" };
            wrapper.person1 = new { name = "name1", job = wrapper.job1 };

            Console.WriteLine(wrapper.person1.job.name);
        }

        [TestMethod]
        public void Set2()
        {
            var graph = new Graph();
            dynamic wrapper = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"));
            wrapper["s"] = new { p = new { p2 = "o" } };

            Console.WriteLine(wrapper.s.p[0]);
        }

        [TestMethod]
        public void Set3()
        {
            var graph = new Graph();
            dynamic wrapper = new GraphWrapper(graph, new Uri("http://example.com/"), new Uri("http://example.com/"));
            wrapper["s"] = new C { P = new Uri("http://example.com/o") };

            Console.WriteLine(wrapper.s.P[0]);
        }

        [TestMethod]
        public void Set4()
        {
            var graph = new Graph();
            dynamic g = new GraphWrapper(graph, new Uri("https://id.parliament.uk/"), new Uri("https://id.parliament.uk/schema/"), true);

            g.house1 = new { name = "House of Lords" };
            g.house1["rdf:type"] = new Uri("http://example.com/House");
            g.houseSeat1 = new { houseSeatHasHouse = g.house1 };
            g.interruption1 = new { startDate = DateTime.Now, endDate = DateTime.Now.AddDays(1) };
            g.incumbency1 = new { incumbencyHasInterruption = g.interruption1, seatIncumbencyHasHouseSeat = g.houseSeat1 };

            Assert.AreEqual("House of Lords", g.incumbency1.seatIncumbencyHasHouseSeat.houseSeatHasHouse.name);

            graph.SaveToStream(Console.Out, new CompressingTurtleWriter());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Get_index_fails_multidimensional()
        {
            dynamic wrapper = new GraphWrapper(new Graph());

            var result = wrapper[0, 0];
        }

        [TestMethod]
        [DynamicData(nameof(Data.MemberNames), typeof(Data))]
        public void Member_names_reduce_correctly(string subject, Uri baseUri, string memberName)
        {
            var wrapper = new GraphWrapper(
                Helper.Load($"<{subject}> <urn:example:p> <urn:example:o>."),
                baseUri);

            CollectionAssert.AreEqual(
                new[] { memberName },
                wrapper.GetDynamicMemberNames().ToArray());
        }

        [TestMethod]
        public void Get_member()
        {
            var graph = Helper.Load("<http://example.com/subject1> <http://example.com/predicate1> <http://example.com/object1> .");

            dynamic wrapper = new GraphWrapper(
                graph,
                new Uri("http://example.com/"));

            Assert.AreEqual(
                new NodeWrapper(graph.Nodes.UriNodes().First()),
                wrapper.subject1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Get_member_fails_without_base()
        {
            dynamic wrapper = new GraphWrapper(
                new Graph());

            var result = wrapper.subject1;
        }

        //[TestMethod]
        public void PersonByIdGraph()
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

            dynamic wrapper = new GraphWrapper(
                Helper.Load(rdf),
                new Uri("https://id.parliament.uk/"),
                new Uri("https://id.parliament.uk/schema/"),
                true);

            Assert.AreEqual(
                "Diane",
                wrapper["43RHonMf"].personGivenName);
        }
    }
}
