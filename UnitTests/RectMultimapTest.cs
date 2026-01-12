using PurenailCore.CollectionUtil;

namespace UnitTests;

[TestFixture]
public class RectMultimapTest
{
    private static void AssertContents<T>(RectMultimap<T> map, float x, float y, params T[] expected)
    {
        HashSet<T> actualSet = [.. map.Get(x, y)];
        HashSet<T> expectedSet = [.. expected];

        HashSet<T> missing = [.. expectedSet.Where(t => !actualSet.Contains(t))];
        HashSet<T> extra = [.. actualSet.Where(t => !expectedSet.Contains(t))];

        Assert.AreEqual(missing.Count == 0 && extra.Count == 0, true, $"Mismatch for ({x:0.00}, {y:0.00}).  Missing: [{string.Join(", ", missing)}]; Extra: [{string.Join(", ", extra)}]");
    }

    [Test]
    public void TestRectMultimap()
    {
        RectMultimap<int> map = [];
        var rect1 = new Rect(new Interval(0, 3), new Interval(2, 5));
        var rect2 = new Rect(new Interval(2, 5), new Interval(0, 3));
        var rect3 = new Rect(new Interval(2, 5), new Interval(4, 7));
        var rect4 = new Rect(new Interval(4, 7), new Interval(2, 5));

        map.Add(rect1, 1);
        map.Add(rect2, 2);
        map.Add(rect3, 3);
        map.Add(rect4, 4);

        AssertContents(map, 0, 0);
        AssertContents(map, 0.5f, 0.5f);
        AssertContents(map, 6.5f, 6.5f);
        AssertContents(map, 1.5f, 3.5f, 1);
        AssertContents(map, 3.5f, 1.5f, 2);
        AssertContents(map, 3.5f, 5.5f, 3);
        AssertContents(map, 5.5f, 3.5f, 4);
        AssertContents(map, 2.5f, 2.5f, 1, 2);
        AssertContents(map, 2.5f, 4.5f, 1, 3);
        AssertContents(map, 4.5f, 2.5f, 2, 4);
        AssertContents(map, 4.5f, 4.5f, 3, 4);

        Rect rect5 = new(new Interval(0.1f, 6.9f), new(0.1f, 6.9f));
        map.Add(rect5, 5);

        AssertContents(map, 0, 0);
        AssertContents(map, 0.5f, 0.5f, 5);
        AssertContents(map, 6.5f, 6.5f, 5);
        AssertContents(map, 1.5f, 3.5f, 1, 5);
        AssertContents(map, 3.5f, 1.5f, 2, 5);
        AssertContents(map, 3.5f, 5.5f, 3, 5);
        AssertContents(map, 5.5f, 3.5f, 4, 5);
        AssertContents(map, 2.5f, 2.5f, 1, 2, 5);
        AssertContents(map, 2.5f, 4.5f, 1, 3, 5);
        AssertContents(map, 4.5f, 2.5f, 2, 4, 5);
        AssertContents(map, 4.5f, 4.5f, 3, 4, 5);
    }
}
