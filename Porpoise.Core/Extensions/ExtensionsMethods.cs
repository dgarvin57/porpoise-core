#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Porpoise.Core.Extensions;

/// <summary>
/// Legacy VB.NET extension methods used throughout the original codebase.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Converts a DataTable to List&lt;List&lt;string&gt;&gt; (first list = headers)
    /// </summary>
    public static List<List<string>> ToListOfList(this DataTable dt)
    {
        List<List<string>> newList = [];

        // Get column names
        List<string> colList = [];
        foreach (DataColumn cn in dt.Columns)
            colList.Add(cn.ColumnName);
        newList.Add(colList);

        // Get rows (note: original VB started at index 1, skipping first row)
        for (int rowCount = 1; rowCount < dt.Rows.Count; rowCount++)
        {
            List<string> rowList = [];
            foreach (var item in dt.Rows[rowCount].ItemArray)
                rowList.Add(item?.ToString() ?? string.Empty);
            newList.Add(rowList);
        }

        return newList;
    }

    /// <summary>
    /// Returns the Description attribute of an enum value, or the enum name if none exists.
    /// </summary>
    public static string Description(this Enum enumConstant)
    {
        FieldInfo? fi = enumConstant.GetType().GetField(enumConstant.ToString());


        if (fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] aattr && aattr.Length > 0)
            return aattr[0].Description;

        return enumConstant.ToString();
    }

    /// <summary>
    /// Returns the underlying integer value of an enum.
    /// </summary>
    public static int GetEnumInt<T>(this T enumVal) where T : struct, Enum
    {
        return Convert.ToInt32(enumVal);
    }
}