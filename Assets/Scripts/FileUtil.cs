using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For reading and writing various file types (csv, json)
// TODO: Replace json with binary serialization
public static class FileUtil
{

    //public static string localDirectory = "/Data/";
    //public static string directory = Application.persistentDataPath + localDirectory;
    public static string directory = Application.streamingAssetsPath;

    // Write a list of IJsonable to disk as a json file
    public static void WriteJson<T>(List<T> saveObjects, string fileName) where T : IJsonable
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string json = JsonHelper.ToJson(saveObjects.ToArray(), true);
        File.WriteAllText(directory + fileName, json);
    }

    // Create an array of IJsonable from a json file on disk
    public static T[] ReadJson<T> (string fileName) where T: IJsonable
    {
        //string fullPath = directory + fileName;
        string fullPath = Path.Combine(directory, fileName);

        if (File.Exists(fullPath))
        {
            try
            {
                string json = File.ReadAllText(fullPath);
                T[] saveObjects = JsonHelper.FromJson<T>(json);
                return saveObjects;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
        else
        {
            Debug.LogError("Save file does not exist.");
            return null;
        }
    }


    // Breaks a CSV into string[]s, sends each string[] to T.SetValues()
    public static bool ReadCSV<T>(string fileName) 
        where T : ISaveObject, new()
    {
        StreamReader input = null;
        bool result = false;

        try
        {
            // create stream reader input
            input = File.OpenText(
                Path.Combine(Application.streamingAssetsPath, fileName));

            // populate StatNames from header row 
            string currentLine = input.ReadLine();
            string[] headings = currentLine.Split(',');

            // validate headings?
            T newObject = new T();
            if (!newObject.ValidateHeadings(headings))
                throw new InvalidDataException("Headings do not match");


            // if there are no errors
            currentLine = input.ReadLine();
            while (currentLine != null)
            {
                // parse currentLine into values
                string[] tokens = SplitCSVLine(currentLine);

                // SetValues() will add the new object to Data
                newObject = new T();
                if (!newObject.SetValues(tokens))
                    throw new InvalidDataException(
                        "Invalid CSV data on line starting " + tokens[0]);

                currentLine = input.ReadLine();
            }

            result = true;
        }
        catch(InvalidDataException ex)
        {
            Debug.LogError(ex.Message);
            result = false;
        }
        finally
        {
            input.Close();
        }

        return result;
    }

    // Replacement for string.Split, because , between " need to be ignored.
    public static string[] SplitCSVLine(string input)
    {
        List<string> outputList = new List<string>(50);
        StringBuilder token = new StringBuilder();
        char delimiter = ',';
        char dblQuote = '"';
        bool inQuotes = false;

        if (String.IsNullOrEmpty(input)) return null;

        // read input from left to right
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == dblQuote)
                inQuotes = !inQuotes;
            else if (input[i] == delimiter
                && !inQuotes)
            {
                outputList.Add(token.ToString());
                token.Clear();
            }
            else
                token.Append(input[i]);
        }
        outputList.Add(token.ToString());

        return outputList.ToArray();
    }

}

