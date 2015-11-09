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
using System.Runtime.InteropServices;

namespace Little_System_Cleaner.Misc
{
    internal static class OsVersion
    {
        /// <summary>
        /// Gets the OS version as a name
        /// </summary>
        /// <remarks>TODO: Fix GetVersionEx from always returning 6.2 on Windows 8.1 and Windows 10</remarks>
        /// <returns>Name of OS version</returns>
        internal static string GetOsVersion()
        {
            var osVersionInfo = new PInvoke.OsVersionInfoEx
            {
                dwOSVersionInfoSize = (uint) Marshal.SizeOf(typeof (PInvoke.OsVersionInfoEx))
            };

            var systemInfo = new PInvoke.SystemInfo();
            PInvoke.GetSystemInfo(ref systemInfo);

            var osName = "Microsoft ";

            if (!PInvoke.GetVersionEx(ref osVersionInfo))
                return string.Empty;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                {
                    switch (osVersionInfo.dwMajorVersion)
                    {
                        case 4:
                        {
                            switch (osVersionInfo.dwMinorVersion)
                            {
                                case 0:
                                    if (osVersionInfo.szCSDVersion == "B" ||
                                        osVersionInfo.szCSDVersion == "C")
                                        osName += "Windows 95 R2";
                                    else
                                        osName += "Windows 95";
                                    break;
                                case 10:
                                    if (osVersionInfo.szCSDVersion == "A")
                                        osName += "Windows 98 SE";
                                    else
                                        osName += "Windows 98";
                                    break;
                                case 90:
                                    osName += "Windows ME";
                                    break;
                            }
                        }
                            break;
                    }
                }
                    break;

                case PlatformID.Win32NT:
                {
                    switch (osVersionInfo.dwMajorVersion)
                    {
                        case 3:
                            osName += "Windows NT 3.5.1";
                            break;

                        case 4:
                            switch (osVersionInfo.wProductType)
                            {
                                case 1:
                                    osName += "Windows NT 4.0";
                                    break;
                                case 3:
                                    osName += "Windows NT 4.0 Server";
                                    break;
                            }
                            break;

                        case 5:
                        {
                            switch (osVersionInfo.dwMinorVersion)
                            {
                                case 0:
                                    osName += "Windows 2000";
                                    break;
                                case 1:
                                    osName += "Windows XP";
                                    break;
                                case 2:
                                {
                                    if (osVersionInfo.wSuiteMask == PInvoke.VER_SUITE_WH_SERVER)
                                        osName += "Windows Home Server";
                                    else if (osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION &&
                                             systemInfo.wProcessorArchitecture == PInvoke.PROCESSOR_ARCHITECTURE_AMD64)
                                        osName += "Windows XP Professional";
                                    else
                                        osName += PInvoke.GetSystemMetrics(PInvoke.SM_SERVERR2) == 0
                                            ? "Windows Server 2003"
                                            : "Windows Server 2003 R2";
                                }
                                    break;
                            }
                        }
                            break;

                        case 6:
                        {
                            switch (osVersionInfo.dwMinorVersion)
                            {
                                case 0:
                                    osName += osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION
                                        ? "Windows Vista"
                                        : "Windows Server 2008";
                                    break;

                                case 1:
                                    osName += osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION
                                        ? "Windows 7"
                                        : "Windows Server 2008 R2";
                                    break;

                                case 2:
                                    osName += osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION
                                        ? "Windows 8"
                                        : "Windows Server 2012";
                                    break;

                                case 3:
                                    osName += osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION
                                        ? "Windows 8.1"
                                        : "Windows Server 2012 R2";
                                    break;
                                case 4:
                                    // Windows 10 was originally v6.4 
                                    osName += "Windows 10 (Technical Preview)";
                                    break;
                            }
                        }
                            break;
                        case 10:
                        {
                            switch (osVersionInfo.dwMinorVersion)
                            {
                                case 0:
                                    osName += osVersionInfo.wProductType == PInvoke.VER_NT_WORKSTATION
                                        ? "Windows 10"
                                        : "Windows Server 2016";
                                    break;
                            }
                        }
                            break;
                    }
                }
                    break;
            }

            osName += " ";

