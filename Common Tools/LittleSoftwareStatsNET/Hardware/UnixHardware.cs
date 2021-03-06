﻿/*
 * Little Software Stats - .NET Library
 * Copyright (C) 2008-2012 Little Apps (http://www.little-apps.org)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LittleSoftwareStats.Hardware
{
    internal class UnixHardware : Hardware
    {
        public override string CpuName
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/cpuinfo");
                    Regex regex = new Regex(@"(?:model name\s+:\s*)(?<ModelName>[\w \(\)@\.]*)");
                    MatchCollection matches = regex.Matches(output);
                    return matches[0].Groups["ModelName"].Value;
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public override string CpuBrand
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/cpuinfo");
                    Regex regex = new Regex(@"(?:vendor_id\s+:\s*)(?<vendor>\w*)");
                    MatchCollection matches = regex.Matches(output);
                    return matches[0].Groups[1].Value;
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public override double CpuFrequency
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/cpuinfo");
                    Regex regex = new Regex(@"(?:bogomips\s+:\s*)(?<bogomips>\w*)");
                    MatchCollection matches = regex.Matches(output);
                    int bogomips = int.Parse(matches[0].Groups[1].Value);
                    return bogomips/CpuCores;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override int CpuArchitecture
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/cpuinfo");
                    Regex regex = new Regex(@"flags\s+\s:[\w\s]*");
                    MatchCollection matches = regex.Matches(output);
                    string flags = matches[0].Groups[0].Value;
                    if (flags.Contains(" lm"))
                        return 64;
                }
                catch
                {
                    // ignored
                }

                return 32;
            }
        }

        public override int CpuCores
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/cpuinfo");
                    Regex regex = new Regex(@"(?:cpu cores\s+:\s*)(?<num>\w*)");
                    MatchCollection matches = regex.Matches(output);
                    return Int32.Parse(matches[0].Groups[1].Value);
                }
                catch
                {
                    // There has to be at least 1 core, cause how would we be able reach this ???
                    return 1;
                }
            }
        }

        public override double MemoryTotal
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/meminfo");
                    Regex regex = new Regex(@"(?:MemTotal:\s*)(?<memtotal>\d+)");
                    MatchCollection matches = regex.Matches(output);

                    // Convert from KB -> MB
                    return double.Parse(matches[0].Groups[1].Value)/1024;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override double MemoryFree
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("cat", "/proc/meminfo");
                    Regex regex = new Regex(@"(?:MemFree:\s*)(?<memtotal>\d+)");
                    MatchCollection matches = regex.Matches(output);

                    // Convert from KB -> MB
                    return double.Parse(matches[0].Groups[1].Value)/1024;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override long DiskTotal
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("df", "-k");
                    Regex regex = new Regex(@"^/[\w/]*\s*(?<total>\d+)\s*(?<used>\d+)\s*(?<available>\d+)");
                    MatchCollection matches = regex.Matches(output);

                    long total = matches.Cast<Match>().Sum(match => long.Parse(match.Groups["total"].Value));

                    // Convert from KB -> MB
                    return total/1024;
                }
                catch
                {
                    return -1;
                }
            }
        }

        public override long DiskFree
        {
            get
            {
                try
                {
                    string output = Utils.GetCommandExecutionOutput("df", "-B 1k");
                    Regex regex = new Regex(@"^/[\w/]*\s*(?<total>\d+)\s*(?<used>\d+)\s*(?<available>\d+)");
                    MatchCollection matches = regex.Matches(output);

                    long total = matches.Cast<Match>().Sum(match => long.Parse(match.Groups["available"].Value));

                    // Convert from KB -> MB
                    return total/1024;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override string ScreenResolution
        {
            get
            {
                try
                {
                    int deskHeight = Screen.PrimaryScreen.Bounds.Height;
                    int deskWidth = Screen.PrimaryScreen.Bounds.Width;
                    return deskWidth + "x" + deskHeight;
                }
                catch
                {
                    return "800x600";
                }
            }
        } 
    }
}
