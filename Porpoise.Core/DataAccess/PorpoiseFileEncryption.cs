using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Porpoise.Core.Models;

namespace Porpoise.Core.DataAccess
{
    /// <summary>
    /// Handles encryption/decryption and file I/O for legacy Porpoise file formats (.porp, .porps, .porpd)
    /// </summary>
    public static class PorpoiseFileEncryption
    { 
        #region P R I V A T E  M E M B E R S

            // PROFILE KEY = Brown + Orange + Green + Violet (16 bytes)
            private static readonly byte[] _parmKey = new byte[]
            {
                0x13, 0xBF, 0x42, 0xFE, // Brown
                0xA1, 0xF2, 0xE7, 0xD6, // Orange
                0xE1, 0xD6, 0x41, 0x77, // Green
                0xAF, 0x82, 0x9D, 0xFB  // Violet
            };

            // RESPONSE IV = Black + Red + Yellow + Blue (16 bytes)
            private static readonly byte[] _parmIV = new byte[]
            {
                0xBA, 0x3F, 0x23, 0x8E, // Black
                0x0F, 0x1E, 0x92, 0xFD, // Red
                0xEA, 0xFB, 0x7A, 0x50, // Yellow
                0x49, 0x3B, 0x4C, 0xF2  // Blue
            };
        #endregion // P R I V A T E  M E M B E R S
        
        #region P U B L I C  M E T H O D S

        /// <summary>
        /// Reads and decrypts a generic object from a legacy Porpoise encrypted file
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="path">Path to the file</param>
        /// <returns>Deserialized object of type T</returns>
        /// <exception cref="InvalidOperationException">If file cannot be read</exception>
        /// <exception cref="NotSupportedException">If text files are encountered</exception>
        private static T ReadEncryptedFile<T>(string path) where T : class
        {
            var pFile = ReadFile(path) ?? throw new InvalidOperationException("Failed to read file.");
            
            if (pFile.FileType == PFileType.Binary)
            {
                var fileText = Uncolorize(pFile.Content);
                return fileText.XmlDeserializeFromString<T>();
            }
            else
            {
                throw new NotSupportedException("Text Porpoise files are not supported.");
            }
        }

        /// <summary>
        /// Reads and decrypts a Survey from a legacy Porpoise file (.porps)
        /// </summary>
        /// <param name="path">Path to the survey file</param>
        /// <returns>Deserialized Survey object</returns>
        public static Survey ReadSurvey(string path)
        {
            return ReadEncryptedFile<Survey>(path);
        }

        /// <summary>
        /// Reads and decrypts a Project from a legacy Porpoise file (.porp)
        /// </summary>
        /// <param name="path">Path to the project file</param>
        /// <returns>Deserialized Project object</returns>
        public static Project ReadProject(string path)
        {
            return ReadEncryptedFile<Project>(path);
        }

        /// <summary>
        /// Reads and decrypts SurveyData from a legacy Porpoise file (.porpd)
        /// </summary>
        /// <param name="path">Path to the data file</param>
        /// <returns>Deserialized SurveyData object</returns>
        public static SurveyData ReadData(string path)
        {
            var pFile = ReadFile(path) ?? throw new InvalidOperationException("Failed to read data file.");
            
            if (pFile.FileType == PFileType.Binary)
            {
                var fileText = Uncolorize(pFile.Content);
                
                // The .porpd file contains a DataSet XML, not SurveyData XML
                // We need to deserialize it as a DataSet and convert to SurveyData
                var dataSet = new System.Data.DataSet();
                using (var reader = new System.IO.StringReader(fileText))
                {
                    dataSet.ReadXml(reader);
                }
                
                if (dataSet.Tables.Count == 0)
                    throw new InvalidOperationException("Data file contains no tables");
                
                var dataTable = dataSet.Tables[0];
                
                // Convert DataTable to List<List<string>>
                var surveyData = new SurveyData();
                var dataList = surveyData.DataTableToListOfList(dataTable);
                
                // Create a new SurveyData with the converted data
                return new SurveyData(dataList);
            }
            else
            {
                throw new NotSupportedException("Text Porpoise data files are not supported.");
            }
        }

        /// <summary>
        /// Read any file and determine if a) it has a header, or b) it's a plain text file
        /// If it has a header, determine if its binary or text and route accordingly
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static CPFileInfo? ReadFile(string filepath)
        {
            using var fs = new FileStream(filepath, FileMode.Open);
            var getBytes = new byte[25];
            fs.ReadExactly(getBytes, 0, 25);
            
            var header = Encoding.ASCII.GetString(getBytes);
            fs.Close();

            if (header.Contains("PORPOISE_") && header.Contains("EXPORTFILE="))
            {
                // File has Porpoise header
                if (header.Contains("PORPOISE_BIN")) {
                    CPFileInfo? fileInfo = ReadFromBinary(filepath);
                    return fileInfo;
                }
            }
            return ReadFromText(filepath);
        }

        /// <summary>
        /// Indicates if file at filepath has a Porpoise header
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool IsFilePorpoiseBinary(string filepath)
        {
            var reader = new BinaryReader(new StreamReader(filepath).BaseStream);
            var getBytes = reader.ReadBytes(25);
            var header = Encoding.ASCII.GetString(getBytes);
            reader.Close();
            // Check if we have a header
            return header.Contains("PORPOISE_") && header.Contains("EXPORTFILE=");
        }

