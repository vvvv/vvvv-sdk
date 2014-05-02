/*
 * Created by SharpDevelop.
 * User: Tebjan Halm
 * Date: 05.12.2013
 * Time: 17:19
 * 
 * 
 */
using System;

namespace VVVV.Utils
{
	/// <summary>
	/// Generic event args
	/// </summary>
	public class EventArgs<T> : EventArgs
	{
		public T Parameter
		{
			get;
			private set;
		}
		
		public EventArgs(T parameter)
		{
			Parameter = parameter;
		}
	}
	
	/// <summary>
	/// Mutable generic event args 
	/// </summary>
	public class EventArgsMutable<T> : EventArgs
	{
		public T Parameter
		{
			get;
			set;
		}
		
		public EventArgsMutable(T parameter)
		{
			Parameter = parameter;
		}
	}
	
	public static class GenericEventHandlerExtensions
	{
		public static EventArgsMutable<T> CreateMutableArgs<T>(this EventHandler<EventArgsMutable<T>> handler, T input)
		{
			return new EventArgsMutable<T>(input);
		}

		public static EventArgs<T> CreateArgs<T>(this EventHandler<EventArgs<T>> handler, T input)
		{
			return new EventArgs<T>(input);
		}

		//tuple event args
		public static EventArgs<Tuple<T1,T2>> CreateArgs<T1,T2>(this EventHandler<EventArgs<Tuple<T1,T2>>> handler, T1 input1, T2 input2)
		{
			return new EventArgs<Tuple<T1,T2>>(Tuple.Create(input1, input2));
		}

		public static EventArgs<Tuple<T1,T2,T3>> CreateArgs<T1,T2,T3>(this EventHandler<EventArgs<Tuple<T1,T2,T3>>> handler, T1 input1, T2 input2, T3 input3)
		{
			return new EventArgs<Tuple<T1,T2,T3>>(Tuple.Create(input1, input2, input3));
		}
		
		//add more tuples if needed, up to 8 generic parameters are possible
		
	}
}
