using StarWars.Infrastructure.External.DTOs;

namespace StarWars.Tests.External;

public class SwapiPersonTests
{
    [Fact]
    public void SwapiPerson_CanBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var person = new SwapiPerson();

        // Assert
        person.Should().NotBeNull();
        person.Name.Should().BeEmpty();
        person.Height.Should().BeEmpty();
        person.Mass.Should().BeEmpty();
        person.HairColor.Should().BeEmpty();
        person.SkinColor.Should().BeEmpty();
        person.EyeColor.Should().BeEmpty();
        person.BirthYear.Should().BeEmpty();
        person.Gender.Should().BeEmpty();
        person.Homeworld.Should().BeEmpty();
        person.Films.Should().NotBeNull();
        person.Films.Should().BeEmpty();
        person.Species.Should().NotBeNull();
        person.Species.Should().BeEmpty();
        person.Vehicles.Should().NotBeNull();
        person.Vehicles.Should().BeEmpty();
        person.Starships.Should().NotBeNull();
        person.Starships.Should().BeEmpty();
        person.Url.Should().BeEmpty();
    }

    [Fact]
    public void SwapiPerson_CanBeCreated_WithAllProperties()
    {
        // Arrange
        var films = new List<string> { "https://swapi.dev/api/films/1/" };
        var species = new List<string> { "https://swapi.dev/api/species/1/" };
        var vehicles = new List<string> { "https://swapi.dev/api/vehicles/14/" };
        var starships = new List<string> { "https://swapi.dev/api/starships/12/" };

        // Act
        var person = new SwapiPerson
        {
            Name = "Luke Skywalker",
            Height = "172",
            Mass = "77",
            HairColor = "blond",
            SkinColor = "fair",
            EyeColor = "blue",
            BirthYear = "19BBY",
            Gender = "male",
            Homeworld = "https://swapi.dev/api/planets/1/",
            Films = films,
            Species = species,
            Vehicles = vehicles,
            Starships = starships,
            Url = "https://swapi.dev/api/people/1/"
        };

        // Assert
        person.Name.Should().Be("Luke Skywalker");
        person.Height.Should().Be("172");
        person.Mass.Should().Be("77");
        person.HairColor.Should().Be("blond");
        person.SkinColor.Should().Be("fair");
        person.EyeColor.Should().Be("blue");
        person.BirthYear.Should().Be("19BBY");
        person.Gender.Should().Be("male");
        person.Homeworld.Should().Be("https://swapi.dev/api/planets/1/");
        person.Films.Should().HaveCount(1);
        person.Species.Should().HaveCount(1);
        person.Vehicles.Should().HaveCount(1);
        person.Starships.Should().HaveCount(1);
        person.Url.Should().Be("https://swapi.dev/api/people/1/");
    }

    [Fact]
    public void SwapiPerson_Properties_CanBeModified()
    {
        // Arrange
        var person = new SwapiPerson
        {
            Name = "Initial Name"
        };

        // Act
        person.Name = "Updated Name";
        person.Height = "180";
        person.Films.Add("https://swapi.dev/api/films/2/");

        // Assert
        person.Name.Should().Be("Updated Name");
        person.Height.Should().Be("180");
        person.Films.Should().HaveCount(1);
    }

    [Fact]
    public void SwapiPerson_CanHaveEmptyLists()
    {
        // Arrange & Act
        var person = new SwapiPerson
        {
            Name = "Test Person",
            Films = new List<string>(),
            Species = new List<string>(),
            Vehicles = new List<string>(),
            Starships = new List<string>()
        };

        // Assert
        person.Films.Should().BeEmpty();
        person.Species.Should().BeEmpty();
        person.Vehicles.Should().BeEmpty();
        person.Starships.Should().BeEmpty();
    }

    [Fact]
    public void SwapiPerson_CanHaveMultipleItemsInLists()
    {
        // Arrange & Act
        var person = new SwapiPerson
        {
            Name = "Test Person",
            Films = new List<string>
            {
                "https://swapi.dev/api/films/1/",
                "https://swapi.dev/api/films/2/",
                "https://swapi.dev/api/films/3/"
            },
            Vehicles = new List<string>
            {
                "https://swapi.dev/api/vehicles/14/",
                "https://swapi.dev/api/vehicles/30/"
            }
        };

        // Assert
        person.Films.Should().HaveCount(3);
        person.Vehicles.Should().HaveCount(2);
    }
}