            switch (osVersionInfo.dwMajorVersion)
            {
                case 4:
                {
                    switch (osVersionInfo.wProductType)
                    {
                        case PInvoke.VER_NT_WORKSTATION:
                            osName += "Workstation";
                            break;

                        case PInvoke.VER_NT_SERVER:
                            osName += (osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_ENTERPRISE) != 0
                                ? "Enterprise Server"
                                : "Standard Server";
                            break;
                    }
                }
                    break;

                case 5:
                {
                    switch (osVersionInfo.wProductType)
                    {
                        case PInvoke.VER_NT_WORKSTATION:
                            osName += (osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_PERSONAL) != 0 ? "Home" : "Professional";
                            break;

                        case PInvoke.VER_NT_SERVER:
                        {
                            switch (osVersionInfo.dwMinorVersion)
                            {
                                case 0:
                                {
                                    if ((osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_DATACENTER) != 0)
                                        osName += "Data Center Server";
                                    else if ((osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_ENTERPRISE) != 0)
                                        osName += "Advanced Server";
                                    else
                                        osName += "Server";
                                }
                                    break;

                                default:
                                {
                                    if ((osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_DATACENTER) != 0)
                                        osName += "Data Center Server";
                                    else if ((osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_ENTERPRISE) != 0)
                                        osName += "Enterprise Server";
                                    else if ((osVersionInfo.wSuiteMask & PInvoke.VER_SUITE_BLADE) != 0)
                                        osName += "Web Edition";
                                    else
                                        osName += "Standard Server";
                                }
                                    break;
                            }
                        }
                            break;
                    }
                }
                    break;

                case 6:
                {
                    uint ed;
                    if (PInvoke.GetProductInfo(osVersionInfo.dwMajorVersion, osVersionInfo.dwMinorVersion,
                        osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor, out ed))
                    {
                        switch (ed)
                        {
                            case PInvoke.PRODUCT_BUSINESS:
                                osName += "Business";
                                break;
                            case PInvoke.PRODUCT_BUSINESS_N:
                                osName += "Business N";
                                break;
                            case PInvoke.PRODUCT_CLUSTER_SERVER:
                                osName += "HPC Edition";
                                break;
                            case PInvoke.PRODUCT_DATACENTER_SERVER:
                                osName += "Data Center Server";
                                break;
                            case PInvoke.PRODUCT_DATACENTER_SERVER_CORE:
                                osName += "Data Center Server Core";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE:
                                osName += "Enterprise";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_N:
                                osName += "Enterprise N";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_SERVER:
                                osName += "Enterprise Server";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_SERVER_CORE:
                                osName += "Enterprise Server Core Installation";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                osName += "Enterprise Server Without Hyper-V Core Installation";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_SERVER_IA64:
                                osName += "Enterprise Server For Itanium Based Systems";
                                break;
                            case PInvoke.PRODUCT_ENTERPRISE_SERVER_V:
                                osName += "Enterprise Server Without Hyper-V";
                                break;
                            case PInvoke.PRODUCT_HOME_BASIC:
                                osName += "Home Basic";
                                break;
                            case PInvoke.PRODUCT_HOME_BASIC_N:
                                osName += "Home Basic N";
                                break;
                            case PInvoke.PRODUCT_HOME_PREMIUM:
                                osName += "Home Premium";
                                break;
                            case PInvoke.PRODUCT_HOME_PREMIUM_N:
                                osName += "Home Premium N";
                                break;
                            case PInvoke.PRODUCT_HYPERV:
                                osName += "Hyper-V Server";
                                break;
                            case PInvoke.PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                osName += "Essential Business Management Server";
                                break;
                            case PInvoke.PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                osName += "Essential Business Messaging Server";
                                break;
                            case PInvoke.PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                osName += "Essential Business Security Server";
                                break;
                            case PInvoke.PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                osName += "Essential Server Solutions";
                                break;
                            case PInvoke.PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                osName += "Essential Server Solutions Without Hyper-V";
                                break;
                            case PInvoke.PRODUCT_SMALLBUSINESS_SERVER:
                                osName += "Small Business Server";
                                break;
                            case PInvoke.PRODUCT_STANDARD_SERVER:
                                osName += "Standard Server";
                                break;
                            case PInvoke.PRODUCT_STANDARD_SERVER_CORE:
                                osName += "Standard Server Core Installation";
                                break;
                            case PInvoke.PRODUCT_STANDARD_SERVER_CORE_V:
                                osName += "Standard Server Without Hyper-V Core Installation";
                                break;
                            case PInvoke.PRODUCT_STANDARD_SERVER_V:
                                osName += "Standard Server Without Hyper-V";
                                break;
                            case PInvoke.PRODUCT_STARTER:
                                osName += "Starter";
                                break;
                            case PInvoke.PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                osName += "Enterprise Storage Server";
                                break;
                            case PInvoke.PRODUCT_STORAGE_EXPRESS_SERVER:
                                osName += "Express Storage Server";
                                break;
                            case PInvoke.PRODUCT_STORAGE_STANDARD_SERVER:
                                osName += "Standard Storage Server";
                                break;
                            case PInvoke.PRODUCT_STORAGE_WORKGROUP_SERVER:
                                osName += "Workgroup Storage Server";
                                break;
                            case PInvoke.PRODUCT_UNDEFINED:
                                break;
                            case PInvoke.PRODUCT_ULTIMATE:
                                osName += "Ultimate";
                                break;
                            case PInvoke.PRODUCT_ULTIMATE_N:
                                osName += "Ultimate N";
                                break;
                            case PInvoke.PRODUCT_WEB_SERVER:
                                osName += "Web Server";
                                break;
                            case PInvoke.PRODUCT_WEB_SERVER_CORE:
                                osName += "Web Server Core Installation";
                                break;
                            case PInvoke.PRODUCT_PROFESSIONAL:
                                osName += "Professional";
                                break;
                            case PInvoke.PRODUCT_PROFESSIONAL_N:
                                osName += "Professional N";
                                break;
                            case PInvoke.PRODUCT_STARTER_N:
                                osName += "Starter N";
                                break;
                        }
                    }
                }
                    break;
            }

            // If 64 bit OS -> Append (x64)
            if (Environment.Is64BitOperatingSystem)
                osName = osName.Trim() + " (x64)";
            else
            // Otherwise (x86)
                osName = osName.Trim() + " (x86)";

            return osName;
        }
    }
}