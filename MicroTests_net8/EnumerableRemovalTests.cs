using System.ComponentModel;

namespace MicroTests_net8;

public class EnumerableRemovalTests
{
    [Fact]
    [Trait("Owner", "Kuba S.")]
    [Description("Given enumerable array " +
                 "When removing element " +
                 "Then it crashes")]
    public void RemoveWhileEnumerating()
    {
        // Arrange
        var array = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        void Act()
        {
            foreach (var item in array)
            {
                if (item == 3)
                    array.Remove(item);
            }
        }

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    [Trait("Owner", "Kuba S.")]
    [Description("Given enumerable array " +
                 "When removing element even in wrapped linq of the source array " +
                 "Then it crashes")]
    public void RemoveWhileEnumeratingLinqWrapped()
    {
        // Arrange
        var array = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        void Act()
        {
            foreach (var item in array.Where(x => x > 2))
            {
                if (item == 3)
                    array.Remove(item);
            }
        }

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }
}