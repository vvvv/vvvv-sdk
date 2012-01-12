using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks.Schedulers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.ImagePlayer
{
	/// <summary>
	/// Given a directory containing a sequence of images this class will
	/// try to preload the next couple of images based on the given frame
	/// number in order to provide fast access.
	/// </summary>
	public class ImagePlayer : IDisposable
	{
		public const string DEFAULT_FILEMASK = "*.dds";
		public const double DEFAULT_FPS = 25.0;
		
		private string FDirectory = string.Empty;
		private string FFilemask = DEFAULT_FILEMASK;
		private double FFps = DEFAULT_FPS;
		private string[] FFiles = new string[0];
		private Frame FCurrentFrame = new Frame(new FrameInfo(-1, string.Empty), Enumerable.Empty<Texture>(), null);
		
		private readonly int FPreloadCount;
		private readonly IEnumerable<Device> FDevices;
		private readonly ILogger FLogger;
		private ObjectPool<Stream> FStreamPool;
		private readonly ObjectPool<byte[]> FBufferPool;
		private readonly TexturePool FTexturePool;
		private readonly CancellationTokenSource FCancellationTokenSource;
		private readonly CancellationToken FCancellationToken;
		private readonly TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>> FFilePreloader;
		private readonly TransformBlock<Tuple<FrameInfo, Stream>, Frame> FFramePreloader;
		private readonly BufferBlock<Frame> FFrameBuffer;
		private readonly LinkedList<FrameInfo> FScheduledFrameInfos = new LinkedList<FrameInfo>();
		
		public ImagePlayer(int preloadCount, IEnumerable<Device> devices, ILogger logger)
		{
			FPreloadCount = preloadCount;
			FDevices = devices;
			FLogger = logger;

			FStreamPool = new ObjectPool<Stream>(() => new MemoryStream());
			FBufferPool = new ObjectPool<byte[]>(() => new byte[0x2000]);
			FTexturePool = new TexturePool();
			
			FCancellationTokenSource = new CancellationTokenSource();
			FCancellationToken = FCancellationTokenSource.Token;
			
			var filePreloaderOptions = new ExecutionDataflowBlockOptions()
			{
				CancellationToken = FCancellationToken,
				MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
				TaskScheduler = new IOTaskScheduler(),
				BoundedCapacity = FPreloadCount + 1
			};
			
			FFilePreloader = new TransformBlock<FrameInfo, Tuple<FrameInfo, Stream>>(
				(Func<FrameInfo, Tuple<FrameInfo, Stream>>) PreloadFile,
				filePreloaderOptions
			);
			
			var framePreloaderOptions = new ExecutionDataflowBlockOptions()
			{
				CancellationToken = FCancellationToken,
				MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
				BoundedCapacity = FPreloadCount + 1
			};
			
			FFramePreloader = new TransformBlock<Tuple<FrameInfo, Stream>, Frame>(
				(Func<Tuple<FrameInfo, Stream>, Frame>) PreloadFrame,
				framePreloaderOptions
			);
			
			var frameBufferOptions = new DataflowBlockOptions()
			{
				CancellationToken = FCancellationToken
			};
			
			FFrameBuffer = new BufferBlock<Frame>(frameBufferOptions);
			
			FFilePreloader.LinkTo(FFramePreloader);
			FFramePreloader.LinkTo(FFrameBuffer);
			
			FFilePreloader.Completion.ContinueWith(t => FFramePreloader.Complete());
			FFramePreloader.Completion.ContinueWith(t => FFrameBuffer.Complete());
		}
		
		public void Dispose()
		{
			Stall();
			
			FCurrentFrame.Dispose();
			
			// Mark head of pipeline as completed
			FFilePreloader.Complete();
			
			try
			{
				// Wait for last element in pipeline till completed
				FFrameBuffer.Completion.Wait();
				
				FFilePreloader.Completion.Dispose();
				FFramePreloader.Completion.Dispose();
				FFrameBuffer.Completion.Dispose();
			}
			catch (AggregateException e)
			{
				// TODO: Check if these are only exceptions of type OperationCancledException
			}
			finally
			{
				foreach (var stream in FStreamPool.ToArrayAndClear())
				{
					stream.Dispose();
				}
				FStreamPool = null;
				
				FBufferPool.ToArrayAndClear();
				FTexturePool.Dispose();
				FCancellationTokenSource.Dispose();
			}
		}
		
		public string Directory
		{
			get
			{
				return FDirectory;
			}
			set
			{
				Contract.Requires(System.IO.Directory.Exists(value));
				
				if (value != FDirectory)
				{
					FDirectory = value;
					Reload();
				}
			}
		}
		
		public string Filemask
		{
			get
			{
				return FFilemask;
			}
			set
			{
				Contract.Requires(value != null);
				
				if (value != FFilemask)
				{
					FFilemask = value;
					Reload();
				}
			}
		}
		
		public void Reload()
		{
			FFiles = System.IO.Directory.GetFiles(FDirectory, FFilemask);
		}
		
		public double FPS
		{
			get
			{
				return FFps;
			}
			set
			{
				Contract.Requires(value >= 0.0);
				
				FFps = value;
			}
		}
		
		public int PreloadCount
		{
			get
			{
				return FPreloadCount;
			}
		}
		
		public Frame CurrentFrame
		{
			get
			{
				return FCurrentFrame;
			}
		}
		
		public int UnusedFrames
		{
			get;
			private set;
		}
		
		public void Stall()
		{
			var stallingFrameInfo = new FrameInfo(-1, string.Empty);
			
			stallingFrameInfo.Cancel();
			
			while (!FFilePreloader.Post(stallingFrameInfo))
			{
				Thread.Sleep(1);
			}
			
			FrameInfo frameInfo = null;
			while (frameInfo != stallingFrameInfo)
			{
				var frame = FFrameBuffer.Receive();
				frameInfo = frame.Info;
				FScheduledFrameInfos.Remove(frameInfo);
				frame.Dispose();
			}
		}
		
		public void Seek(double time)
		{
			// Nothing to do
			if (FFiles.Length == 0)
			{
				return;
			}
			
			// Compute and normalize the frame number, as it can be any value
			int newFrameNr = VMath.Zmod((int) Math.Floor(time * FPS), FFiles.Length);
			
			// Cancel scheduled frames with a lower frame number
			foreach (var frameInfo in FScheduledFrameInfos)
			{
				if (frameInfo.FrameNr < newFrameNr)
				{
					frameInfo.Cancel();
				}
			}
			
			var frameInfosToPreload =
				from frameNr in Enumerable.Range(newFrameNr, FPreloadCount + 1)
				let normalizedFrameNr = VMath.Zmod(frameNr, FFiles.Length)
				select new FrameInfo(normalizedFrameNr, FFiles[normalizedFrameNr]);
			
			foreach (var frameInfo in frameInfosToPreload)
			{
				if (!FScheduledFrameInfos.Contains(frameInfo))
				{
					if (FFilePreloader.Post(frameInfo))
					{
						FScheduledFrameInfos.AddLast(frameInfo);
					}
				}
			}
			
			var newFrameInfo = frameInfosToPreload.First();
			
			while (newFrameInfo != FCurrentFrame.Info)
			{
				var frame = FFrameBuffer.Receive();
				if (frame.Info != newFrameInfo)
				{
					FScheduledFrameInfos.Remove(frame.Info);
					frame.Dispose();
					UnusedFrames++;
				}
				else
				{
					FScheduledFrameInfos.Remove(FCurrentFrame.Info);
					FCurrentFrame.Dispose();
					FCurrentFrame = frame;
				}
			}
		}
		
		private Tuple<FrameInfo, Stream> PreloadFile(FrameInfo frameInfo)
		{
			// TODO: Consider using CopyStreamToStreamAsync from TPL extensions
			var timer = new HiPerfTimer();
			timer.Start();
			
			var filename = frameInfo.Filename;
			var cancellationToken = frameInfo.Token;
			var memoryStream = FStreamPool.GetObject();
			var buffer = FBufferPool.GetObject();
			
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					memoryStream.Position = 0;
					if (fileStream.Length != memoryStream.Length)
					{
						memoryStream.SetLength(fileStream.Length);
					}
					
					while (fileStream.Position < fileStream.Length)
					{
						//                                cancellationToken.ThrowIfCancellationRequested();

						int numBytesRead = fileStream.Read(buffer, 0, buffer.Length);
						memoryStream.Write(buffer, 0, numBytesRead);
					}
//							fileStream.CopyTo(memoryStream);
					
					memoryStream.Position = 0;
				}
			}
			catch (OperationCanceledException)
			{
				// That's fine
			}
			catch (Exception e)
			{
				// Mark this frame as canceled
				frameInfo.Cancel();
				
				// Log the exception
				FLogger.Log(e);
			}
			finally
			{
				// Put the buffer back in the pool so other blocks can use it
				FBufferPool.PutObject(buffer);
			}
			
			timer.Stop();
			frameInfo.DurationIO = timer.Duration;
			
			return Tuple.Create(frameInfo, memoryStream);
		}
		
		private Frame PreloadFrame(Tuple<FrameInfo, Stream> tuple)
		{
			var timer = new HiPerfTimer();
			timer.Start();
			
			var frameInfo = tuple.Item1;
			var stream = tuple.Item2;
			var cancellationToken = frameInfo.Token;
			var textures = new List<Texture>();
			
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
				var bitmapFrame = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Bgra32, decoder.Palette, 0.0);
				var sourceRect = new Int32Rect(0, 0, bitmapFrame.PixelWidth, bitmapFrame.PixelHeight);
				
				Device[] devices = null;
				lock (FDevices)
				{
					devices = FDevices.ToArray();
				}
				
				foreach (var device in devices)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var texture = FTexturePool.GetTexture(
						device,
						bitmapFrame.PixelWidth,
						bitmapFrame.PixelHeight,
						1,
						Usage.Dynamic & ~Usage.AutoGenerateMipMap,
						Format.A8R8G8B8,
						Pool.Default);

					var dataRectangle = texture.LockRectangle(0, LockFlags.None);

					try
					{
						var stride = bitmapFrame.PixelWidth * (bitmapFrame.Format.BitsPerPixel / 8);
						var data = dataRectangle.Data;
						var buffer = data.DataPointer;
						bitmapFrame.CopyPixels(sourceRect, buffer, (int) data.Length, stride);
					}
					finally
					{
						texture.UnlockRectangle(0);
					}

					textures.Add(texture);
				}
			}
			catch (OperationCanceledException)
			{
				// That's fine
			}
			catch (Exception e)
			{
				// Mark this frame as canceled
				frameInfo.Cancel();
				
				// Do some cleanup
				foreach (var texture in textures)
				{
					FTexturePool.PutTexture(texture);
				}
				textures.Clear();
				
				// Log the exception
				FLogger.Log(e);
			}
			finally
			{
				// We're done with this frame, put used memory stream back in the pool
				FStreamPool.PutObject(stream);
			}
			
			timer.Stop();
			frameInfo.DurationTexture = timer.Duration;
			
			return new Frame(frameInfo, textures, FTexturePool);
		}
	}
}
