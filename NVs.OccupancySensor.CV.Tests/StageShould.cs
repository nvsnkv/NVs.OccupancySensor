using System;
using System.Threading.Tasks;
using Emgu.CV;
using NVs.OccupancySensor.CV.Tests.Utils;
using NVs.OccupancySensor.CV.Utils.Flow;
using Xunit;

namespace NVs.OccupancySensor.CV.Tests
{
    public abstract class StageShould<TIn, TOut> where TIn:struct,IColor where TOut:struct, IColor
    {
        private readonly Stage<Image<TIn, byte>, Image<TOut, byte>> stage;

        internal StageShould(Stage<Image<TIn, byte>, Image<TOut, byte>> stage)
        {
            this.stage = stage;
        }

        [Fact]
        public async Task CompleteOutputStreamWhenSourceStreamCompleted()
        {
            var observer = new TestImageObserver<TOut>();
            
            using (stage.Output.Subscribe(observer))
            {
                await Task.Run(() => stage.OnCompleted());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task ForwardErrors()
        {
            var observer = new TestImageObserver<TOut>();
            
            using (stage.Output.Subscribe(observer))
            {
                await Task.Run(() => stage.OnError(new TestException()));
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.IsType<TestException>(observer.Error);
        }

        [Fact]
        public async Task CompleteStreamOnReset()
        {
            var observer = new TestImageObserver<TOut>();

            using (stage.Output.Subscribe(observer))
            {
                await Task.Run(() => stage.Reset());
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Assert.True(observer.StreamCompleted);
        }

        [Fact]
        public async Task DropNewFramesIfSubtractorIsPreviousIsStillInProgress()
        {
            SetupLongRunningPayload(TimeSpan.FromMilliseconds(200));

            var observer = new TestImageObserver<TOut>();

            using (stage.Output.Subscribe(observer))
            {
                var _ = Task.Run(() => stage.OnNext(new Image<TIn, byte>(1, 1)));
                _ = Task.Run(() => stage.OnNext(new Image<TIn, byte>(1, 1)));
                _ = Task.Run(() => stage.OnNext(new Image<TIn, byte>(1, 1)));
                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }

            Assert.Single(observer.ReceivedItems);
            Assert.Equal((ulong)1, stage.Statistics.ProcessedFrames);
            Assert.Equal((ulong)2, stage.Statistics.DroppedFrames);
        }

        protected abstract void SetupLongRunningPayload(TimeSpan delay);
    }
}