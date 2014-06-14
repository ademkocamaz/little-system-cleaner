﻿using Little_System_Cleaner.Privacy_Cleaner.Controls;
using Little_System_Cleaner.Privacy_Cleaner.Helpers.Results;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Little_System_Cleaner.Privacy_Cleaner.Helpers
{
    public class PluginFunctions
    {
        private readonly Dictionary<RegistryKey, string[]> regKeyValueNames;
        private readonly Dictionary<RegistryKey, bool> regKeySubKeys;
        private readonly Dictionary<string, bool> dictFolders;
        private readonly List<string> filePathList;
        private readonly List<INIInfo> iniInfoList;
        private readonly Dictionary<string, List<string>> dictXmlPaths;

        public Dictionary<RegistryKey, string[]> RegistryValueNames 
        {
            get
            {
                return regKeyValueNames;
            }
        }

        public Dictionary<RegistryKey, bool> RegistrySubKeys
        {
            get
            {
                return regKeySubKeys;
            }
        }
        public Dictionary<string, bool> Folders
        {
            get
            {
                return dictFolders;
            }
        }

        public List<string> FilePaths
        {
            get
            {
                return filePathList;
            }
        }

        public List<INIInfo> INIList
        {
            get
            {
                return iniInfoList;
            }
        }

        public Dictionary<string, List<string>> XmlPaths
        {
            get
            {
                return dictXmlPaths;
            }
        }

        public PluginFunctions()
        {
            regKeyValueNames = new Dictionary<RegistryKey, string[]>();
            regKeySubKeys = new Dictionary<RegistryKey, bool>();
            dictFolders = new Dictionary<string, bool>();
            filePathList = new List<string>();
            iniInfoList = new List<INIInfo>();
            dictXmlPaths = new Dictionary<string, List<string>>();
        }

        public void DeleteKey(RegistryKey regKey, bool recurse)
        {
            if (regKey == null)
                return;

            Wizard.CurrentFile = regKey.Name;

            RegistrySubKeys.Add(regKey, recurse);
        }

        public void DeleteValue(RegistryKey regKey, string searchText)
        {
            List<string> valueNames = new List<string>();

            if (regKey == null)
                return;

            Wizard.CurrentFile = regKey.Name;

            string[] regValueNames = null;

            try
            {
                regValueNames = regKey.GetValueNames();
            }
            catch (System.Security.SecurityException ex)
            {
                Debug.WriteLine("The following error occurred: " + ex.Message + "\nUnable to get value names for " + regKey.ToString());
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: " + ex.Message + "\nUnable to get value names for " + regKey.ToString());
            }

            if (regValueNames == null)
                return;

            // Get value names that match regex
            foreach (string valueName in regValueNames)
            {
                if (Regex.IsMatch(valueName, searchText))
                    valueNames.Add(valueName);
            }

            if (!RegistryValueNames.ContainsKey(regKey))
                // Create new entry if regkey doesnt exist
                RegistryValueNames.Add(regKey, valueNames.ToArray());
            else
            {
                // Append value names to existing entry
                valueNames.AddRange(RegistryValueNames[regKey]);

                RegistryValueNames[regKey] = valueNames.ToArray();
            }
        }

        public void DeleteFile(string filePath)
        {
            Wizard.CurrentFile = filePath;

            AddToFiles(filePath);
        }

        public void DeleteFolder(string folderPath, bool recurse)
        {
            Wizard.CurrentFile = folderPath;

            AddToFolders(folderPath, recurse);
        }

        public void DeleteFileList(string searchPath, string searchText, SearchOption includeSubFolders)
        {
            // Skip if search path doesnt exist
            if (!Directory.Exists(searchPath))
                return;

            string[] fileList = null;

            try
            {
                fileList = Directory.GetFiles(searchPath, searchText, includeSubFolders);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of files.", ex.Message);
            }
            catch (PathTooLongException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of files.", ex.Message);
            }

            if (fileList != null)
            {
                foreach (string filePath in fileList)
                {
                    Wizard.CurrentFile = filePath;

                    AddToFiles(filePath);
                }
            }
        }

        public void DeleteFolderList(string searchPath, string searchText, SearchOption includeSubFolders)
        {
            // Skip if search path doesnt exist
            if (!Directory.Exists(searchPath))
                return;

            string[] dirList = null;

            try
            {
                dirList = Directory.GetDirectories(searchPath, searchText, includeSubFolders);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of directories.", ex.Message);
            }
            catch (PathTooLongException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of directories.", ex.Message);
            }

            if (dirList != null)
            {
                foreach (string folderPath in dirList)
                {
                    Wizard.CurrentFile = folderPath;

                    AddToFolders(folderPath, false);
                }
            }
        }

        public void DeleteFoundRegKeys(RegistryKey regKey, bool includeSubKeys, XmlReader xmlChildren)
        {
            if (regKey == null)
                return;

            Dictionary<string, bool> regexSubKeys = new Dictionary<string, bool>();
            List<string> regexValueNames = new List<string>();
            Dictionary<RegistryKey, string[]> valueNames = new Dictionary<RegistryKey, string[]>();
            Dictionary<RegistryKey, bool> subKeys = new Dictionary<RegistryKey, bool>();

            while (xmlChildren.Read())
            {
                if (xmlChildren.Name == "IfSubKey")
                {
                    string searchText = xmlChildren.GetAttribute("SearchText");
                    bool recurse = ((xmlChildren.GetAttribute("Recursive") == "Y") ? (true) : (false));

                    regexSubKeys.Add(searchText, recurse);
                }
                else if (xmlChildren.Name == "IfValueName")
                {
                    string searchText = xmlChildren.GetAttribute("SearchText");
                    regexValueNames.Add(searchText);
                }
            }

            valueNames = RecurseRegKeyValueNames(regKey, regexValueNames, includeSubKeys);
            subKeys = RecurseRegKeySubKeys(regKey, regexSubKeys, includeSubKeys);

            if (valueNames.Count > 0)
            {
                foreach (KeyValuePair<RegistryKey, string[]> kvp in valueNames)
                    RegistryValueNames.Add(kvp.Key, kvp.Value);
            }

            if (subKeys.Count > 0)
            {
                foreach (KeyValuePair<RegistryKey, bool> kvp in subKeys)
                    RegistrySubKeys.Add(kvp.Key, kvp.Value);
            }
        }

        public void DeleteFoundPaths(string searchPath, string searchText, SearchOption includeSubFolders, XmlReader xmlChildren)
        {
            List<string> regexFiles = new List<string>();
            Dictionary<string, bool> regexFolders = new Dictionary<string, bool>();

            while (xmlChildren.Read())
            {
                if (xmlChildren.Name == "IfFile")
                {
                    string fileName = xmlChildren.GetAttribute("SearchText");
                    if (!string.IsNullOrEmpty(fileName))
                        regexFiles.Add(fileName);
                }
                else if (xmlChildren.Name == "IfFolder")
                {
                    string folderName = xmlChildren.GetAttribute("SearchText");
                    bool recurse = ((xmlChildren.GetAttribute("Recursive") == "Y") ? (true) : (false));

                    if (!string.IsNullOrEmpty(folderName))
                        regexFolders.Add(folderName, recurse);
                }
            }

            // Skip if search path doesnt exist or the lists are empty
            if (!Directory.Exists(searchPath)|| (regexFiles.Count == 0 && regexFolders.Count == 0))
                return;

            string[] dirList = null;

            try
            {
                dirList = Directory.GetDirectories(searchPath, searchText, includeSubFolders);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of directories.", ex.Message);
            }
            catch (PathTooLongException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get list of directories.", ex.Message);
            }

            if (dirList == null)
                return;

            foreach (string folderPath in dirList)
            {
                string[] fileList = null;

                Wizard.CurrentFile = folderPath;
                string folderName = folderPath.Substring(Path.GetDirectoryName(folderPath).Length + 1);

                // Iterate through the files and folders in the current folder
                foreach (KeyValuePair<string, bool> kvp in regexFolders)
                {
                    if (Regex.IsMatch(folderName, kvp.Key))
                        AddToFolders(folderPath, kvp.Value);
                }

                try
                {
                    fileList = Directory.GetFiles(folderPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("The following error occurred: {0}\nSkipping trying to get list of files", ex.Message);
                }
                catch (PathTooLongException)
                {
                    Debug.WriteLine("Skipping directory ({0}) because the length is too long.", folderPath);
                }

                if (fileList == null)
                    continue;

                foreach (string filePath in fileList)
                {
                    if (string.IsNullOrEmpty(filePath))
                        continue;

                    // Get filename from file path
                    string fileName = Path.GetFileName(filePath);

                    foreach (string regex in regexFiles)
                    {
                        if (string.IsNullOrEmpty(regex))
                            continue;

                        if (Regex.IsMatch(fileName, regex))
                        {
                            AddToFiles(filePath);
                            break;
                        }
                    }
                }
            }
        }

        public void DeleteINIValue(string filePath, string searchSectionText, string searchValueNameText)
        {
            if (!File.Exists(filePath))
                return;

            foreach (string sectionName in Utils.GetSections(filePath))
            {
                if (string.IsNullOrEmpty(sectionName))
                    continue;

                if (Regex.IsMatch(sectionName, searchSectionText))
                {
                    foreach (KeyValuePair<string, string> kvp in Utils.GetValues(filePath, sectionName))
                    {
                        if (Regex.IsMatch(kvp.Key, searchValueNameText))
                        {
                            INIList.Add(new INIInfo() { filePath = filePath, sectionName = sectionName, valueName = kvp.Key });
                        }
                    }
                }
            }
        }

        public void DeleteINISection(string filePath, string searchSectionText)
        {
            if (!File.Exists(filePath))
                return;

            foreach (string sectionName in Utils.GetSections(filePath))
            {
                if (string.IsNullOrEmpty(sectionName))
                    continue;

                if (Regex.IsMatch(sectionName, searchSectionText))
                {
                    INIList.Add(new INIInfo() { filePath = filePath, sectionName = sectionName });
                }
            }
        }

        public void DeleteXml(string filePath, string xPath)
        {
            if (!File.Exists(filePath))
                return;

            AddToXmlPaths(filePath, xPath);
        }

        Dictionary<RegistryKey, string[]> RecurseRegKeyValueNames(RegistryKey regKey, List<string> regexValueNames, bool recurse)
        {
            Dictionary<RegistryKey, string[]> ret = new Dictionary<RegistryKey, string[]>();
            List<string> valueNames = new List<string>();

            if (regKey == null || regexValueNames.Count == 0)
                return ret;

            string[] regValueNames = null;

            try
            {
                regValueNames = regKey.GetValueNames();
            }
            catch (System.Security.SecurityException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get registry key value names.", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get registry key value names.", ex.Message);
            }

            if (regValueNames == null)
            {
                return ret;
            }

            foreach (string valueName in regValueNames)
            {
                foreach (string regex in regexValueNames)
                {
                    if (Regex.IsMatch(valueName, regex) && (!valueNames.Contains(valueName)))
                    {
                        valueNames.Add(valueName);
                        break;
                    }
                }
            }

            if (recurse)
            {
                string[] subKeys = null;

                try
                {
                    subKeys = regKey.GetSubKeyNames();
                }
                catch (System.Security.SecurityException ex)
                {
                    Debug.WriteLine("The following error occurred: {0}\nUnable to get registry key sub keys.", ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("The following error occurred: {0}\nUnable to get registry key sub keys.", ex.Message);
                }

                if (subKeys == null)
                {
                    return ret;
                }

                foreach (string subKey in subKeys)
                {
                    RegistryKey subRegKey = null;

                    try
                    {
                        subRegKey = regKey.OpenSubKey(subKey);
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        Debug.WriteLine("The following error occurred: {0}\nUnable to open sub key.", ex.Message);
                    }

                    if (subRegKey != null)
                    {
                        foreach (KeyValuePair<RegistryKey, string[]> kvp in RecurseRegKeyValueNames(subRegKey, regexValueNames, recurse))
                            ret.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            if (valueNames.Count > 0)
                ret.Add(regKey, valueNames.ToArray());

            return ret;
        }

        Dictionary<RegistryKey, bool> RecurseRegKeySubKeys(RegistryKey regKey, Dictionary<string, bool> regexSubKeys, bool recurse)
        {
            Dictionary<RegistryKey, bool> ret = new Dictionary<RegistryKey, bool>();

            if (regKey == null || regexSubKeys.Count == 0)
                return ret;

            string[] subKeys = null;

            try
            {
                subKeys = regKey.GetSubKeyNames();
            }
            catch (System.Security.SecurityException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get sub keys.", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("The following error occurred: {0}\nUnable to get sub keys.", ex.Message);
            }

            if (subKeys == null)
                return ret;

            foreach (string subKeyName in subKeys)
            {
                foreach (KeyValuePair<string, bool> kvp in regexSubKeys)
                {
                    if (Regex.IsMatch(subKeyName, kvp.Key))
                    {
                        RegistryKey subKey = null;

                        try
                        {
                            subKey = regKey.OpenSubKey(subKeyName, true);
                        }
                        catch (System.Security.SecurityException ex)
                        {
                            Debug.WriteLine("The following error occurred: {0}\nUnable to open sub key.", ex.Message);
                        }

                        if (subKey != null)
                        {
                            ret.Add(subKey, kvp.Value);
                            break;
                        }
                    }
                }
            }

            if (recurse)
            {
                string[] recurseSubKeys = null;

                try
                {
                    recurseSubKeys = regKey.GetSubKeyNames();
                }
                catch (System.Security.SecurityException ex)
                {
                    Debug.WriteLine("The following error occurred: {0}\nUnable to get sub keys.", ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("The following error occurred: {0}\nUnable to get sub keys.", ex.Message);
                }

                if (recurseSubKeys == null)
                    return ret;

                foreach (string subKey in recurseSubKeys)
                {
                    RegistryKey subRegKey = null;

                    try
                    {
                        subRegKey = regKey.OpenSubKey(subKey, true);
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        Debug.WriteLine("The following error occurred: {0}\nUnable to open sub key.", ex.Message);
                    }

                    if (subRegKey != null)
                    {
                        foreach (KeyValuePair<RegistryKey, bool> kvp in RecurseRegKeySubKeys(subRegKey, regexSubKeys, recurse))
                            ret.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Adds a folder to the results
        /// </summary>
        /// <param name="folderPath">Folder path</param>
        /// <param name="recurse">True to recurse when removing folder</param>
        private void AddToFolders(string folderPath, bool recurse)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            folderPath = folderPath.Trim();

            if (!Directory.Exists(folderPath))
                return;

            string cleanFolderPath = null;

            try
            {
                cleanFolderPath = Path.GetDirectoryName(folderPath);
            }
            catch (PathTooLongException)
            {
                Debug.WriteLine("Unable to get clean folder path because the length is too long");
                return;
            }

            // Check if folder is root directory (THIS IS DANGEROUS)
            try
            {
                string rootDir = Directory.GetDirectoryRoot(cleanFolderPath);

                if (rootDir == cleanFolderPath)
                    return;
            }
            catch (Exception ex)
            {
                Debug.Write("The following error occurred: {0}\nUnable to determine root folder.", ex.Message);
                return;
            } 

            if (!FolderAlreadyAdded(cleanFolderPath))
                Folders.Add(cleanFolderPath, recurse);
        }

        private void AddToFiles(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            filePath = filePath.Trim();

            if (!File.Exists(filePath))
                return;

            if (!FolderAlreadyAdded(filePath))
            {
                this.FilePaths.Add(filePath);
            }
        }

        private void AddToXmlPaths(string filePath, string xPath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(xPath))
                return;

            filePath = filePath.Trim();
            xPath = xPath.Trim();

            if (!this.XmlPaths.ContainsKey(filePath))
            {
                this.XmlPaths.Add(filePath, new List<string>(new string[] { xPath }));
            }
            else
            {
                List<string> xPaths = this.XmlPaths[filePath];

                if (xPaths.Contains(xPath))
                    // Already added
                    return;

                xPaths.Add(xPath);

                this.XmlPaths[filePath] = xPaths;
            }
        }

        /// <summary>
        /// Checks if the folder (from the path) is already added as a recursive directory
        /// </summary>
        /// <param name="path">File or folder path</param>
        /// <param name="startDir">Is it the starting directory?</param>
        /// <returns>True if it's been added</returns>
        private bool FolderAlreadyAdded(string path, bool startDir = true)
        {
            string actualFolder;

            try
            {
                actualFolder = Path.GetDirectoryName(path);
            }
            catch (PathTooLongException)
            {
                Debug.WriteLine("Unable to get directory from {0} because it is too long", path);
                return false;
            }

            if (string.IsNullOrEmpty(actualFolder))
                // Unable to get directory name, use parameter
                actualFolder = path.Trim();

            if (startDir)
            {
                if (Folders.ContainsKey(actualFolder))
                    return false;
            }
            else
            {
                // Parent folders need to have recurse set to true
                if (Folders.Contains(new KeyValuePair<string, bool>(actualFolder, true)))
                    return false;
            }
            
            // Check parent folders
            DirectoryInfo diParent = null;

            try
            {
                diParent = Directory.GetParent(actualFolder);
            }
            catch (Exception)
            {
            }

            if (diParent != null)
                return FolderAlreadyAdded(diParent.ToString(), false);
            else 
                // No parent folder or an exception occurred
                return true;
        }
    }
}