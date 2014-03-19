﻿/*
    Little System Cleaner
    Copyright (C) 2008 Little Apps (http://www.little-apps.com/)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace Little_System_Cleaner 
{
    public class BadRegistryKey : INotifyPropertyChanged, ICloneable
    {
        #region INotifyPropertyChanged & ICloneable Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion

        #region Properties
        private readonly ObservableCollection<BadRegistryKey> _children = new ObservableCollection<BadRegistryKey>();
        public ObservableCollection<BadRegistryKey> Children
        {
            get { return _children; }
        }

        private BadRegistryKey _parent;
        private bool? _bIsChecked = true;
        private string _strProblem = "";
        private string _strValueName = "";
        private string _strSectionName = "";
        private string _strData = "";
        private int _nSeverity = 1;
        public readonly string baseRegKey = "";
        public readonly string subRegKey = "";

        public Image bMapImg { get; set; }

        /// <summary>
        /// Gets/Sets whether the item is checked
        /// </summary>
        public bool? IsChecked
        {
            get { return _bIsChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        /// <summary>
        /// Gets the section name
        /// </summary>
        public string SectionName
        {
            get { return _strSectionName; }
            set { _strSectionName = value; }
        }

        /// <summary>
        /// Get the problem
        /// </summary>
        public string Problem
        {
            get { return _strProblem; }
        }

        /// <summary>
        /// Gets the value name
        /// </summary>
        public string ValueName
        {
            get { return _strValueName; }
        }

        /// <summary>
        /// Gets the data in the bad registry key
        /// </summary>
        public string Data
        {
            get { return _strData; }
        }

        /// <summary>
        /// Gets the registry path
        /// </summary>
        public string RegKeyPath
        {
            get
            {
                if (!string.IsNullOrEmpty(baseRegKey) && !string.IsNullOrEmpty(subRegKey))
                    return string.Format("{0}\\{1}", baseRegKey, subRegKey);
                else if (!string.IsNullOrEmpty(baseRegKey))
                    return baseRegKey;

                return "";
            }
        }

        /// <summary>
        /// Gets the image showing the severity
        /// </summary>
        public Image SeverityImg
        {
            get
            {
                Image img = new Image();
                System.Drawing.Bitmap bmp;
                if (this._nSeverity == 1)
                {
                    bmp = Properties.Resources._1;
                }
                else if (this._nSeverity == 2)
                {
                    bmp = Properties.Resources._2;
                }
                else if (this._nSeverity == 3)
                {
                    bmp = Properties.Resources._3;
                }
                else if (this._nSeverity == 4)
                {
                    bmp = Properties.Resources._4;
                }
                else if (this._nSeverity == 5)
                {
                    bmp = Properties.Resources._5;
                }
                else
                {
                    // Return blank image (problem root key)
                    return img;
                }

                IntPtr hBitmap = bmp.GetHbitmap();

                img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                return img;
            }
        }

        public object Tag
        {
            get;
            set;
        }

        #region IsChecked Methods
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _bIsChecked)
                return;

            _bIsChecked = value;

            if (updateChildren && _bIsChecked.HasValue)
                this.Children.ToList().ForEach(c => c.SetIsChecked(_bIsChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
        #endregion
        #endregion

        /// <summary>
        /// Constructor for new bad registry key
        /// </summary>
        /// <param name="problem">Reason registry key is invalid</param>
        /// <param name="regPath">Path to registry key (including registry hive)</param>
        /// <param name="valueName">Value Name (can be null)</param> 
        /// <param name="severity">The severity (between 1-5) of the problem</param>
        public BadRegistryKey(string sectionName, string problem, string baseKey, string subKey, string valueName, int severity)
        {
            this._strSectionName = sectionName;
            this._strProblem = problem;
            this.baseRegKey = baseKey;
            this.subRegKey = subKey;
            this._nSeverity = severity;

            if (!string.IsNullOrEmpty(valueName))
            {
                this._strValueName = valueName;

                // Open registry key
                RegistryKey regKey = Utils.RegOpenKey(baseKey, subKey);

                // Convert value to string
                if (regKey != null)
                    this._strData = Utils.RegConvertXValueToString(regKey, valueName);
            }
        }

        /// <summary>
        /// Constructor for root node
        /// </summary>
        public BadRegistryKey(BitmapImage icon, string sectionName)
        {
            this.bMapImg = new Image();

            this.bMapImg.Source = icon;
            this._strProblem = sectionName;

            this._strSectionName = "";
            this._strValueName = "";
            this.baseRegKey = "";
            this.subRegKey = "";
            this._nSeverity = 0;
        }

        public void Init()
        {
            foreach (BadRegistryKey childBadRegKey in this.Children)
            {
                childBadRegKey._parent = this;
                childBadRegKey.Init();
            }
        }

        public override string ToString()
        {
            return string.Copy(RegKeyPath);
        }
    }

    public class BadRegKeyArray : CollectionBase
    {

        public BadRegistryKey this[int index]
        {
            get { return (BadRegistryKey)this.InnerList[index]; }
            set { this.InnerList[index] = value; }
        }

        public int Add(BadRegistryKey BadRegKey)
        {
            if (BadRegKey == null)
                throw new ArgumentNullException("BadRegKey");

            return (this.InnerList.Add(BadRegKey));
        }

        public int IndexOf(BadRegistryKey BadRegKey)
        {
            return (this.InnerList.IndexOf(BadRegKey));
        }

        public void Insert(int index, BadRegistryKey BadRegKey)
        {
            if (BadRegKey == null)
                throw new ArgumentNullException("BadRegKey");

            this.InnerList.Insert(index, BadRegKey);
        }

        public void Remove(BadRegistryKey BadRegKey)
        {
            if (BadRegKey == null)
                throw new ArgumentNullException("BadRegKey");

            this.InnerList.Remove(BadRegKey);
        }

        /// <summary>
        /// Checks if an entry already exists with the same registry key
        /// </summary>
        /// <param name="regPath">Registry key</param>
        /// <param name="valueName">Value Name</param>
        /// <returns>True if it exists</returns>
        public bool Contains(string regPath, string valueName)
        {
            foreach (BadRegistryKey brk in this.InnerList)
            {
                if (string.IsNullOrEmpty(valueName))
                {
                    if (brk.RegKeyPath == regPath)
                        return true;
                }
                else
                {
                    if (brk.RegKeyPath == regPath && brk.ValueName == valueName)
                        return true;
                }
            }

            return false;
        }

        public int Problems(string sectionName)
        {
            int count = 0;

            foreach (BadRegistryKey badRegKey in (ArrayList)this.InnerList.Clone())
            {
                if (badRegKey.SectionName == sectionName)
                    count++;
            }

            return count;
        }

        
    }

}
