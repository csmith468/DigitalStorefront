using API.Database;
using FluentAssertions;
using Xunit;

namespace API.Tests.UnitTests;

public class DbAttributeValidatorTests
{
    [Fact]
    public void ValidateAllEntities_WithValidEntity_ShouldPass()
    {
        // Arrange & Act
        var exception = Record.Exception(() => DbAttributeValidator.ValidateSingleEntity(typeof(ValidTestEntity)));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void ValidateAllEntities_WithMissingDbTable_ShouldThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => DbAttributeValidator.ValidateSingleEntity(typeof(MissingDbTableEntity)));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Contain("Missing [DbTable] attribute");
    }

    [Fact]
    public void ValidateAllEntities_WithMissingPrimaryKey_ShouldThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => DbAttributeValidator.ValidateSingleEntity(typeof(MissingPrimaryKeyEntity)));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Contain("Missing [DbPrimaryKey] attribute");
    }

    [Fact]
    public void ValidateAllEntities_WithMultiplePrimaryKeys_ShouldThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => DbAttributeValidator.ValidateSingleEntity(typeof(MultiplePrimaryKeysEntity)));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Contain("Multiple [DbPrimaryKey] attributes found");
    }

    [Fact]
    public void ValidateAllEntities_WithNoColumns_ShouldThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => DbAttributeValidator.ValidateSingleEntity(typeof(NoColumnsEntity)));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOperationException>();
        exception.Message.Should().Contain("No properties with [DbColumn] attribute found");
    }

    
    

    [DbTable("dbo.validTest")]
    private class ValidTestEntity
    {
        [DbPrimaryKey] public int Id { get; set; }
        [DbColumn] public string Name { get; set; } = "";
    }

    // Missing [DbTable] attribute
    private class MissingDbTableEntity
    {
        [DbPrimaryKey] public int Id { get; set; }
        [DbColumn] public string Name { get; set; } = "";
    }

    [DbTable("dbo.missingPkTest")]
    private class MissingPrimaryKeyEntity
    {
        // No [DbPrimaryKey] attribute
        public int Id { get; set; }
        [DbColumn] public string Name { get; set; } = "";
    }

    [DbTable("dbo.multiplePkTest")]
    private class MultiplePrimaryKeysEntity
    {
        [DbPrimaryKey] public int Id { get; set; }
        [DbPrimaryKey] public int SecondId { get; set; } // Two primary keys
        [DbColumn] public string Name { get; set; } = "";
    }

    [DbTable("dbo.noColumnsTest")]
    private class NoColumnsEntity
    {
        [DbPrimaryKey] public int Id { get; set; }
        // No [DbColumn] attributes
        public string Name { get; set; } = "";
    }
}