using NUnit.Framework;

namespace Com2Verse.UIExtension.Tests
{
    public class RenderTextureHelperTests
    {
        [Test]
        public void DefaultRenderTextureFormatTest()
        {
            var renderTexture = RenderTextureHelper.CreateRenderTexture();
            Assert.IsTrue(renderTexture.format.IsSupported());
        }
    }
}
