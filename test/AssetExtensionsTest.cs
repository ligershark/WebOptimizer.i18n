using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace WebOptimizer.i18n.Test
{
    public class AssetExtensionsTest
    {
        [Fact]
        public void Localize_single_Success()
        {
            var asset = GenerateAssets(1).First();

            asset.Localize<AssetExtensionsTest>();

            Assert.Equal(1, asset.Processors.Count);
            Assert.True(asset.Processors.Any(p => p is Localizer<AssetExtensionsTest>));
        }

        [Fact]
        public void Localize_Multiple_Success()
        {
            var assets = GenerateAssets(5).ToArray();

            assets.Localize<AssetExtensionsTest>();

            foreach (IAsset asset in assets)
            {
                Assert.Equal(1, asset.Processors.Count);
                Assert.True(asset.Processors.Any(p => p is Localizer<AssetExtensionsTest>));
            }
        }

        private IEnumerable<IAsset> GenerateAssets(int howMany)
        {
            for (int i = 0; i < howMany; i++)
            {
                var asset = new Mock<IAsset>();
                asset.SetupGet(a => a.Processors)
                     .Returns(new List<IProcessor>());

                yield return asset.Object;
            }
        }
    }
}
