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
using NLog;
using Npgsql;

namespace Ghosts.Spectre.Infrastructure.ML
{
    internal static class Generators
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static void EnsureCreated(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (Path.GetExtension(path).Length > 0)
            {
                path = Path.GetDirectoryName(path);
            }
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal static int GenerateNewBrowseFiles(Configuration config)
        {
            EnsureCreated(config.InputFilePref.AppToDbDirectory());
            EnsureCreated(config.InputFileRand.AppToDbDirectory());

            using var connection = new NpgsqlConnection(Program.Configuration.ConnectionString);
            connection.Open();

            connection.Execute("truncate table ml_agent_browse_history;");
            connection.Execute("call create_preferenced();");
            connection.Execute("truncate table ml_agent_browse_history_random;");
            connection.Execute("call create_random();");

            var s = @"select user_id, item_id,
                            CASE
                                WHEN vw_agentprefs.preference = category THEN 5
                                ELSE 1
                                END
                            AS rating,
                            timestamp, 0 as iteration
                        from vw_browsehistory as b
                            right join vw_agentprefs on vw_agentprefs.agentid = b.agentid
                        order by timestamp;";

            var sb = new StringBuilder("user_id,item_id,rating,timestamp,iteration");
            sb.Append(Environment.NewLine);
            var i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["user_id"]).Append(',').Append(r["item_id"]).Append(',').Append(r["rating"]).Append(',').Append(r["timestamp"])
                        .Append(',').Append(r["iteration"]).Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.InputFilePref.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.InputFilePref.AppToDbDirectory()} from vw_agentprefs");

            sb = new StringBuilder("user_id,item_id");
            sb.Append(Environment.NewLine);
            s = @"select user_id, item_id,
                            CASE
                                WHEN vw_agentprefs.preference = category THEN 5
                                ELSE 1
                                END
                            AS rating,
                            timestamp, 0 as iteration
                        from vw_browsehistory_random as b
                            right join vw_agentprefs on vw_agentprefs.agentid = b.agentid
                        order by timestamp;";
            i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["user_id"]).Append(',').Append(r["item_id"]).Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.InputFileRand.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.InputFileRand.AppToDbDirectory()} from vw_agentprefs");
            return i;
        }

        internal static int GenerateSitesFile(Configuration config)
        {
            EnsureCreated(config.SitesFile.AppToDbDirectory());

            using var connection = new NpgsqlConnection(Program.Configuration.ConnectionString);
            connection.Open();

            var sb = new StringBuilder("site_id,cats");
            sb.Append(Environment.NewLine);
            var s = @"select s.id as site_id, c.cats
                        from ml_sites as s, ml_categories as c
                        where s.domain = c.url and s.id < 500000 and c.cats not like '%|%';";
            var i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["site_id"]).Append(',').Append(r["cats"]).Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.SitesFile.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.SitesFile.AppToDbDirectory()} from ml_sites, ml_categories");
            return i;
        }

        internal static int GenerateAgentsFile(Configuration config)
        {
            EnsureCreated(config.AgentsFile.AppToDbDirectory());

            using var connection = new NpgsqlConnection(Program.Configuration.ConnectionString);
            connection.Open();

            var s = @"select a2.cloudid, preference, score
                        from vw_agentprefs as a, agents as a2
                        where a.agentid = a2.id
                        order by cloudid;";
            var sb = new StringBuilder("cloudid,preference,score");
            sb.Append(Environment.NewLine);
            var i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["cloudid"]).Append(',').Append(r["preference"]).Append(',').Append(r["score"]).Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.AgentsFile.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.AgentsFile.AppToDbDirectory()} from vw_agentprefs, agents");
            return i;
        }

        internal static void GenerateReportFile(Configuration config)
        {
            EnsureCreated(config.Campaign.AppToDbDirectory());
            EnsureCreated(config.ReportFile.AppToDbDirectory());
            EnsureCreated(config.ResultFile.AppToDbDirectory());

            using var connection = new NpgsqlConnection(Program.Configuration.ConnectionString);
            connection.Open();

            connection.Execute("truncate table ml_learned_import_extended;");

            //load ML results file
            var sb = new StringBuilder();
            var lines = File.ReadLines(config.Campaign.AppToDbDirectory());
            var i = 0;
            foreach (var line in lines)
            {
                var lineArray = line.Split(',');
                sb.AppendFormat($"insert into ml_learned_recommendations (campaign, cloudid, itemid, created, iteration) values ('{config.Campaign.AppToDbDirectory()}', '{lineArray[0]}', '{lineArray[1]}', '{lineArray[2]}', '{lineArray[3]}');");
                i++;
            }
            connection.Execute(sb.ToString());
            log.Trace($"Inserted {i} records to ml_learned_recommendations from {config.Campaign.AppToDbDirectory()}");

            //gen report
            sb = new StringBuilder("username,preference,iteration,count");
            sb.Append(Environment.NewLine);
            var s = $@"select a.username, c.cats as preference, r.iteration, count(*) as count
                        from ml_learned_recommendations as r, agents as a, ml_categories as c, ml_sites as s
                        where r.itemid = s.id and s.domain = c.url and c.cats not like '%|%'
                            and a.cloudid = r.cloudid and r.campaign = '{config.Campaign}'
                        group by r.iteration, a.username, c.cats
                        order by iteration, username, cats;";
            i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["username"]).Append(',').Append(r["preference"]).Append(',').Append(r["iteration"]).Append(',').Append(r["count"])
                        .Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.ReportFile.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.ReportFile.AppToDbDirectory()} from ml_learned_recommendations");
                
            //gen final results
            sb = new StringBuilder("id,recs,a,s");
            sb.Append(Environment.NewLine);
            s = $@"select distinct a.id, s.domain from ml_learned_recommendations as recs, agents as a, ml_sites as s where
                        recs.cloudid = a.cloudid and s.id = recs.itemid and recs.campaign = '{config.Campaign}'
                        order by a.id, s.domain;";
            i = 0;
            using (var r = connection.ExecuteReader(s))
            {
                while (r.Read())
                {
                    sb.Append(r["id"]).Append(',').Append(r["recs"]).Append(',').Append(r["a"]).Append(',').Append(r["s"]).Append(Environment.NewLine);
                    i++;
                }
            }
            File.WriteAllText(config.ResultFile.AppToDbDirectory(), sb.ToString());
            log.Trace($"Wrote {i} records to {config.ResultFile.AppToDbDirectory()} from ml_learned_recommendations");
        }
    }
}