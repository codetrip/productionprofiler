
using NUnit.Framework;

namespace ProductionProfiler.Tests.Integration
{
    [TestFixture]
    public class CastleWebTests : WebTestsBase
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            ConfigureWithCastle();
        }
    }
}
