using System.Drawing;
using Poltergeist.Automations.Utilities.Images;
using Poltergeist.Operations;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Tests.UnitTests.Components.Operations;

[TestClass]
public class CapturingTests : OperationModuleTestBase
{
    [DataTestMethod]
    [DataRow(WindowWidth, WindowHeight)]
    [DataRow(WindowWidth * 2, WindowHeight * 3)]
    [DataRow(WindowWidth / 2, WindowHeight / 3)]
    public void TestScreenCapturingService(int width, int height)
    {
        TestCapture<ScreenLocatingService, ScreenCapturingService>(width, height);
    }


    [DataTestMethod]
    [DataRow(WindowWidth, WindowHeight)]
    [DataRow(WindowWidth * 2, WindowHeight * 3)]
    [DataRow(WindowWidth / 2, WindowHeight / 3)]
    public void TestPrintWindowCapturingService(int width, int height)
    {
        TestCapture<WindowLocatingService, PrintWindowCapturingService>(width, height);
    }


    [DataTestMethod]
    [DataRow(WindowWidth, WindowHeight)]
    [DataRow(WindowWidth * 2, WindowHeight * 3)]
    [DataRow(WindowWidth / 2, WindowHeight / 3)]
    public void TestBitBltCapturingService(int width, int height)
    {
        TestCapture<WindowLocatingService, BitBltCapturingService>(width, height);
    }

    public TestContext TestContext { get; set; }

    private void TestCapture<T1, T2>(int width, int height)
        where T1 : LocatingProvider
        where T2 : CapturingProvider
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new OperationModule(),
            },
            Execute = (processor) =>
            {
                var locatingService = (LocatingProvider)processor.GetService<T1>();
                var capturingService = (CapturingProvider)processor.GetService<T2>();

                if (locatingService is ScreenLocatingService sls)
                {
                    Assert.IsTrue(sls.TryLocate(new()
                    {
                        ClassName = className,
                        WorkspaceSize = new Size(width, height),
                        Resizable = ResizeRule.AnySize
                    }), "Failed to locate");
                }
                else if (locatingService is WindowLocatingService wls)
                {
                    Assert.IsTrue(wls.TryLocate(new()
                    {
                        ClassName = className,
                        WorkspaceSize = new Size(width, height),
                        Resizable = ResizeRule.AnySize
                    }, out _), "Failed to locate");
                }
                
                using var windowImage = capturingService.Capture();
                using var sampleImage = CreateTestPattern(width, height);
                if (!AreEqual(sampleImage, windowImage))
                {
                    windowImage.Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_client.png"));
                    sampleImage.Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_client_sample.png"));
                    Assert.Fail("Failed to capture whole client");
                }

                using var areaImage = capturingService.Capture(new Rectangle(0, 0, width / 2, height / 2));
                using var sampleImage2 = BitmapUtil.Crop(sampleImage, new(0, 0, width / 2, height / 2));
                if (!AreEqual(sampleImage2, areaImage))
                {
                    areaImage.Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_area.png"));
                    sampleImage2.Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_area_sample.png"));
                    Assert.Fail("Failed to capture area");
                }

                var pieceAreas = new Rectangle[]
                {
                    new (0, 0, width / 2, height / 2),
                    new (width / 2, 0, width / 2, height / 2),
                    new (0, height / 2, width / 2, height / 2),
                    new (width / 2, height / 2, width / 2, height / 2),
                };
                var pieceImages = capturingService.Capture(pieceAreas);
                for (var i = 0; i < pieceAreas.Length; i++)
                {
                    using var sampleImage3 = BitmapUtil.Crop(sampleImage, pieceAreas[i]);
                    if (!AreEqual(sampleImage3, pieceImages[i]))
                    {
                        pieceImages[i].Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_piece{i}.png"));
                        sampleImage3.Save(Path.Combine((string)TestContext.Properties["DeploymentDirectory"], $"{nameof(CapturingProvider)}_{width}x{height}_piece{i}_sample.png"));
                        pieceImages[i].Dispose();
                        Assert.Fail($"Failed to capture piece{i}");
                    }
                }
            },
        };

        macro.Test().AssertSuccess();
    }
}
