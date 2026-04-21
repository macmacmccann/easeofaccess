using System;
using main_interface;
using Xunit;

namespace main_interface.Tests
{
    // takencombinations uses static collections
    // so i have tp clear them before/after each test.
    public class TakenCombinationsTests : IDisposable
    {
        public TakenCombinationsTests()
        {
            TakenCombinations._taken.Clear();
            TakenCombinations._assignedCombos.Clear();
        }

        public void Dispose()
        {
            TakenCombinations._taken.Clear();
            TakenCombinations._assignedCombos.Clear();
        }

        // ── HotKeyCombo equality ──

        [Fact]
        public void HotKeyCombo_SameValues_AreEqual()
        {
            var a = new TakenCombinations.HotKeyCombo(2, 65); // Ctrl + A
            var b = new TakenCombinations.HotKeyCombo(2, 65);

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void HotKeyCombo_DifferentModifier_AreNotEqual()
        {
            var ctrl_a = new TakenCombinations.HotKeyCombo(2, 65);  // Ctrl + A
            var alt_a  = new TakenCombinations.HotKeyCombo(1, 65);  // Alt  + A

            Assert.NotEqual(ctrl_a, alt_a);
            Assert.True(ctrl_a != alt_a);
        }

        [Fact]
        public void HotKeyCombo_DifferentVirtualKey_AreNotEqual()
        {
            var ctrl_a = new TakenCombinations.HotKeyCombo(2, 65); // Ctrl + A
            var ctrl_b = new TakenCombinations.HotKeyCombo(2, 66); // Ctrl + B

            Assert.NotEqual(ctrl_a, ctrl_b);
        }

        [Fact]
        public void HotKeyCombo_EqualCombos_HaveSameHashCode()
        {
            var a = new TakenCombinations.HotKeyCombo(4, 83); // Shift + S
            var b = new TakenCombinations.HotKeyCombo(4, 83);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void HotKeyCombo_ToString_ContainsModAndVk()
        {
            var combo = new TakenCombinations.HotKeyCombo(2, 65);
            var str = combo.ToString();

            Assert.Contains("2", str);
            Assert.Contains("65", str);
        }

        // ── Add / IsTaken 

        [Fact]
        public void Add_NewCombo_ReturnsTrue()
        {
            bool added = TakenCombinations.Add(2, 65);

            Assert.True(added);
        }

        [Fact]
        public void Add_DuplicateCombo_ReturnsFalse()
        {
            TakenCombinations.Add(2, 65);
            bool addedAgain = TakenCombinations.Add(2, 65);

            Assert.False(addedAgain);
        }

        [Fact]
        public void IsTaken_AfterAdd_ReturnsTrue()
        {
            TakenCombinations.Add(2, 65);

            Assert.True(TakenCombinations.IsTaken(2, 65));
        }

        [Fact]
        public void IsTaken_BeforeAdd_ReturnsFalse()
        {
            Assert.False(TakenCombinations.IsTaken(2, 65));
        }

        [Fact]
        public void IsTaken_OnlyMatchesExactCombo()
        {
            TakenCombinations.Add(2, 65); // Ctrl + A

            Assert.False(TakenCombinations.IsTaken(1, 65));  // Alt + A  — different mod
            Assert.False(TakenCombinations.IsTaken(2, 66));  // Ctrl + B — different key
        }

        // ── Remove

        [Fact]
        public void Remove_ExistingCombo_ReturnsTrueAndIsNoLongerTaken()
        {
            TakenCombinations.Add(2, 65);

            bool removed = TakenCombinations.Remove(2, 65);

            Assert.True(removed);
            Assert.False(TakenCombinations.IsTaken(2, 65));
        }

        [Fact]
        public void Remove_NonExistentCombo_ReturnsFalse()
        {
            bool removed = TakenCombinations.Remove(2, 65);

            Assert.False(removed);
        }

        // ── RemoveById 

        [Fact]
        public void RemoveById_KnownId_RemovesFromTakenAndDictionary()
        {
            var combo = new TakenCombinations.HotKeyCombo(2, 65);
            TakenCombinations._taken.Add(combo);
            TakenCombinations._assignedCombos[1] = combo;

            TakenCombinations.RemoveById(1);

            Assert.False(TakenCombinations.IsTaken(2, 65));
            Assert.False(TakenCombinations._assignedCombos.ContainsKey(1));
        }

        [Fact]
        public void RemoveById_UnknownId_DoesNotThrow()
        {
            // Should be a no-op, not an exception
            var ex = Record.Exception(() => TakenCombinations.RemoveById(999));

            Assert.Null(ex);
        }

        // ── TryGetCombo 

        [Fact]
        public void TryGetCombo_KnownId_ReturnsTrueAndCombo()
        {
            var combo = new TakenCombinations.HotKeyCombo(2, 65);
            TakenCombinations._assignedCombos[42] = combo;

            bool found = TakenCombinations.TryGetCombo(42, out var result);

            Assert.True(found);
            Assert.Equal(combo, result);
        }

        [Fact]
        public void TryGetCombo_UnknownId_ReturnsFalse()
        {
            bool found = TakenCombinations.TryGetCombo(99, out _);

            Assert.False(found);
        }

        // ── Multi-combo isolation 

        [Fact]
        public void MultipleCombos_TrackIndependently()
        {
            TakenCombinations.Add(2, 65);  // Ctrl + A
            TakenCombinations.Add(1, 66);  // Alt  + B
            TakenCombinations.Add(4, 67);  // Shift + C

            Assert.True(TakenCombinations.IsTaken(2, 65));
            Assert.True(TakenCombinations.IsTaken(1, 66));
            Assert.True(TakenCombinations.IsTaken(4, 67));

            TakenCombinations.Remove(1, 66);

            Assert.True(TakenCombinations.IsTaken(2, 65));
            Assert.False(TakenCombinations.IsTaken(1, 66));
            Assert.True(TakenCombinations.IsTaken(4, 67));
        }
    }
}
