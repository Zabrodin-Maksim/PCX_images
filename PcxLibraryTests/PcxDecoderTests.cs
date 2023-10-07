using Microsoft.VisualStudio.TestTools.UnitTesting;
using PcxLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static PcxLibrary.Tests.TestUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PcxLibrary.Tests
{
    [TestClass()]
    public class PcxDecoderTests
    {
        const string MarblesExpectedSamples = "4288323493,4290358958,4291283394,4278190080,4284446616,4278198904,4289512908,4285760912,4282470784,4278195456,4293585378,4294441979,4284834662,4278197294,4288653012,4284321369,4278195593,4285046975,4286881628,4286023812,4282471508,4288722291,4278210955,4278195456,4285031270,4278201410,4278787114,4293389028,4282732945,4292007127,4292007887,4280245357,4290952892,4291023572,4278201994,4292007121,4284832810,4289380022,4289636515,4283458424,4279245830,4283061337,4290161589,4281746780,4278221258,4286026148,4287398511,4280696118,4291679447,4278817489,4279259529,4286876810,4285495163,4290099371,4290756281,4289444543,4286408796,4290952617,4288580467,4293326847,4284444819,4290953154,4280235069,4282765556,4289831840,4289711007,4278195462,4288321948,4278791764,4285756252,4289505443,4280235050,4281491335,4278211777,4278785293,4279260283,4285360170,4284838831,4286413188,4281751919,4285049451,4285032559,4280716349,4279936721,4278190080,4285505268,4287082971,4279260283,4287794038,4285357117,4283072569,4287278976,4278195589,4292012014,4284452461,4293385937,4284460788,4285374059,4292797933,4293651944,";
        const string AlchemyPaletteExpectedSamples = "4287274160,4294967295,4294967295,4290308095,4294967295,4290308095,4288446841,4294967295,4290889423,4290029995,4290029995,4294967295,4281610808,4290308095,4292135423,4294967295,4278321410,4289787135,4292405756,4294967295,4283980410,4290968063,4290029995,4290029995,4284838018,4290029995,4292344831,4292073952,4290029995,4278321410,4290029995,4290029995,4278321410,4290029995,4294967295,4289370838,4283980410,4289787135,4294967295,4294967295,4290889423,4289370838,4292344831,4286211500,4294967295,4289370838,4294967295,4290029995,4294967295,4289787135,4278321410,4289787135,4290029995,4287274160,4294967295,4290029995,4292135423,4282598489,4290308095,4290029995,4294967295,4287274160,4287274160,4290029995,4290889423,4290029995,4292405756,4294967295,4290029995,4292405756,4290029995,4290029995,4290029995,4290029995,4278321410,4290029995,4278321410,4283980410,4294967295,4289787135,4292344831,4278321410,4294967295,4290029995,4290029995,4292405756,4287274160,4294967295,4287272408,4290029995,4294967295,4290889423,4292405756,4290308095,4284838018,4287272408,4290029995,4294967295,4292135423,4292344831,";
        const string FormsExpectedSamples = "4293980400,4292993505,4293980400,4293980400,4294967295,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4293980400,4294967295,4293980400,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4287661954,4294967295,4294967295,4293980400,4293980400,4293980400,4294967295,4293980400,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4292993505,4294967295,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4293980400,4294967295,4293980400,4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4293980400,4293980400,4293980400,4293980400,4293980400,4292993505,4294967295,4294967295,4294967295,4294967295,4294967295,4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4293980400,4293980400,4294701541,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4293980400,";
        const string FormsPaletteExpectedSamples = "4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4293980400,4294967295,4293980400,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4294967295,4292993505,4294967295,4294967295,4292993505,4294967295,4293980400,4294967295,4293980400,4294967295,4293980400,4294967295,4293980400,4293980400,4294967295,4294967295,4293980400,4294967295,4294967295,4294967295,4293980400,4294967295,4294967295,";
        const string ChristmasPaletteExpectedSamples = "4278915873,4280761689,4287482090,4285098110,4278849043,4281086776,4282403172,4279111696,4282013307,4280559916,4290895594,4290493629,4279967780,4279910506,4280493092,4278848268,4286494682,4279835162,4280101177,4281289581,4280821286,4281091688,4287273399,4294506745,4281945693,4294967295,4279967780,4281216559,4282604154,4280630880,4280691765,4281088067,4285234330,4286229964,4279509555,4284703844,4280165417,4279111696,4279705131,4278387975,4288063943,4278918711,4294770173,4284440928,4279509555,4279837486,4280630880,4294967295,4284440928,4280629842,4281491081,4281285459,4278848268,4290895594,4294967295,4281084714,4283716196,4281682791,4281091688,4292013811,4283528590,4284243292,4283916926,4279835162,4279180589,4281084714,4280229662,4282206548,4287280333,4294770173,4280630880,4279441186,4283388782,4281682791,4282731866,4288729321,4287007394,4290888387,4278853952,4294967295,4281682791,4283718263,4287545050,4279442215,4279837486,4282477205,4280761689,4284182157,4279111696,4279113766,4280104278,4285625975,4281030274,4280559916,4294967295,4287862681,4281216559,4286086036,4280104278,4278849043,";

        private static readonly string RESOURCES_FOLDER = "Resources";
        private static readonly string MARBLES_FILE = Path.Combine(RESOURCES_FOLDER, "marbles.pcx");
        private static readonly string MARBLES_PALETTE_FILE = Path.Combine(RESOURCES_FOLDER, "marbles_palette.pcx");
        private static readonly string FORMS_FILE = Path.Combine(RESOURCES_FOLDER, "forms.pcx");
        private static readonly string FORMS_PALETTE_FILE = Path.Combine(RESOURCES_FOLDER, "forms_palette.pcx");
        private static readonly string ALCHEMY_PALETTE_FILE = Path.Combine(RESOURCES_FOLDER, "alchemy_palette.pcx");
        private static readonly string CHRISTMAS_PALETTE_FILE = Path.Combine(RESOURCES_FOLDER, "christmas_palette.pcx");

        private static FileStream MarblesStream => File.Open(MARBLES_FILE, FileMode.Open);
        private static FileStream MarblesPaletteStream => File.Open(MARBLES_PALETTE_FILE, FileMode.Open);
        private static FileStream FormsStream => File.Open(FORMS_FILE, FileMode.Open);
        private static FileStream FormsPaletteStream => File.Open(FORMS_PALETTE_FILE, FileMode.Open);
        private static FileStream AlchemyPaletteStream => File.Open(ALCHEMY_PALETTE_FILE, FileMode.Open);
        private static FileStream ChristmasPaletteStream => File.Open(CHRISTMAS_PALETTE_FILE, FileMode.Open);
        private static MemoryStream NotAPcxFileStream => new MemoryStream(new byte[500]);
        private static MemoryStream NotAPcxFileStream2 => new MemoryStream(new byte[1]);

        private Type? type;
        private object? obj;

        [TestInitialize]
        public void InitializeType()
        {
            type = typeof(PcxHeader).Assembly.GetType("PcxLibrary.PcxDecoder");
            Assert.IsNotNull(type);
        }

        [TestMethod()]
        public void IsPcxFileTest()
        {
            using (var s = MarblesStream)
            {
                obj = New(type, s);
                Invoke(obj, "ReadHeader");
                Assert.IsTrue((bool)GetProperty(obj, "IsPcxFile"));
            }
            using (var s = AlchemyPaletteStream)
            {
                obj = New(type, s);
                Invoke(obj, "ReadHeader");
                Assert.IsTrue((bool)GetProperty(obj, "IsPcxFile"));
            }
            using (var s = NotAPcxFileStream)
            {
                obj = New(type, s);
                Invoke(obj, "ReadHeader");
                Assert.IsFalse((bool)GetProperty(obj, "IsPcxFile"));
            }
            using (var s = NotAPcxFileStream2)
            {
                obj = New(type, s);
                Invoke(obj, "ReadHeader");
                Assert.IsFalse((bool)GetProperty(obj, "IsPcxFile"));
            }
        }

        [TestMethod()]
        public void MarblesHeaderTest()
        {
            using var s = MarblesStream;
            obj = New(type, s);
            Invoke(obj, "ReadHeader");

            PcxHeader? header = (PcxHeader)GetProperty(obj, "Header");
            Assert.IsNotNull(header);

            Assert.AreEqual(8, header.BitsPerPixel);
            Assert.AreEqual(1420, header.BytesPerScanLine);
            Assert.AreEqual(1001, header.Height);
            Assert.AreEqual(1419, header.Width);
            Assert.AreEqual(300, header.HorizontalResolution);
            Assert.AreEqual(300, header.VerticalResolution);
            Assert.AreEqual(3, header.NumberOfBitPlanes);
            Assert.AreEqual(1, header.PaletteType);
            Assert.AreEqual(1, header.Encoding);
        }

        [TestMethod()]
        public void MarblesPaletteHeaderTest()
        {
            using var s = MarblesPaletteStream;
            obj = New(type, s);
            Invoke(obj, "ReadHeader");

            PcxHeader? header = (PcxHeader)GetProperty(obj, "Header");
            Assert.IsNotNull(header);

            Assert.AreEqual(8, header.BitsPerPixel);
            Assert.AreEqual(1419, header.BytesPerScanLine);
            Assert.AreEqual(1001, header.Height);
            Assert.AreEqual(1419, header.Width);
            Assert.AreEqual(0, header.HorizontalResolution);
            Assert.AreEqual(0, header.VerticalResolution);
            Assert.AreEqual(1, header.NumberOfBitPlanes);
            Assert.AreEqual(1, header.PaletteType);
            Assert.AreEqual(1, header.Encoding);
        }

        [TestMethod()]
        public void AlchemyPaletteHeaderTest()
        {
            using var s = AlchemyPaletteStream;
            obj = New(type, s);
            Invoke(obj, "ReadHeader");

            PcxHeader? header = (PcxHeader)GetProperty(obj, "Header");
            Assert.IsNotNull(header);

            Assert.AreEqual(8, header.BitsPerPixel);
            Assert.AreEqual(530, header.BytesPerScanLine);
            Assert.AreEqual(1050, header.Height);
            Assert.AreEqual(530, header.Width);
            Assert.AreEqual(0, header.HorizontalResolution);
            Assert.AreEqual(0, header.VerticalResolution);
            Assert.AreEqual(1, header.NumberOfBitPlanes);
            Assert.AreEqual(1, header.PaletteType);
            Assert.AreEqual(1, header.Encoding);
        }

        [TestMethod()]
        public void MarblesBorderTest()
        {
            using var s = MarblesStream;
            obj = New(type, s);
            Invoke(obj, "ReadHeader");
            Invoke(obj, "DecodeImageInForegroundThread");

            Image<Rgba32>? image = (Image<Rgba32>)GetProperty(obj, "Image");
            Assert.IsNotNull(image);

            uint black = new Rgba32(0, 0, 0, 255).Rgba;

            // top border
            for (int i = 0; i < 1419; i++)
                Assert.AreEqual(black, image[i, 0].Rgba);
            // left border
            for (int i = 0; i < 1001; i++)
                Assert.AreEqual(black, image[0, i].Rgba);
            // right border
            for (int i = 0; i < 1001; i++)
                Assert.AreEqual(black, image[1418, i].Rgba);
            // bottom border
            for (int i = 0; i < 1419; i++)
                Assert.AreEqual(black, image[i, 1000].Rgba);
        }

        [TestMethod()]
        public void MarblesSamplerTest()
        {
            SamplerTest(MarblesStream, 12333, 1009, 100, MarblesExpectedSamples);
        }

        [TestMethod()]
        public void AlchemyPaletteSamplerTest()
        {
            SamplerTest(AlchemyPaletteStream, 123, 11, 100, AlchemyPaletteExpectedSamples);
        }

        [TestMethod()]
        public void FormsSamplerTest()
        {
            SamplerTest(FormsStream, 1, 7, 120, FormsExpectedSamples);
        }
        
        [TestMethod()]
        public void FormsPaletteSamplerTest()
        {
            SamplerTest(FormsPaletteStream, 11011, 333, 78, FormsPaletteExpectedSamples);
        }

        [TestMethod()]
        public void ChristmasPaletteSamplerTest()
        {
            SamplerTest(ChristmasPaletteStream, 231, 321, 100, ChristmasPaletteExpectedSamples);
        }

        private void SamplerTest(FileStream imagestream, uint seedX, uint seedY, int count, string expectedSamples)
        {
            using var s = imagestream;
            obj = New(type, s);
            Assert.IsNotNull(obj);
            Invoke(obj, "ReadHeader");
            Invoke(obj, "DecodeImageInForegroundThread");

            uint samplerX = Sampler(seedX);
            uint samplerY = Sampler(seedY);

            Image<Rgba32>? image = (Image<Rgba32>?)GetProperty(obj, "Image");
            Assert.IsNotNull(image);

            string actualSamples = "";

            for (int i = 0; i < count; i++)
            {
                samplerX = Sampler(samplerX);
                samplerY = Sampler(samplerY);

                int x = (int)(samplerX % image.Width);
                int y = (int)(samplerY % image.Height);

                actualSamples += $"{image[x, y].PackedValue},";
            }

            Assert.AreEqual(expectedSamples, actualSamples);

            static uint Sampler(uint state)
            {
                return 22695477 * state + 1;
            }
        }
    }
}