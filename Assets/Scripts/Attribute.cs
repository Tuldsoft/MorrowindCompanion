using System.Collections;
using System.Collections.Generic;

public enum AttrName
{
    none,
    Strength,
    Intelligence,
    Willpower,
    Agility,
    Speed,
    Endurance,
    Personality,
    Luck
}

// A Character has eight Attributes, one for each type.
public class Attribute 
{
    public AttrName Name { get; set; }
    
    static readonly Dictionary<AttrName, string> shortName = new Dictionary<AttrName, string>()
    {
        { AttrName.Strength, "STR" },
        { AttrName.Intelligence, "INT" },
        { AttrName.Willpower, "WIL" },
        { AttrName.Agility, "AGI" },
        { AttrName.Speed, "SPD" },
        { AttrName.Endurance, "END" },
        { AttrName.Personality, "PER" },
        { AttrName.Luck, "LUK" }
    };

    public string ShortName { get => shortName[Name]; }

    public override string ToString() => Name.ToString();

    public static implicit operator string(Attribute rhs) => rhs.ToString();

}


