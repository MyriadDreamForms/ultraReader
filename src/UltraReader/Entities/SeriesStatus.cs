namespace UltraReader.Entities;

/// <summary>
/// Reading status for a series.
/// </summary>
public enum SeriesStatus
{
    /// <summary>Haven't started reading</summary>
    NotStarted = 0,
    
    /// <summary>Currently reading</summary>
    Reading = 1,
    
    /// <summary>Finished reading all available chapters</summary>
    Completed = 2,
    
    /// <summary>Paused reading</summary>
    OnHold = 3,
    
    /// <summary>Abandoned/dropped</summary>
    Dropped = 4
}
