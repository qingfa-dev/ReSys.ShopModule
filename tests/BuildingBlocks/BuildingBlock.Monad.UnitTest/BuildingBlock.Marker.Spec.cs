using System.Reflection;

namespace BuildingBlock.Monad.UnitTest;

[Trait("Category", "Contract")]
public sealed class BuildingBlockMarkerSpec
{
    private static readonly Type MarkerType = typeof(IBuildingBlockMonadMarker);

    [Fact]
    public void BuildingBlockMarker_Should_Be_A_Static_Class()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var isClass = type.IsClass;
        var isAbstract = type.IsAbstract;
        var isSealed = type.IsSealed;

        // Assert
        isClass.ShouldBeTrue();
        isAbstract.ShouldBeTrue();
        isSealed.ShouldBeTrue();
    }

    [Fact]
    public void BuildingBlockMarker_Should_Live_In_Monad_Namespace()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var @namespace = type.Namespace;

        // Assert
        @namespace.ShouldBe("BuildingBlock.Monad");
    }

    [Fact]
    public void BuildingBlockMarker_Should_Live_In_Monad_Assembly()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var assemblyName = type.Assembly.GetName().Name;

        // Assert
        assemblyName.ShouldBe("BuildingBlock.Monad");
    }

    [Fact]
    public void BuildingBlockMarker_Should_Declare_No_Public_Members()
    {
        // Arrange
        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly;

        // Act
        var members = MarkerType
            .GetMembers(flags)
            .ToArray();

        // Assert
        members.ShouldBeEmpty();
    }

    [Fact]
    public void BuildingBlockMarker_Should_Not_Be_Instantiable()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var exception = Should.Throw<MemberAccessException>(
            () => Activator.CreateInstance(type));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void BuildingBlockMarker_Should_Identify_Its_Assembly()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var assembly = type.Assembly;
        var types = assembly.GetTypes();

        // Assert
        types.ShouldContain(type);
    }
}
