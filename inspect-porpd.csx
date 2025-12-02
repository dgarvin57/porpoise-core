#!/usr/bin/env dotnet-script
#r "Porpoise.Core/bin/Debug/net10.0/Porpoise.Core.dll"

using Porpoise.Core.Data;
using Porpoise.Core.DataAccess;
using Porpoise.Core.Models;
using System.Text.Json;

var porpdPath = Args.Length > 0 ? Args[0] : "Porpoise.Api/SampleData/Demo 2015.porpd";
var porpsPath = porpdPath.Replace(".porpd", ".porps");
var porpPath = porpdPath.Replace(".porpd", ".porp");

Console.WriteLine($"Loading files:");
Console.WriteLine($"  Survey: {porpsPath}");
Console.WriteLine($"  Project: {porpPath}");
Console.WriteLine($"  Data: {porpdPath}");
Console.WriteLine();

try
{
    var (survey, project, data) = ProjectLoader.LoadProject(porpsPath, porpPath, porpdPath);
    
    Console.WriteLine("=== SURVEY INFORMATION ===");
    Console.WriteLine($"Survey Name: {survey.SurveyName}");
    Console.WriteLine($"Number of Questions: {survey.QuestionList.Count}");
    Console.WriteLine();
    
    Console.WriteLine("=== FIRST 3 QUESTIONS ===");
    for (int i = 0; i < Math.Min(3, survey.QuestionList.Count); i++)
    {
        var q = survey.QuestionList[i];
        Console.WriteLine($"\nQuestion {i + 1}:");
        Console.WriteLine($"  QstNumber: {q.QstNumber}");
        Console.WriteLine($"  QstLabel: {q.QstLabel}");
        Console.WriteLine($"  QstStem: {q.QstStem}");
        Console.WriteLine($"  DataFileCol: {q.DataFileCol}");
        Console.WriteLine($"  VariableType: {q.VariableType}");
        Console.WriteLine($"  DataType: {q.DataType}");
        Console.WriteLine($"  MissValue1: '{q.MissValue1}'");
        Console.WriteLine($"  MissValue2: '{q.MissValue2}'");
        Console.WriteLine($"  MissValue3: '{q.MissValue3}'");
        Console.WriteLine($"  MissingLow: {q.MissingLow}");
        Console.WriteLine($"  MissingHigh: {q.MissingHigh}");
        Console.WriteLine($"  Responses: {q.Responses.Count}");
        
        if (q.Responses.Count > 0)
        {
            Console.WriteLine($"  First Response:");
            var r = q.Responses[0];
            Console.WriteLine($"    RespValue: {r.RespValue}");
            Console.WriteLine($"    Label: {r.Label}");
            Console.WriteLine($"    IndexType: {r.IndexType}");
        }
    }
    
    Console.WriteLine("\n=== DATA INFORMATION ===");
    Console.WriteLine($"DataList Rows: {data.DataList.Count}");
    if (data.DataList.Count > 0)
    {
        Console.WriteLine($"DataList Columns: {data.DataList[0].Count}");
        Console.WriteLine($"\nFirst row (header):");
        Console.WriteLine($"  {string.Join(", ", data.DataList[0])}");
        
        if (data.DataList.Count > 1)
        {
            Console.WriteLine($"\nSecond row (first data):");
            Console.WriteLine($"  {string.Join(", ", data.DataList[1])}");
        }
    }
    
    Console.WriteLine($"\nMissingResponseValues: {data.MissingResponseValues.Count}");
    if (data.MissingResponseValues.Count > 0)
    {
        Console.WriteLine($"  Values: {string.Join(", ", data.MissingResponseValues)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
