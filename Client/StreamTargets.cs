﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SysDVRClient
{
	enum StreamKind
	{
		Video,
		Audio
	};

	interface IOutTarget
	{
		void SendData(byte[] data, UInt64 ts) => SendData(data, 0, data.Length, ts);
		void SendData(byte[] data, int offset, int size, UInt64 ts);
	}

	abstract class BaseStreamManager
	{
		protected StreamingThread VideoThread, AudioThread;

		public StreamingSource VideoSource { get => VideoThread?.Source; set { if (VideoThread != null) VideoThread.Source = value; } }
		public StreamingSource AudioSource { get => AudioThread?.Source; set { if (AudioThread != null) AudioThread.Source = value; } }

		public IOutTarget VideoTarget => VideoThread?.Target;
		public IOutTarget AudioTarget => AudioThread?.Target;

		public BaseStreamManager(IOutTarget VideoTarget, IOutTarget AudioTarget)
		{
			if (VideoTarget != null)
				VideoThread = new StreamingThread(StreamKind.Video, VideoTarget);
			if (AudioTarget != null)
				AudioThread = new StreamingThread(StreamKind.Audio, AudioTarget);
		}

		public virtual void Begin()
		{
			VideoThread?.Start();
			AudioThread?.Start();
		}

		public virtual void Stop()
		{
			VideoThread?.Stop();
			AudioThread?.Stop();
		}
	}

#if DEBUG
	class LoggingTarget : IOutTarget
	{
		readonly BinaryWriter bin;
		readonly MemoryStream mem = new MemoryStream();
		readonly string filename;

		public LoggingTarget(string filename)
		{
			this.filename = filename;
			bin = new BinaryWriter(mem);
		}

		~LoggingTarget()
		{
			File.WriteAllBytes(filename, mem.ToArray());
		}

		Stopwatch sw = new Stopwatch();

		public void SendData(byte[] data, int offset, int size, UInt64 ts)
		{
			Console.WriteLine($"{filename} - ts: {ts}");
			bin.Write(sw.ElapsedMilliseconds);
			bin.Write(ts);
			bin.Write(data, offset, size);
			sw.Restart();
		}
	}
#endif
}
