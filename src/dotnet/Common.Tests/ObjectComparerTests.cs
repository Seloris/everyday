using Everyday.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.Tests
{
    public class ObjectComparerTests
    {
        public class RootWithProps
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
            public decimal Prop3 { get; set; }

            public RootWithProps(string a, int b, decimal c)
            {
                Prop1 = a;
                Prop2 = b;
                Prop3 = c;
            }
        }

        public class RootWithChild<T>
        {
            public T Child { get; set; }

            public RootWithChild(T child)
            {
                Child = child;
            }
        }
        public class Child
        {
            public string ChildProp { get; set; }

            public Child(string childProp)
            {
                ChildProp = childProp;
            }
        }
        public class WithIndexer
        {
            private Dictionary<string, int> _internalData;

            public WithIndexer(int a, int b)
            {
                _internalData = new Dictionary<string, int>()
                {
                    ["a"] = a,
                    ["b"] = b
                };
            }

            public int A
            {
                get => Getter("a");
                set => Setter("a", value);
            }

            public int B
            {
                get => Getter("b");
                set => Setter("b ", value);
            }

            public int this[string index] => Getter(index);

            private int Getter(string key) => _internalData.TryGetValue(key, out int value) ? value : 0;
            private void Setter(string key, int value) => _internalData[key] = value;
        }


        [Fact]
        public void ObjectComparer_Compare_RootPropsAreDifferents_ShouldReturnDifferences()
        {
            // Arrange
            var objectA = new RootWithProps("a", 1, 2m);
            var objectB = new RootWithProps("b", 2, 3m);

            // Act
            PropertyCompareResult[] diffs = ObjectComparer.Compare(objectA, objectB);

            // Assert
            diffs.Should().Contain(new PropertyCompareResult(nameof(RootWithProps.Prop1), "a", "b"));
            diffs.Should().Contain(new PropertyCompareResult(nameof(RootWithProps.Prop2), 1, 2));
            diffs.Should().Contain(new PropertyCompareResult(nameof(RootWithProps.Prop3), 2m, 3m));
        }

        [Fact]
        public void ObjectComparer_Compare_RootPropsAreTheSame_ShouldReturnEmpty()
        {
            // Arrange
            var objectA = new RootWithProps("a", 1, 2m);
            var objectB = new RootWithProps("a", 1, 2m);

            // Act
            PropertyCompareResult[] diffs = ObjectComparer.Compare(objectA, objectB);

            // Assert
            diffs.Should().BeEmpty();
        }

        [Fact]
        public void ObjectComparer_Compare_ChildPropsAreDifferents_ShouldReturnDifferences()
        {
            // Arrange
            var objectA = new RootWithChild<Child>(new Child("a"));
            var objectB = new RootWithChild<Child>(new Child("b"));

            // Act
            PropertyCompareResult[] diffs = ObjectComparer.Compare(objectA, objectB);

            // Assert
            diffs.Should().Contain(new PropertyCompareResult("Child.ChildProp", "a", "b"));
        }

        [Fact]
        public void ObjectComparer_Compare_ChildPropsAreTheSame_ShouldReturnEmpty()
        {
            // Arrange
            var objectA = new RootWithChild<Child>(new Child("a"));
            var objectB = new RootWithChild<Child>(new Child("a"));

            // Act
            PropertyCompareResult[] diffs = ObjectComparer.Compare(objectA, objectB);

            // Assert
            diffs.Should().BeEmpty();
        }


        [Fact]
        public void ObjectComparer_Compare_RootWithDifferentArrays_ShouldReturnDifferences()
        {
            // Arrange
            var baseline = new string[] { "a" };
            var empty = Array.Empty<string>();
            var withOneMoreValue = new string[] { "a", "b" };
            var sameSizeDifferentValue = new string[] { "b" };

            var baselineObj = new RootWithChild<string[]>(baseline);
            var emptyObj = new RootWithChild<string[]>(empty);
            var withOneMoreValueObj = new RootWithChild<string[]>(withOneMoreValue);
            var sameSizeDifferentValueObj = new RootWithChild<string[]>(sameSizeDifferentValue);

            // Act
            PropertyCompareResult[] emptyDiffs = ObjectComparer.Compare(baselineObj, emptyObj);
            PropertyCompareResult[] withOneMoreValueDiffs = ObjectComparer.Compare(baselineObj, withOneMoreValueObj);
            PropertyCompareResult[] sameSizeDifferentValueDiffs = ObjectComparer.Compare(baselineObj, sameSizeDifferentValueObj);

            // Assert
            emptyDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, empty));
            withOneMoreValueDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, withOneMoreValue));
            sameSizeDifferentValueDiffs.Should().Contain(new PropertyCompareResult("Child[0]", baseline[0], sameSizeDifferentValue[0]));
        }

        [Fact]
        public void ObjectComparer_Compare_RootWithSameArrays_ShouldBeEmpty()
        {
            // Arrange
            var a1 = new string[] { "a" };
            var a2 = new string[] { "a" };

            var b1 = new string[] { "a", "b" };
            var b2 = new string[] { "a", "b" };

            var a1Obj = new RootWithChild<string[]>(a1);
            var a2Obj = new RootWithChild<string[]>(a2);

            var b1Obj = new RootWithChild<string[]>(b1);
            var b2Obj = new RootWithChild<string[]>(b2);

            // Act
            PropertyCompareResult[] diffsA = ObjectComparer.Compare(a1Obj, a2Obj);
            PropertyCompareResult[] diffsB = ObjectComparer.Compare(b1Obj, b2Obj);

            // Assert
            diffsA.Should().BeEmpty();
            diffsB.Should().BeEmpty();
        }

        [Fact]
        public void ObjectComparer_Compare_RootWithDifferentLists_ShouldReturnPropertyCompareResults()
        {
            // Arrange
            var baseline = new List<string> { "a" };
            var empty = new List<string>();
            var withOneMoreValue = new List<string> { "a", "b" };
            var sameSizeDifferentValue = new List<string> { "b" };

            var baselineObj = new RootWithChild<List<string>>(baseline);
            var emptyObj = new RootWithChild<List<string>>(empty);
            var withOneMoreValueObj = new RootWithChild<List<string>>(withOneMoreValue);
            var sameSizeDifferentValueObj = new RootWithChild<List<string>>(sameSizeDifferentValue);

            // Act
            PropertyCompareResult[] emptyDiffs = ObjectComparer.Compare(baselineObj, emptyObj);
            PropertyCompareResult[] withOneMoreValueDiffs = ObjectComparer.Compare(baselineObj, withOneMoreValueObj);
            PropertyCompareResult[] sameSizeDifferentValueDiffs = ObjectComparer.Compare(baselineObj, sameSizeDifferentValueObj);

            // Assert
            emptyDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, empty));
            withOneMoreValueDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, withOneMoreValue));
            sameSizeDifferentValueDiffs.Should().Contain(new PropertyCompareResult("Child[0]", baseline[0], sameSizeDifferentValue[0]));
        }

        [Fact]
        public void ObjectComparer_Compare_RootWithSameLists_ShouldBeEmpty()
        {
            // Arrange
            var a1 = new List<string> { "a" };
            var a2 = new List<string> { "a" };

            var b1 = new List<string> { "a", "b" };
            var b2 = new List<string> { "a", "b" };

            var a1Obj = new RootWithChild<List<string>>(a1);
            var a2Obj = new RootWithChild<List<string>>(a2);

            var b1Obj = new RootWithChild<List<string>>(b1);
            var b2Obj = new RootWithChild<List<string>>(b2);

            // Act
            PropertyCompareResult[] diffsA = ObjectComparer.Compare(a1Obj, a2Obj);
            PropertyCompareResult[] diffsB = ObjectComparer.Compare(b1Obj, b2Obj);

            // Assert
            diffsA.Should().BeEmpty();
            diffsB.Should().BeEmpty();
        }

        [Fact]
        public void ObjectComparer_Compare_RootWithDifferentDictionaries_ShouldReturnPropertyCompareResults()
        {
            // Arrange
            var baseline = new Dictionary<string, string> { ["a"] = "a" };
            var empty = new Dictionary<string, string>();
            var withOneMoreValue = new Dictionary<string, string> { ["a"] = "a", ["b"] = "b" };
            var sameSizeDifferentKey = new Dictionary<string, string> { ["b"] = "a" };
            var sameSizeDifferentValue = new Dictionary<string, string> { ["a"] = "b" };
            var propWithIndexerA = new WithIndexer(1, 2);
            var propWithIndexerB = new WithIndexer(1, 3);

            var baselineObj = new RootWithChild<Dictionary<string, string>>(baseline);
            var emptyObj = new RootWithChild<Dictionary<string, string>>(empty);
            var withOneMoreValueObj = new RootWithChild<Dictionary<string, string>>(withOneMoreValue);
            var sameSizeDifferentKeyObj = new RootWithChild<Dictionary<string, string>>(sameSizeDifferentKey);
            var sameSizeDifferentValueObj = new RootWithChild<Dictionary<string, string>>(sameSizeDifferentValue);
            var propWithIndexerObjA = new RootWithChild<WithIndexer>(propWithIndexerA);
            var propWithIndexerObjB = new RootWithChild<WithIndexer>(propWithIndexerB);

            // Act
            PropertyCompareResult[] emptyDiffs = ObjectComparer.Compare(baselineObj, emptyObj);
            PropertyCompareResult[] withOneMoreValueDiffs = ObjectComparer.Compare(baselineObj, withOneMoreValueObj);
            PropertyCompareResult[] sameSizeDifferentValueDiffs = ObjectComparer.Compare(baselineObj, sameSizeDifferentKeyObj);
            PropertyCompareResult[] sameSizeDifferentKeyDiffs = ObjectComparer.Compare(baselineObj, sameSizeDifferentValueObj);
            PropertyCompareResult[] propWithIndexerObjDiffs = ObjectComparer.Compare(propWithIndexerObjA, propWithIndexerObjB);

            // Assert
            emptyDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, empty));
            withOneMoreValueDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, withOneMoreValue));
            sameSizeDifferentValueDiffs.Should().Contain(new PropertyCompareResult("Child", baseline, sameSizeDifferentKey));
            sameSizeDifferentKeyDiffs.Should().Contain(new PropertyCompareResult("Child[a]", baseline["a"], sameSizeDifferentValue["a"]));
            propWithIndexerObjDiffs.Should().Contain(new PropertyCompareResult("Child.B", 2, 3));
        }

        [Fact]
        public void ObjectComparer_Compare_RootWithSameDictionaries_ShouldBeEmpty()
        {
            // Arrange
            var a1 = new Dictionary<string, string> { ["a"] = "a" };
            var a2 = new Dictionary<string, string> { ["a"] = "a" };
            var b1 = new Dictionary<string, string> { ["a"] = "a", ["b"] = "b" };
            var b2 = new Dictionary<string, string> { ["a"] = "a", ["b"] = "b" };

            var a1Obj = new RootWithChild<Dictionary<string, string>>(a1);
            var a2Obj = new RootWithChild<Dictionary<string, string>>(a2);
            var b1Obj = new RootWithChild<Dictionary<string, string>>(b1);
            var b2Obj = new RootWithChild<Dictionary<string, string>>(b2);

            // Act
            PropertyCompareResult[] diffsA = ObjectComparer.Compare(a1Obj, a2Obj);
            PropertyCompareResult[] diffsB = ObjectComparer.Compare(b1Obj, b2Obj);

            // Assert
            diffsA.Should().BeEmpty();
            diffsB.Should().BeEmpty();
        }
    }
}
