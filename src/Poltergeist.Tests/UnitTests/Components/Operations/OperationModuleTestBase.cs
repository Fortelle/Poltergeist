using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Tests.UnitTests.Components.Operations;

public abstract class OperationModuleTestBase : CommonMacroBase
{
    protected static string className = "test_window";
    protected static string windowName = "Test Window";
    protected const int WindowLeft = 100;
    protected const int WindowTop = 100;
    protected const int WindowWidth = 400;
    protected const int WindowHeight = 300;
    protected static nint windowHandle = nint.Zero;

    protected static TestWindowHelper? TestWindow;

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void Initialize(TestContext testContext)
    {
        Task.Run(() =>
        {
            TestWindow = new TestWindowHelper()
            {
                ClassName = className,
                WindowName = windowName,
                Width = WindowWidth,
                Height = WindowHeight,
                X = WindowLeft,
                Y = WindowTop,
                TopMost = true,
                Background = CreateTestPattern(WindowWidth, WindowHeight)
            };
            TestWindow.Build();

            TestWindowHelper.TranslateMessage();
        });

        Thread.Sleep(100);
    }

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass, ClassCleanupBehavior.EndOfClass)]
    public static void Cleanup()
    {
        TestWindow?.Close();
    }

    [TestInitialize]
    public void TestInitialize()
    {
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    protected static Bitmap CreateTestPattern(int width, int height)
    {
        var sampleImage = new Bitmap(width, height);
        using var gra = Graphics.FromImage(sampleImage);
        using var brush1 = new SolidBrush(Color.Black);
        using var brush2 = new SolidBrush(Color.White);
        for (var i = 0; i < 10; i++)
        {
            var brush = i % 2 == 0 ? brush1 : brush2;
            gra.FillRectangle(brush, width * i / 20, height * i / 20, width * (20 - i * 2) / 20, height * (20 - i * 2) / 20);
        }
        return sampleImage;
    }

    protected static bool AreEqual(Bitmap image1, Bitmap image2)
    {
        var fingerprint1 = GetFingerprint(image1, 8);
        var fingerprint2 = GetFingerprint(image2, 8);

        return fingerprint1.Zip(fingerprint2).Where(x => x.First != x.Second).Count() < 4;
        
        static bool[] GetFingerprint(Bitmap source, int size)
        {
            using var hashBitmap = new Bitmap(source, new Size(size, size));
            var bitmapData = hashBitmap.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.ReadOnly, hashBitmap.PixelFormat);
            var byteCount = bitmapData.Stride * size;
            var buffer = new byte[byteCount];
            Marshal.Copy(bitmapData.Scan0, buffer, 0, buffer.Length);
            hashBitmap.UnlockBits(bitmapData);

            var fingerprint = new bool[size * size];
            for (var i = 0; i < fingerprint.Length; i ++)
            {
                fingerprint[i] = buffer[i * 4 + 2] * .299 + buffer[i * 4 + 1] * .587 + buffer[i * 4 + 0] * .114 > 186;
            }
            return fingerprint;
        }
    }
}
