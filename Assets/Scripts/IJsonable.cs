using System.Collections;
using System.Collections.Generic;


// While the interface has only one requirement - to implement a FileName - it is
//   only attached to objects that can be represented by json. IJsonables may have
//   other IJsonables contained in them. IJsonables are always System.Serializable.

// Fields of simple values, Lists, and other IJsonables are jsonable. Most everything
//   else is not, including Enum (the parent class), properties, non-serializable classes,
//   Dictionaries, etc.
public interface IJsonable
{
    public string FileName { get; }

}
