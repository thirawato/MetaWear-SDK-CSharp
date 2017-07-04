﻿using MbientLab.MetaWear.Builder;
using MbientLab.MetaWear.Core;
using MbientLab.MetaWear.Peripheral;
using MbientLab.MetaWear.Peripheral.Led;
using MbientLab.MetaWear.Sensor;
using NUnit.Framework;
using System.Threading.Tasks;

namespace MbientLab.MetaWear.Test {
    [TestFixture]
    class MacroTest : UnitTestBase {
        private IMacro macro;

        public MacroTest() : base(typeof(IMacro), typeof(ILed), typeof(IAccelerometerBmi160), typeof(ISwitch), typeof(IDataProcessor)) {
        }

        [SetUp]
        public override void SetUp() {
            base.SetUp();

            macro = metawear.GetModule<IMacro>();
        }

        [Test]
        public async Task LedOnBootAsync() {
            byte[][] expected = {
                new byte[] {0x02, 0x03, 0x02, 0x02, 0x10, 0x10, 0x00, 0x00, 0xf4, 0x01, 0x00, 0x00, 0xe8, 0x03, 0x00, 0x00, 0x05},
                new byte[] {0x02, 0x01, 0x01},
                new byte[] {0x0f, 0x02, 0x01},
                new byte[] {0x0f, 0x03, 0x02, 0x03, 0x02, 0x02, 0x10, 0x10, 0x00, 0x00, 0xf4, 0x01, 0x00, 0x00, 0xe8, 0x03, 0x00, 0x00, 0x05},
                new byte[] {0x0f, 0x03, 0x02, 0x01, 0x01},
                new byte[] {0x0f, 0x04}
            };
            var led = metawear.GetModule<ILed>();

            macro.StartRecord();
            led.EditPattern(Color.Blue, high: 16, low: 16, highTime: 500, duration: 1000, count: 5);
            led.Play();
            await macro.EndRecordAsync();

            Assert.That(platform.GetCommands(), Is.EqualTo(expected));
        }

