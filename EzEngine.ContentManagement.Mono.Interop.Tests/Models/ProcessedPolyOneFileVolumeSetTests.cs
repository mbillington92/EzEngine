using EzEngine.ContentManagement.Models.PolyOneFile;
using EzEngine.ContentManagement.Mono.Interop.Models;
using Microsoft.Xna.Framework;

namespace EzEngine.ContentManagement.Mono.Interop.Tests.Models;

public class ProcessedPolyOneFileVolumeSetTests
{
    [Fact]
    public void GivenAPointInsideAVolume_WhenPointIsWithinAnyVolumeIsCalled_ThenItReturnsTrue()
    {
        List<CustomVertexProperty> customVertexProperties =
        [
            new CustomVertexProperty
            {
                Name = "Z",
                Values = [ "0", "0", "0" ]
            },
            new CustomVertexProperty
            {
                Name = "ZTop",
                Values = [ "512", "512", "512" ]
            }
        ];
        var testData = new ProcessedPolyOneFileVolumeSet(new Layer
        {
            VertsX = [ 0, 64, 32 ],
            VertsY = [ 0, 16, 64 ],
            CustomVertexProperties = customVertexProperties,
        }, new ProcessedPolyOneFile(new PolyOneRawFileData
        {
            PolyOneMeta = new MetaData
            {
                FileVersion = "0.1"
            },
            LayerGroups = [],
            CustomProperties = new CustomProperties
            {
                Levels = []
            }
        }));
        var candidatePoint = new Vector3(-32.0F, 24.0F, 128.0F);

        var result = testData.PointIsWithinAnyVolume(candidatePoint);

        Assert.True(result);
    }
}
