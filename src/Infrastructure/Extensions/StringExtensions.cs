/*
GHOSTS SPECTRE
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0370
*/

using System;
using System.Text.RegularExpressions;

namespace Ghosts.Spectre.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToCondensedLowerCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return input.ToLower();

            var startUnderscores = Regex.Match(input, @"^+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1$2").ToLower();
        }

        public static string Clean(this string x, bool isLinux, bool isOsx)
        {
            //linux path is file:/users
            //windows path is file:/z:
            //ugh
            var fileFormat = "file:\\";
            if (isLinux || isOsx)
            {
                fileFormat = "file:";
            }

            if (x.Contains(fileFormat))
            {
                x = x.Substring(x.IndexOf(fileFormat, StringComparison.InvariantCultureIgnoreCase) + fileFormat.Length);
            }

            x = x.Replace(Convert.ToChar(@"\"), System.IO.Path.DirectorySeparatorChar);
            x = x.Replace(Convert.ToChar(@"/"), System.IO.Path.DirectorySeparatorChar);

            return x;
        }

        public static string AppToDbDirectory(this string appPath)
        {
            return appPath.Replace(Program.Configuration.DataDirectory, Program.Configuration.DataDirectoryDb);
        }
    }
}