using StarWars.Infrastructure.External.DTOs;

namespace StarWars.Tests.External;

public class SwapiPagedResponseTests
{
    [Fact]
    public void SwapiPagedResponse_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var response = new SwapiPagedResponse<object>();

        // Assert
        response.Should().NotBeNull();
        response.Count.Should().Be(0);
        response.Next.Should().BeNull();
        response.Previous.Should().BeNull();
        response.Results.Should().NotBeNull();
        response.Results.Should().BeEmpty();
    }

    [Fact]
    public void SwapiPagedResponse_CanBeCreated_WithAllProperties()
    {
        // Arrange
        var results = new List<string> { "result1", "result2" };

        // Act
        var response = new SwapiPagedResponse<string>
        {
            Count = 10,
            Next = "https://swapi.dev/api/people/?page=2",
            Previous = null,
            Results = results
        };

        // Assert
        response.Should().NotBeNull();
        response.Count.Should().Be(10);
        response.Next.Should().Be("https://swapi.dev/api/people/?page=2");
        response.Previous.Should().BeNull();
        response.Results.Should().HaveCount(2);
        response.Results.Should().Contain("result1");
        response.Results.Should().Contain("result2");
    }

    [Fact]
    public void SwapiPagedResponse_CanSetPrevious()
    {
        // Arrange & Act
        var response = new SwapiPagedResponse<object>
        {
            Previous = "https://swapi.dev/api/people/?page=1"
        };

        // Assert
        response.Previous.Should().Be("https://swapi.dev/api/people/?page=1");
    }

    [Fact]
    public void SwapiPagedResponse_Results_CanBeModified()
    {
        // Arrange
        var response = new SwapiPagedResponse<string>();

        // Act
        response.Results.Add("item1");
        response.Results.Add("item2");

        // Assert
        response.Results.Should().HaveCount(2);
        response.Results.Should().Contain("item1");
        response.Results.Should().Contain("item2");
    }

    [Fact]
    public void SwapiPagedResponse_CanBeCreated_WithNullNext()
    {
        // Arrange & Act
        var response = new SwapiPagedResponse<object>
        {
            Count = 5,
            Next = null,
            Previous = null,
            Results = new List<object>()
        };

        // Assert
        response.Next.Should().BeNull();
        response.Previous.Should().BeNull();
    }
}

