using System.Reflection;

namespace BuildingBlock.Monad.UnitTest;

[Trait("Category", "Contract")]
public class BuildingBlockMarkerSpec
{
    private static readonly Type MarkerType = typeof(IBuildingBlockMonadMarker);

    [Fact]
    public void BuildingBlockMarker_Should_Be_A_Static_StaticClass()
    {
        // Arrange
        var type = typeof(IBuildingBlockMonadMarker);

        // Act
        // (no action needed)

        // Assert
        type.IsAbstract.ShouldBeTrue();
        type.IsSealed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(Type.IsPublic), true)]
    [InlineData(nameof(Type.IsClass), true)]
    [InlineData(nameof(Type.IsAbstract), true)]
    [InlineData(nameof(Type.IsSealed), true)]
    [InlineData(nameof(Type.IsInterface), false)]
    [InlineData(nameof(Type.IsGenericType), false)]
    [InlineData(nameof(Type.IsValueType), false)]
    [InlineData(nameof(Type.IsEnum), false)]
    [InlineData(nameof(Type.IsArray), false)]
    [InlineData(nameof(Type.IsPrimitive), false)]
    public void BuildingBlockMarker_Should_Satisfy_Metadata_Contract(string propertyName, bool expected)
    {
        // Arrange
        var property = typeof(Type).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        // Act
        var actual = property?.GetValue(MarkerType);

        // Assert
        property.ShouldNotBeNull($"Type.{propertyName} not found — update the InlineData row if the property was renamed.");
        actual.ShouldBe(expected);
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

    [Theory]
    [InlineData(MemberTypes.Field)]
    [InlineData(MemberTypes.Property)]
    [InlineData(MemberTypes.Method)]
    [InlineData(MemberTypes.Event)]
    [InlineData(MemberTypes.NestedType)]
    [InlineData(MemberTypes.Constructor)]
    public void BuildingBlockMarker_Should_Declare_No_Public_Members(MemberTypes memberKind)
    {
        // Arrange
        const BindingFlags flags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        // Act
        var members = MarkerType
            .GetMembers(flags)
            .Where(member => member.MemberType == memberKind)
            .ToArray();

        // Assert
        members.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(BindingFlags.Public | BindingFlags.Instance)]
    [InlineData(BindingFlags.Public | BindingFlags.Static)]
    public void BuildingBlockMarker_Should_Have_No_Public_Constructor(BindingFlags flags)
    {
        // Arrange
        var type = MarkerType;

        // Act
        var constructors = type.GetConstructors(flags);

        // Assert
        constructors.ShouldBeEmpty();
    }

    [Fact]
    public void BuildingBlockMarker_Should_Reject_Reflection_Instantiation()
    {
        // Arrange
        var type = MarkerType;
        Exception? exception = null;

        // Act
        try
        {
            _ = Activator.CreateInstance(type);
        }
        catch (Exception ex)
        // Catch: Broad catch so the assertion reports the runtime's real exception type.
        {
            exception = ex;
        }

        // Assert
        exception.ShouldNotBeNull("Activator succeeded — the marker became instantiable.");
        exception.ShouldBeAssignableTo<MemberAccessException>();
    }
}
