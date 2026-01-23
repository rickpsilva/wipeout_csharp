using Xunit;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace WipeoutRewrite.Tests.Core.Data;

public class GameDataServiceTests
{
    private readonly IGameDataService _service;
    
    public GameDataServiceTests()
    {
        _service = new GameDataService(NullLogger<GameDataService>.Instance);
        
        // Load from test data path (relative to solution root)
        var testDataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "game_data.json");
        _service.Load(testDataPath);
    }
    
    [Fact]
    public void GetTeams_ReturnsAllFourTeams()
    {
        var teams = _service.GetTeams();
        
        Assert.Equal(4, teams.Count);
        Assert.Contains(teams, t => t.Name == "AG SYSTEMS");
        Assert.Contains(teams, t => t.Name == "AURICOM");
        Assert.Contains(teams, t => t.Name == "QIREX");
        Assert.Contains(teams, t => t.Name == "FEISAR");
    }
    
    [Fact]
    public void GetTeam_WithValidId_ReturnsCorrectTeam()
    {
        var team = _service.GetTeam(0);
        
        Assert.Equal(0, team.Id);
        Assert.Equal("AG SYSTEMS", team.Name);
        Assert.Equal(2, team.LogoModelIndex);
        Assert.Equal(new[] { 0, 1 }, team.Pilots);
    }
    
    [Fact]
    public void GetTeam_WithInvalidId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _service.GetTeam(999));
    }
    
    [Fact]
    public void GetPilots_ReturnsAllEightPilots()
    {
        var pilots = _service.GetPilots();
        
        Assert.Equal(8, pilots.Count);
        Assert.Contains(pilots, p => p.Name == "JOHN DEKKA");
        Assert.Contains(pilots, p => p.Name == "PAUL JACKSON");
    }
    
    [Fact]
    public void GetPilot_WithValidId_ReturnsCorrectPilot()
    {
        var pilot = _service.GetPilot(0);
        
        Assert.Equal(0, pilot.Id);
        Assert.Equal("JOHN DEKKA", pilot.Name);
        Assert.Equal(0, pilot.TeamId);
        Assert.Equal("wipeout/textures/dekka.cmp", pilot.PortraitPath);
    }
    
    [Fact]
    public void GetPilotsForTeam_ReturnsCorrectPilots()
    {
        // Team 0 (AG SYSTEMS) has pilots 0 and 1
        var pilots = _service.GetPilotsForTeam(0);
        
        Assert.Equal(2, pilots.Count);
        Assert.Equal("JOHN DEKKA", pilots[0].Name);
        Assert.Equal("DANIEL CHANG", pilots[1].Name);
    }
    
    [Fact]
    public void GetCircuits_ReturnsAllSevenCircuits()
    {
        var circuits = _service.GetCircuits();
        
        Assert.Equal(7, circuits.Count);
        Assert.Contains(circuits, c => c.Name == "ALTIMA VII");
        Assert.Contains(circuits, c => c.Name == "FIRESTAR");
    }
    
    [Fact]
    public void GetNonBonusCircuits_ReturnsSixCircuits()
    {
        var circuits = _service.GetNonBonusCircuits();
        
        Assert.Equal(6, circuits.Count);
        Assert.DoesNotContain(circuits, c => c.Name == "FIRESTAR");
    }
    
    [Fact]
    public void GetCircuit_WithValidId_ReturnsCorrectCircuit()
    {
        var circuit = _service.GetCircuit(0);
        
        Assert.Equal(0, circuit.Id);
        Assert.Equal("ALTIMA VII", circuit.Name);
        Assert.False(circuit.IsBonus);
    }
    
    [Fact]
    public void CircuitGetSettings_WithValidRaceClass_ReturnsCorrectSettings()
    {
        var circuit = _service.GetCircuit(0);
        var venomSettings = circuit.GetSettings("venom");
        var rapierSettings = circuit.GetSettings("rapier");
        
        Assert.Equal("wipeout/track02/", venomSettings.TrackPath);
        Assert.Equal("wipeout/track03/", rapierSettings.TrackPath);
        Assert.Equal(27.0f, venomSettings.StartLinePos);
        Assert.Equal(300.0f, venomSettings.BehindSpeed);
    }
    
    [Fact]
    public void TeamGetAttributes_WithValidRaceClass_ReturnsCorrectAttributes()
    {
        var team = _service.GetTeam(0); // AG SYSTEMS
        var venomAttrs = team.GetAttributes("venom");
        var rapierAttrs = team.GetAttributes("rapier");
        
        Assert.Equal(150.0f, venomAttrs.Mass);
        Assert.Equal(790.0f, venomAttrs.ThrustMax);
        Assert.Equal(12.0f, venomAttrs.Skid);
        
        Assert.Equal(150.0f, rapierAttrs.Mass);
        Assert.Equal(1200.0f, rapierAttrs.ThrustMax);
        Assert.Equal(10.0f, rapierAttrs.Skid);
    }
    
    [Fact]
    public void GetAiSettings_WithValidRaceClass_ReturnsSevenSettings()
    {
        var venomAi = _service.GetAiSettings("venom");
        var rapierAi = _service.GetAiSettings("rapier");
        
        Assert.Equal(7, venomAi.Count);
        Assert.Equal(7, rapierAi.Count);
        
        Assert.Equal(2550.0f, venomAi[0].ThrustMax);
        Assert.Equal(3750.0f, rapierAi[0].ThrustMax);
    }
    
    [Fact]
    public void GetRaceClasses_ReturnsTwoClasses()
    {
        var classes = _service.GetRaceClasses();
        
        Assert.Equal(2, classes.Count);
        Assert.Equal("VENOM CLASS", classes[0].Name);
        Assert.Equal("RAPIER CLASS", classes[1].Name);
    }
    
    [Fact]
    public void GetRaceTypes_ReturnsThreeTypes()
    {
        var types = _service.GetRaceTypes();
        
        Assert.Equal(3, types.Count);
        Assert.Equal("CHAMPIONSHIP RACE", types[0].Name);
        Assert.Equal("SINGLE RACE", types[1].Name);
        Assert.Equal("TIME TRIAL", types[2].Name);
    }
    
    [Fact]
    public void GetConstants_ReturnsCorrectValues()
    {
        var constants = _service.GetConstants();
        
        Assert.Equal(3, constants.NumLaps);
        Assert.Equal(3, constants.NumLives);
        Assert.Equal(3, constants.QualifyingRank);
        Assert.Equal(7, constants.NumAiOpponents);
        Assert.Equal(2, constants.NumPilotsPerTeam);
        Assert.Equal(6, constants.NumNonBonusCircuits);
        Assert.Equal(new[] { 9, 7, 5, 3, 2, 1, 0, 0 }, constants.RacePointsForRank);
    }
}
