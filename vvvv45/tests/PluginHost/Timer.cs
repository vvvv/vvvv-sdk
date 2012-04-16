/*
 * Timer.cs - a cross-platform hi-res timer
 * Version 0.5, 16 May 2005
 *
 * Copyright (C) 2004-2005 Morgan LaMoore
 *
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 * 
 * Morgan LaMoore morganl@gmail.com
 * Modified by Rob Loach (http://www.robloach.net)
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MLib
{
	/// <summary>
	/// A Cross-Platform Hi-Resolution Timer.
	/// </summary>
	/// <remarks>
	/// By Morgan LaMoore (morganl@gmail.com).
	/// Modified by Rob Loach (http:/www.robloach.net).
	/// </remarks>
	/// <example>The following is an example of the timer's use:
	/// <code>
	/// using Timer = MLib.Timer;
	/// 
	/// // Limit to 100 FPS
	/// Timer.MaxFPS = 100;
	/// // If you didn't have this, it wouldn't limit framerate at all,
	/// 
	/// // Every frame
	/// int dt = Timer.DeltaTime; //get the length of the last frame (in ms)
	/// int absTime = Timer.Ticks; // get the absolute time relative to the system startup (in ms)
	/// Console.WriteLine(Timer.FPS.ToString());
	/// </code>
	/// </example>
	public class Timer
	{
		#region Static Constructor
		/// <summary>
		/// This initializes the timer; it gets called automatically at the first use of Timer.
		/// </summary>
		static Timer()
		{
			try
			{
				// use QPF/QPC if we can
				if (QueryPerformanceFrequency(out mFrequency) == false ||
					QueryPerformanceCounter(out mTicks) == false)
				{
					throw new Win32Exception();
				}
			}
			catch
			{
				// Function failed; we're not in windows or something's borked
				UseEnvClock();
			}
			
			// Get everything else set up
			mOldTicks = mTicks;
		}
		#endregion Static Constructor

		#region Public Properties

		#region DeltaTime
		/// <summary>
		/// The number of milliseconds between the previous two updates.
		/// </summary>
		public static int DeltaTime
		{
			get
			{
				return mDeltaTime;
			}
		}
		#endregion DeltaTime
		
		#region Ticks
		/// <summary>
		/// The absolute number of ticks in milliseconds.
		/// </summary>
		public static int Ticks
		{
			get
			{
				return (int) (1000 * mTicks / mFrequency);
			}
		}
		#endregion Ticks
		
		#region FPS
		/// <summary>
		/// The resulting number of frames per second.
		/// </summary>
		public static int FPS
		{
			get
			{
				return mFPS;
			}
		}
		#endregion FPS
		
		#region MaxFPS
		/// <summary>
		/// The maximum number of frames per second.
		/// </summary>
		/// <remarks>
		/// If you set this to a value less than 1000, it will limit the speed
		/// of your program so you don't go above the max fps (in reality you
		/// will get slightly lower than maxFPS). By default it is set to not
		/// limit speed at all.
		/// </remarks>
		public static int MaxFPS
		{
			get
			{
				return 1000 / mMinWait;
			}
			set
			{
				mMinWait = 1000 / value;
			}
		}
		#endregion MaxFPS

		#endregion Public Properties
		
		#region Public Methods

		#region Update()
		/// <summary>
		/// This function should be called once per frame. It updates the timer.
		/// </summary>
		public static void Update()
		{
			// last frame's ticks
			mOldTicks = mTicks;
			
			// Should we do it the easy way...
			if(mUseEnvTicks)
			{
				mTicks = Environment.TickCount;
				
				while(mMinWait > mTicks - mOldTicks)
				{
					System.Threading.Thread.Sleep(0);
					mTicks = Environment.TickCount;
				}
				
				mDeltaTime = (int) (mTicks - mOldTicks);
			}
				// or the QPF way?
			else
			{
				QueryPerformanceCounter(out mTicks);
				
				while(mMinWait * mFrequency / 1000 > mTicks - mOldTicks)
				{
					System.Threading.Thread.Sleep(0);
					QueryPerformanceCounter(out mTicks);
				}
				
				mDeltaTime = ((int) (1000 * (mTicks - mOldTicks) / mFrequency));
			}
			
			// counts frames and updates every second
			if(mSinceFPSUpdate >= 500)
			{
				mSinceFPSUpdate = 0;
				mFPS = mTempFPS;
				mTempFPS = 0;
			}
			
			mTempFPS += 2;
			mSinceFPSUpdate += mDeltaTime;
		}
		#endregion Update()

		#endregion Public Methods

		#region Private Methods
		
		#region UseEnvClock
		/// <summary>
		/// A private method that makes the timer use the environment clock.
		/// </summary>
		private static void UseEnvClock()
		{
			mUseEnvTicks = true;
			mFrequency = 1000;
			mTicks = Environment.TickCount;
			// Doesn't the framework make everything simpler?
			// It's too bad Microsoft didn't implement their
			// own framework with a high resolution timer.
		}
		#endregion UseEnvClock

		#region P/Invoke Functions

		#region QueryPerformanceFrequency
		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceFrequency(out long Frequency);
		#endregion QueryPerformanceFrequency
		
		#region QueryPerformanceCounter
		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceCounter(out long TimeCount);
		#endregion QueryPerformanceCounter

		#endregion P/Invoke Functions

		#endregion Private Methods
		
		#region Private Variables
		private static long mFrequency = 0;
		private static long mTicks = 0;
		private static long mOldTicks = 0;
		private static int mDeltaTime = 0;
		private static int mMinWait = 0;
		private static int mFPS = 0;
		private static int mTempFPS = 0;
		private static int mSinceFPSUpdate = 0;
		private static bool mUseEnvTicks = false;
		#endregion Private Variables
	}
}
