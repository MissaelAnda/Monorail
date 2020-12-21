using System.Diagnostics;

namespace Monorail.Debug
{
    public static class Insist
    {
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Fail()
		{
			Log.Core.Error("Assertion failed.");
			Debugger.Break();
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Fail(string message, params object[] args)
		{
			Log.Core.Error(message, args);
			Debugger.Break();
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Assert(bool condition)
		{
			if (!condition)
				Fail();
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Assert(bool condition, string message, params object[] args)
		{
			if (!condition)
				Fail(message, args);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertFalse(bool condition)
		{
			Assert(!condition);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertFalse(bool condition, string message, params object[] args)
		{
			Assert(!condition, message, args);
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNull(object obj)
		{
			Assert(obj == null);
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNull(object obj, string message, params object[] args)
		{
			Assert(obj == null, message, args);
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNotNull(object obj)
		{
			Assert(obj != null);
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNotNull(object obj, string message, params object[] args)
		{
			Assert(obj != null, message, args);
		}


		/// <summary>
		/// asserts that first is equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertEq(object first, object second)
		{
			Assert(first.Equals(second));
		}


		/// <summary>
		/// asserts that first is not equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNotEq(object first, object second)
		{
			Assert(!first.Equals(second));
		}


		/// <summary>
		/// asserts that first is equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertEq(object first, object second, string message, params object[] args)
		{
			Assert(first.Equals(second), message, args);
		}


		/// <summary>
		/// asserts that first is not equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AssertNotEq(object first, object second, string message, params object[] args)
		{
			Assert(!first.Equals(second), message, args);
		}
	}
}
