using System.Drawing;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations;
using Poltergeist.Operations.Inputing;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Tests.UnitTests.Components.Operations;

[TestClass]
public class LocatingTests : OperationModuleTestBase
{
    [TestMethod]
    [DataRow(WindowWidth, WindowHeight, ResizeRule.Disallow, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 2, ResizeRule.ConstrainProportion, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.ConstrainProportion, false)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.AnySize, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.Disallow, false)]
    public void TestScreenLocatingService(int width, int height, ResizeRule resize, bool expectedResult)
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new OperationModule(),
            },
            Execute = (processor) =>
            {
                var locatingService = processor.GetService<ScreenLocatingService>();
                var result = locatingService.TryLocate(new()
                {
                    ClassName = className,
                    WorkspaceSize = new(width, height),
                    Resizable = resize
                });

                Assert.AreEqual(expectedResult, result);
            },
        };

        macro.Test().AssertSuccess();
    }

    [TestMethod]
    [DataRow(WindowWidth, WindowHeight)]
    [DataRow(WindowWidth * 2, WindowHeight * 3)]
    [DataRow(WindowWidth / 2, WindowHeight / 3)]
    public void TestScreenLocatingPosition(int width, int height)
    {
        var targetX = width / 2;
        var targetY = height / 2;

        var macro = new TestMacro()
        {
            Modules =
            {
                new OperationModule(),
            },
            Execute = (processor) =>
            {
                processor.GetService<ScreenLocatingService>().TryLocate(new()
                {
                    ClassName = className,
                    WorkspaceSize = new Size(width, height),
                    Resizable = ResizeRule.AnySize
                });

                var mouseService = processor.GetService<MouseSendInputService>();
                mouseService.MoveTo(new PrecisePoint(targetX, targetY));
                mouseService.Click(MouseButtons.Left);
            },
        };

        var clickedPosition = new Point();
        void onMouseDown(MouseButtons _, int X, int Y)
        {
            clickedPosition = new Point(X, Y);
        }
        TestWindow!.MouseDown += onMouseDown;

        macro.Test();

        Thread.Sleep(100);

        TestWindow!.MouseDown -= onMouseDown;

        var expectPoint = new Point()
        {
            X = (int)(1.0f * WindowWidth / width * targetX),
            Y = (int)(1.0f * WindowHeight / height * targetY),
        };
        Assert.AreEqual(expectPoint, clickedPosition);
    }

    [TestMethod]
    [DataRow(WindowWidth, WindowHeight, ResizeRule.Disallow, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 2, ResizeRule.ConstrainProportion, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.ConstrainProportion, false)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.AnySize, true)]
    [DataRow(WindowWidth * 2, WindowHeight * 3, ResizeRule.Disallow, false)]
    public void TestWindowLocatingService(int width, int height, ResizeRule resize, bool expectedResult)
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new OperationModule(),
            },
            Execute = (processor) =>
            {
                var locatingService = processor.GetService<WindowLocatingService>();
                var result = locatingService.TryLocate(new()
                {
                    ClassName = className,
                    WorkspaceSize = new(width, height),
                    Resizable = resize
                }, out _);

                Assert.AreEqual(expectedResult, result);
            },
        };

        macro.Test().AssertSuccess();
    }

    [TestMethod]
    [DataRow(WindowWidth, WindowHeight)]
    [DataRow(WindowWidth * 2, WindowHeight * 3)]
    [DataRow(WindowWidth / 2, WindowHeight / 3)]
    public void TestWindowLocatingPosition(int width, int height)
    {
        var targetX = width / 2;
        var targetY = height / 2;

        var macro = new TestMacro()
        {
            Modules =
            {
                new OperationModule(),
            },
            Execute = (processor) =>
            {
                processor.GetService<WindowLocatingService>().TryLocate(new()
                {
                    ClassName = className,
                    WorkspaceSize = new Size(width, height),
                    Resizable = ResizeRule.AnySize
                }, out _);

                var mouseService = processor.GetService<MouseSendMessageService>();
                mouseService.Click(new PrecisePoint(targetX, targetY), MouseButtons.Left);
            },
        };

        var clickedPosition = new Point();
        void onMouseDown(MouseButtons _, int X, int Y)
        {
            clickedPosition = new Point(X, Y);
        }
        TestWindow!.MouseDown += onMouseDown;

        macro.Test();

        Thread.Sleep(100);

        TestWindow!.MouseDown -= onMouseDown;

        var expectPoint = new Point()
        {
            X = (int)(1.0f * WindowWidth / width * targetX),
            Y = (int)(1.0f * WindowHeight / height * targetY),
        };
        Assert.AreEqual(expectPoint, clickedPosition);
    }
}
