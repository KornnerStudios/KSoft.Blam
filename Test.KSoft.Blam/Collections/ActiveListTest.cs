using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public sealed class ActiveListTest
		: Blam.BaseTestClass
	{
		[TestMethod]
		public void ActiveList_SlotGuards_ThrowExpectedExceptions()
		{
			var list = new ActiveList<string>(ActiveListDesc<string>.CreateForNullData(2));

			AssertThrowsArgumentOutOfRange("index", () => _ = list.SlotIsFree(-1));
			AssertThrowsArgumentOutOfRange("index", () => _ = list.SlotIsFree(2));
			AssertThrowsArgumentOutOfRange("index", () => _ = list.SlotIsFreeOrIndexIsNone(-2));
			AssertThrowsArgumentOutOfRange("index", () => _ = list.SlotIsFreeOrIndexIsNone(2));
			AssertThrowsArgumentOutOfRange("index", () => list.AddExplicit("invalid", -1));
			AssertThrowsArgumentOutOfRange("index", () => list.AddExplicit("invalid", 2));
		}

		[TestMethod]
		public void ActiveList_AddExplicit_RejectsOccupiedSlot()
		{
			var list = new ActiveList<string>(ActiveListDesc<string>.CreateForNullData(2));

			Assert.IsTrue(list.SlotIsFreeOrIndexIsNone(-1));
			Assert.IsTrue(list.SlotIsFree(0));

			list.AddExplicit("first", 0);

			Assert.IsFalse(list.SlotIsFree(0));
			Assert.ThrowsExactly<InvalidOperationException>(() => list.AddExplicit("duplicate", 0));
		}

		static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}
	};
}