        // Read from a given binary filepath and return a string
        public static CPFileInfo? ReadFromBinary(string filePath)
        {
            // Validate passed parameters
            if (!ValidateReadParameters(filePath)) return null;
            // Valid parameters
            using var fsRead = new FileStream(filePath, FileMode.Open);
            var br = new BinaryReader(fsRead, Encoding.UTF8);
            
            // Binary format uses length-prefixed strings (BinaryWriter.Write format)
            var readHeader = br.ReadString();  // Reads "PORPOISE_BIN/EXPORTFILE=F" (or T)
            var readRest = br.ReadString();    // Reads the encrypted content
            
            return BuildFileInfo(readHeader + readRest);
        }

        // Read from a given text filepath and return a string
        public static CPFileInfo? ReadFromText(string filePath)
        {
            // Validate passed parameters
            if (!ValidateReadParameters(filePath)) return null;
            // Valid parameters
            using var sw = new StreamReader(filePath);
            // Read text file
            var readTextString = sw.ReadToEnd();
            return BuildFileInfo(readTextString);
        }

        /// <summary>
        /// Write given text to given filepath as a binary file 
        /// </summary>
        /// <param name="fileText"></param>
        /// <param name="filePath"></param>
        /// <param name="exportFile">Is this file being exported</param>
        public static void WriteToBinary(string fileText, string filePath, Boolean exportFile)
        {
            // Validate passed parameters
            if (!ValidateWriteParameters(fileText, filePath)) return;
            // Valid parameters
            var fs = new FileStream(filePath, FileMode.Create);
            try
            {
                // Write out to a binary file
                using (fs)
                {
                    var bw = new BinaryWriter(fs, Encoding.UTF8);
                    bw.Write(string.Format("PORPOISE_BIN/EXPORTFILE={0}", exportFile?"T":"F"));
                    bw.Write(fileText);
                }
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }

        // Write given text to given filepath as a text file
        public static void WriteToText(string fileText, string filePath, Boolean exportFile)
        {
            // Validate passed parameters
            if (!ValidateWriteParameters(fileText, filePath)) return;
            // Valid parameters
            using var sw = new StreamWriter(filePath);
            // Write out as text file
            sw.Write(string.Format("PORPOISE_TXT/EXPORTFILE={0}", exportFile ? "T" : "F"));
            sw.Write(fileText);
        }

        #endregion // Public methods

        #region P R I V A T E  M E T H O D S

               // Obscures any string
        private static string Colorize(string inputText )
        {
            //Create a new RijndaelManaged cipher for the symmetric algorithm from the key and iv
#pragma warning disable SYSLIB0022 // Type or member is obsolete
            var rijndaelCipher = new RijndaelManaged { Key = _parmKey, IV = _parmIV };
#pragma warning restore SYSLIB0022 // Type or member is obsolete

            var plainText = Encoding.Unicode.GetBytes(inputText);

            using (var encryptor = rijndaelCipher.CreateEncryptor())
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        // Unobscures a previously obscured string.
        private static string Uncolorize(string inputText )
        {
#pragma warning disable SYSLIB0022 // Type or member is obsolete
            using var rijndaelCipher = new RijndaelManaged { Key = _parmKey, IV = _parmIV };
#pragma warning restore SYSLIB0022 // Type or member is obsolete
            
            byte[] encryptedData = Convert.FromBase64String(inputText);

            using var decryptor = rijndaelCipher.CreateDecryptor(_parmKey, _parmIV);
            using var memoryStream = new MemoryStream(encryptedData);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();
            cryptoStream.CopyTo(resultStream);
            var plainText = resultStream.ToArray();
            
            return Encoding.Unicode.GetString(plainText);
        }


        // Validate file write parameters
        private static Boolean ValidateWriteParameters(string fileText, string filePath)
        {
            // Validate passed parameters
            if (fileText == null) throw new ArgumentNullException("fileText");
            if (filePath == null) throw new ArgumentNullException("filePath");

            var dir = Path.GetDirectoryName(filePath);
            if (dir == null || !Directory.Exists(dir))
            {
                // Directory in path doesn't exists
                if (dir == null) throw new FileNotFoundException("File does not exist", filePath);
                throw new ArgumentOutOfRangeException(string.Format("Directory doesn't exist in '{0}'", dir));
            }
            return true;

        }

        // Validate file read parameters
        private static Boolean ValidateReadParameters(string filePath)
        {
            // Validate passed parameters
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath)) throw new FileNotFoundException("File does not exist", filePath);
            return true;
        }

        // From content string, build file info object
        private static CPFileInfo BuildFileInfo(string content)
        {
            // Build file info object
            var header = content.Substring(0, 25);
            if (header.Contains("PORPOISE_"))
            {
                // This file has a header
                return new CPFileInfo
                {
                    FileType = header.Substring(0, 12) == "PORPOISE_BIN" ? PFileType.Binary : PFileType.Text,
                    Exported = header.Substring(13, 12) == "EXPORTFILE=T",
                    Content = content.Substring(25, content.Length - 25)
                };
            }
            else
            {
                // No header; treat as normal text file
                return new CPFileInfo
                {
                    FileType = PFileType.Text,
                    Exported = false,
                    Content = content
                };
            }
        }

        #endregion //Private Methods

    }
}
