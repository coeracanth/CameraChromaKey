using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Reactive.Bindings;
using System.Windows;
using System.Windows.Media;

namespace CameraChromaKey.ViewModels
{
	class MainWindow
	{
		const int frameWidth = 960;
		const int frameHeight = 540;

		public MainWindow()
		{
			var capture = new CvCapture(0);
			capture.SetCaptureProperty(CaptureProperty.FrameWidth, frameWidth);
			capture.SetCaptureProperty(CaptureProperty.FrameHeight, frameHeight);

			var frameSize = new CvSize(frameWidth, frameHeight);
			var dstrgba = Cv.CreateImage(frameSize, BitDepth.U8, 4);
			var dst = Cv.CreateImage(frameSize, BitDepth.U8, 3);
			var dst2 = Cv.CreateImage(frameSize, BitDepth.U8, 1);

			Observable.Interval(TimeSpan.FromMilliseconds(20), DispatcherScheduler.Current)
				.Select(_ => Cv.QueryFrame(capture))
				.Where(frame => frame != null)
				.Subscribe(frame =>
				{
					Cv.CvtColor(frame, dstrgba, ColorConversion.RgbToRgba);
					Cv.CvtColor(frame, dst, ColorConversion.RgbToHsv);
					Cv.InRangeS(dst, new CvScalar(55, 70, 70), new CvScalar(65, 255, 255), dst2);

					var lasttt = Cv.CreateImage(frameSize, BitDepth.U8, 4);
					Cv.Copy(dstrgba, lasttt, ~dst2);

					frame.Dispose();
					this.CaptureImageSource.Value = lasttt.ToWriteableBitmap();
				});

			this.InitWindowSizeCommand = new AsyncReactiveCommand();
			this.InitWindowSizeCommand.Subscribe(_ => Task.Run(() => this.InitWindowSize()));

			this.ExitCommand = new ReactiveCommand();
			this.ExitCommand.Subscribe(_ => App.Current.Shutdown());
		}

		public ReactiveProperty<WriteableBitmap> CaptureImageSource { get; } = new ReactiveProperty<WriteableBitmap>();
		public ReactiveProperty<bool> IsTopMost { get; } = new ReactiveProperty<bool>(true);
		public ReactiveProperty<int> CurrentWidth { get; } = new ReactiveProperty<int>(frameWidth);
		public ReactiveProperty<int> CurrentHeight { get; } = new ReactiveProperty<int>(frameHeight);

		public AsyncReactiveCommand InitWindowSizeCommand { get; }

		private void InitWindowSize()
		{
			this.CurrentWidth.Value = frameWidth;
			this.CurrentHeight.Value = frameHeight;
		}

		public ReactiveCommand ExitCommand { get; }

	}
}
