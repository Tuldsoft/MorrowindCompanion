using System.Collections.Generic;

public interface ISaveObject
{
    public string FileName { get; }

    public bool SetValues(string[] values);

    public bool ValidateHeadings(string[] values);
}
