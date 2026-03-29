using System;

[Serializable]
public class FatigueReading
{
    public string playerId;
    public float  fatigueScore;
    public float  pupilDiameter;
    public float  blinkInterval;
    public long   timestampMs;
}

[Serializable]
public class FatigueResponse
{
    public string player_id;
    public float  fatigue_score;
    public float  pupil_diameter;
    public string timestamp;
}