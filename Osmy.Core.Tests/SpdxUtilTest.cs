using Osmy.Core.Util;

namespace Osmy.Core.Tests
{
    public class SpdxUtilTest
    {
        [Theory]
        [InlineData("hoge.spdx.json")]
        [InlineData("hoge.spdx.rdf.xml")]
        [InlineData("hoge.spdx.rdf")]
        [InlineData("hoge.spdx.xml")]
        [InlineData("hoge.spdx.xls")]
        [InlineData("hoge.spdx.xlsx")]
        [InlineData("hoge.spdx.yaml")]
        [InlineData("hoge.spdx.yml")]
        [InlineData("hoge.spdx.tag")]
        [InlineData("hoge.spdx")]
        [InlineData("hoge.spdx.rdf.ttl")]
        public void HasValidExtension_InputHasValidExtension_ShouldReturnTrue(string fileName)
        {
            Assert.True(SpdxUtil.HasValidExtension(fileName));
        }

        [Theory]
        [InlineData("hoge.spdx.ttl")]
        [InlineData("hoge.spdx.txt")]
        [InlineData("hoge.spdx.csv")]
        public void HasValidExntension_InputHasInvalidExtension_ShouldReturnFalse(string fileName)
        {
            Assert.False(SpdxUtil.HasValidExtension(fileName));
        }
    }
}