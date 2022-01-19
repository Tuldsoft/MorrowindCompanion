using System.Collections.Generic;

// Requires implementation of SetValues(), which reads a line from a CSV and
// stores the values into an ISaveObject, and ValidateHeadings(), which requires a CSV
// have a specific header.
public interface ISaveObject
{
    public bool SetValues(string[] values);

    public bool ValidateHeadings(string[] values);
}
