using Xunit;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Testes para entidades Ship.
/// Demonstra testes de lógica pura sem dependências externas.
/// </summary>
public class ShipTests
{
    [Fact]
    public void Ship_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var ship = new Ship("TestShip", shipId: 0);
        
        // Assert
        Assert.Equal(100, ship.Shield); // Shield começa em 100
        Assert.Equal(100, ship.MaxShield);
        Assert.Equal(0, ship.Speed);
        Assert.False(ship.IsDestroyed);
        Assert.False(ship.InputAccelerate);
        Assert.False(ship.InputBrake);
    }
    
    [Fact]
    public void Ship_TakeDamage_ShouldReduceShield()
    {
        // Arrange
        var ship = new Ship("TestShip", shipId: 0);
        
        // Act
        ship.TakeDamage(25);
        
        // Assert
        Assert.Equal(75, ship.Shield);
        Assert.False(ship.IsDestroyed);
    }
    
    [Fact]
    public void Ship_TakeFatalDamage_ShouldDestroy()
    {
        // Arrange
        var ship = new Ship("TestShip", shipId: 0);
        
        // Act
        ship.TakeDamage(150); // Dano maior que shield
        
        // Assert
        Assert.True(ship.Shield <= 0);
        Assert.True(ship.IsDestroyed);
    }
    
    [Fact]
    public void Ship_Acceleration_ShouldIncreaseSpeed()
    {
        // Arrange
        var ship = new Ship("TestShip", shipId: 0);
        float acceleration = 10f;
        float deltaTime = 0.016f; // 60 FPS
        
        // Act
        ship.Speed += acceleration * deltaTime;
        
        // Assert
        Assert.True(ship.Speed > 0);
        Assert.Equal(0.16f, ship.Speed, precision: 2);
    }
    
    // TODO: Re-enable when FireCooldown property is implemented
    // [Fact]
    // public void Ship_Update_ShouldReduceFireCooldown()
    // {
    //     // Arrange
    //     var ship = new Ship("TestShip", shipId: 0);
    //     ship.FireCooldown = 1.0f;
    //     
    //     // Act
    //     ship.Update(deltaTime: 0.5f);
    //     
    //     // Assert
    //     Assert.Equal(0.5f, ship.FireCooldown, precision: 2);
    // }
}

/// <summary>
/// Testes para Track.
/// </summary>
public class TrackTests
{
    [Fact]
    public void Track_Constructor_ShouldSetName()
    {
        // Arrange & Act
        var track = new Track("track01");
        
        // Assert
        Assert.Equal("track01", track.Name);
    }
    
    [Fact]
    public void Track_ShouldHaveEmptyFacesInitially()
    {
        // Arrange & Act
        var track = new Track("test");
        
        // Assert
        Assert.Empty(track.Faces);
    }
}
