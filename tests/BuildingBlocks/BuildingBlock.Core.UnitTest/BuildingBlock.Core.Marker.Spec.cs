using System.Reflection;

namespace BuildingBlock.Core.UnitTest;

[Trait("Category", "Contract")]
public sealed class BuildingBlockCoreMarkerSpec
{
    private static readonly Type MarkerType = typeof(IBuildingBlockCoreMarker);

    [Fact]
    public void BuildingBlockCoreMarker_Should_Be_A_Static_Class()
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
    public void BuildingBlockCoreMarker_Should_Live_In_Core_Namespace()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var @namespace = type.Namespace;

        // Assert
        @namespace.ShouldBe("BuildingBlock.Core");
    }

    [Fact]
    public void BuildingBlockCoreMarker_Should_Live_In_Core_Assembly()
    {
        // Arrange
        var type = MarkerType;

        // Act
        var assemblyName = type.Assembly.GetName().Name;

        // Assert
        assemblyName.ShouldBe("BuildingBlock.Core");
    }

    [Fact]
    public void BuildingBlockCoreMarker_Should_Declare_No_Public_Members()
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
    public void BuildingBlockCoreMarker_Should_Not_Be_Instantiable()
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
    public void BuildingBlockCoreMarker_Should_Identify_Its_Assembly()
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
