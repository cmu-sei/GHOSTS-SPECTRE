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
using System.IO;
using System.Text;
using Dapper;
using Ghosts.Spectre.Infrastructure.Extensions;
using Ghosts.Spectre.Infrastructure.Services;
using Npgsql;

namespace Ghosts.Spectre.Infrastructure.ML
{
    public static class Loaders
    {
        internal static void LoadSeedData()
        {
            var dir = Directory.CreateDirectory($"{Configuration.BaseDirectory}{Path.DirectorySeparatorChar}tmp");
            var seedFile =
                $"{ConfigurationService.InstalledPath}{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}sites.csv";

            var importFile = $"{dir}{Path.DirectorySeparatorChar}sites.csv";
            File.Move(seedFile, importFile, true);

            using (var connection = new NpgsqlConnection(Program.Configuration.ConnectionString))
            {
                connection.Open();


                var sb = new StringBuilder();
                var lines = File.ReadLines(importFile.AppToDbDirectory());
                foreach (var line in lines)
                {
                    var lineArray = line.Split(',');
                    sb.AppendFormat(
                            $"insert into ml_sites (globalrank, tldrank, domain, tld, refsubnets, refips, idn_domain, idn_tld, prevglobalrank, prevtldrank, prevrefsubnets, prevrefips) VALUES ('{importFile.AppToDbDirectory()}', '{line[0]}', '{line[1]}', '{line[2]}', '{line[3]}', '{line[4]}', '{line[5]}', '{line[6]}', '{line[7]}', '{line[8]}', '{line[9]}', '{line[10]}');")
                        .Append(Environment.NewLine);
                }

                connection.Execute(sb.ToString());

                seedFile =
                    $"{ConfigurationService.InstalledPath}{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}categories.csv";
                importFile = $"{dir}{Path.DirectorySeparatorChar}categories.csv";

                File.Move(seedFile, importFile, true);

                sb = new StringBuilder();
                lines = File.ReadLines(importFile.AppToDbDirectory());
                foreach (var line in lines)
                {
                    var lineArray = line.Split(',');
                    sb.AppendFormat($"insert into ml_categories (url, cats) VALUES ('{line[0]}', '{line[1]}');").Append(Environment.NewLine);
                }

                connection.Execute(sb.ToString());
            }

            Directory.Delete(dir.FullName, true);
        }
    }
}