        [Test]
        public async Task FreeFallOnBootAsync() {
            byte[][] expected = {
                new byte[] {0x03, 0x03, 0x28, 0x0c},
                new byte[] {0x09, 0x02, 0x03, 0x04, 0xff, 0xa0, 0x07, 0xa5, 0x01},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x00, 0x20, 0x03, 0x05, 0x10},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x01, 0x20, 0x0d, 0x09, 0x66, 0x02, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x02, 0x00, 0x06, 0x01, 0xff},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x02, 0x00, 0x06, 0x01, 0x01},
                new byte[] {0x09, 0x03, 0x01},
                new byte[] {0x09, 0x07, 0x03, 0x01},
                new byte[] {0x09, 0x03, 0x01},
                new byte[] {0x09, 0x07, 0x04, 0x01},
                new byte[] {0x03, 0x02, 0x01, 0x00},
                new byte[] {0x03, 0x01, 0x01},
                new byte[] {0x0f, 0x02, 0x01},
                new byte[] {0x0f, 0x03, 0x03, 0x03, 0x28, 0x0c},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x03, 0x04, 0xff, 0xa0, 0x07, 0xa5, 0x01},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x00, 0x20, 0x03, 0x05, 0x10},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x01, 0x20, 0x0d, 0x09, 0x66, 0x02, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x02, 0x00, 0x06, 0x01, 0xff},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x02, 0x00, 0x06, 0x01, 0x01},
                new byte[] {0x0f, 0x03, 0x09, 0x03, 0x01},
                new byte[] {0x0f, 0x03, 0x09, 0x07, 0x03, 0x01},
                new byte[] {0x0f, 0x03, 0x09, 0x03, 0x01},
                new byte[] {0x0f, 0x03, 0x09, 0x07, 0x04, 0x01},
                new byte[] {0x0f, 0x03, 0x03, 0x02, 0x01, 0x00},
                new byte[] {0x0f, 0x03, 0x03, 0x01, 0x01},
                new byte[] {0x0f, 0x04}
            };
            var accelerometer = metawear.GetModule<IAccelerometer>();

            macro.StartRecord();
            accelerometer.Configure(range: 16f);
            await accelerometer.Acceleration.AddRouteAsync(source =>
                source.Map(Function1.Rss)
                    .Average(16)
                    .Find(Threshold.Binary, 0.3f)
                    .Multicast()
                        .To().Filter(Comparison.Eq, -1).Stream(null)
                        .To().Filter(Comparison.Eq, 1).Stream(null)
            );
            accelerometer.Acceleration.Start();
            accelerometer.Start();
            await macro.EndRecordAsync();

            Assert.That(platform.GetCommands(), Is.EqualTo(expected));
        }

        [Test]
        public async Task LedSwitchAsync() {
            byte[][] expected = {
                new byte[] {0x09, 0x02, 0x01, 0x01, 0xff, 0x00, 0x02, 0x13},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x00, 0x60, 0x09, 0x0f, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x01, 0x60, 0x06, 0x06, 0x01, 0x00, 0x00, 0x00},
                new byte[] {0x09, 0x02, 0x09, 0x03, 0x01, 0x60, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x0a, 0x02, 0x09, 0x03, 0x02, 0x02, 0x03, 0x0f},
                new byte[] {0x0a, 0x03, 0x02, 0x02, 0x10, 0x10, 0x00, 0x00, 0xf4, 0x01, 0x00, 0x00, 0xe8, 0x03, 0x00, 0x00, 0xff},
                new byte[] {0x0a, 0x02, 0x09, 0x03, 0x02, 0x02, 0x01, 0x01},
                new byte[] {0x0a, 0x03, 0x01},
                new byte[] {0x0a, 0x02, 0x09, 0x03, 0x03, 0x02, 0x02, 0x01},
                new byte[] {0x0a, 0x03, 0x01},
                new byte[] {0x0f, 0x02, 0x00},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x01, 0x01, 0xff, 0x00, 0x02, 0x13},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x00, 0x60, 0x09, 0x0f, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x01, 0x60, 0x06, 0x06, 0x01, 0x00, 0x00, 0x00},
                new byte[] {0x0f, 0x03, 0x09, 0x02, 0x09, 0x03, 0x01, 0x60, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00},
                new byte[] {0x0f, 0x03, 0x0a, 0x02, 0x09, 0x03, 0x02, 0x02, 0x03, 0x0f},
                new byte[] {0x0f, 0x03, 0x0a, 0x03, 0x02, 0x02, 0x10, 0x10, 0x00, 0x00, 0xf4, 0x01, 0x00, 0x00, 0xe8, 0x03, 0x00, 0x00, 0xff},
                new byte[] {0x0f, 0x03, 0x0a, 0x02, 0x09, 0x03, 0x02, 0x02, 0x01, 0x01},
                new byte[] {0x0f, 0x03, 0x0a, 0x03, 0x01},
                new byte[] {0x0f, 0x03, 0x0a, 0x02, 0x09, 0x03, 0x03, 0x02, 0x02, 0x01},
                new byte[] {0x0f, 0x03, 0x0a, 0x03, 0x01},
                new byte[] {0x0f, 0x04}
            };
            var led = metawear.GetModule<ILed>();

            macro.StartRecord(execOnBoot: false);
            await metawear.GetModule<ISwitch>().State.AddRouteAsync(source =>
                source.Count().Map(Function2.Modulus, 2).Multicast()
                        .To().Filter(Comparison.Eq, ComparisonOutput.Absolute, 1).React(token => {
                            led.EditPattern(Color.Blue, high: 16, low: 16, duration: 1000, highTime: 500);
                            led.Play();
                        })
                        .To().Filter(Comparison.Eq, ComparisonOutput.Absolute, 0).React(token => led.Stop(true))
            );
            await macro.EndRecordAsync();

            Assert.That(platform.GetCommands(), Is.EqualTo(expected));
        }
    }
}
