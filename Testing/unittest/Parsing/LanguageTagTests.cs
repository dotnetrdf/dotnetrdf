using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VDS.RDF.Parsing
{
    public class LanguageTagTests
    {
        private static readonly string[] WellFormedTags =
        {
            "en", "aar", "en-ext-ext-ext", "abcd", "abcde", // basic language tags
            "ru-Cyrl", // language + script
            "en-GB", "en-123", // language + region
            "en-variant", "en-1abc", // language + variant
            "en-a-abc123", // language + extension
            "en-x-abc123", // language + private-use
        };

        private static readonly string[] MalformedTags =
        {
            "en gb", // no spaces
        };

        public static IEnumerable<object[]> GetWellFormedTags()
        {
            return WellFormedTags.Select(x => new object[] {x});
        }

        public static IEnumerable<object[]> GetMalformedTags()
        {
            return MalformedTags.Select(x => new object[] {x});
        }

        [Theory]
        [MemberData(nameof(GetWellFormedTags))]
        public void TestWellFormedTags(string tag)
        {
            Assert.True(LanguageTag.IsWellFormed(tag),
                $"Expected language tag {tag} to parse as well-formed");
        }

        [Theory]
        [MemberData(nameof(GetMalformedTags))]
        public void TestMalformedTags(string tag)
        {
            Assert.False(LanguageTag.IsWellFormed(tag),
                $"Expected language tag {tag} to not parse as well-formed");
        }
    }
}
