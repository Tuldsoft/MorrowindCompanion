using System.Collections.Generic;


// A flag for making binary save objects
public interface IBinarySaveObject
{
    // example: "name.cls"
    public string GetFileName();

    //public IBinarySaveObject PackData(object container);

    //public void UnpackData(IBinarySaveObject so, ref object container);

}
