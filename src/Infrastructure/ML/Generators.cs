/*
GHOSTS SPECTRE
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0370
*/

using Dapper;
using Ghosts.Spectre.Infrastructure.Extensions;
using Npgsql;

namespace Ghosts.Spectre.Infrastructure.ML
{
    internal static class Generators
    {
        internal static void GenerateNewBrowseFiles(Configuration config)
        {
            using (var connection = new NpgsqlConnection(Program.Configuration.ConnectionString))
            {
                connection.Open();

                connection.Execute("truncate table ml_agent_browse_history;");
                connection.Execute("call create_preferenced();");
                connection.Execute("truncate table ml_agent_browse_history_random;");
                connection.Execute("call create_random();");

                var s = $@"copy (
                        select user_id, item_id,
                            CASE
                                WHEN vw_agentprefs.preference = category THEN 5
                                ELSE 1
                                END
                            AS rating,
                            timestamp, 0 as iteration
                        from vw_browsehistory as b
                            right join vw_agentprefs on vw_agentprefs.agentid = b.agentid
                        order by timestamp
                ) To '{config.InputFilePref.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);

                s = $@"copy (
                        select user_id, item_id,
                            CASE
                                WHEN vw_agentprefs.preference = category THEN 5
                                ELSE 1
                                END
                            AS rating,
                            timestamp, 0 as iteration
                        from vw_browsehistory_random as b
                            right join vw_agentprefs on vw_agentprefs.agentid = b.agentid
                        order by timestamp
                ) To '{config.InputFileRand.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);
            }
        }

        internal static void GenerateSitesFile(Configuration config)
        {
            using (var connection = new NpgsqlConnection(Program.Configuration.ConnectionString))
            {
                connection.Open();

                var s = $@"copy (
                        select s.id as site_id, c.cats
                        from ml_sites as s, ml_categories as c
                        where s.domain = c.url and s.id < 500000 and c.cats not like '%|%'
                    ) To '{config.SitesFile.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);
            }
        }

        internal static void GenerateAgentsFile(Configuration config)
        {
            using (var connection = new NpgsqlConnection(Program.Configuration.ConnectionString))
            {
                connection.Open();

                var s = $@"copy (
                    select a2.cloudid, preference, score
                        from vw_agentprefs as a, agents as a2
                        where a.agentid = a2.id
                        order by cloudid
                    ) To '{config.AgentsFile.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);
            }
        }

        internal static void GenerateReportFile(Configuration config)
        {
            using (var connection = new NpgsqlConnection(Program.Configuration.ConnectionString))
            {
                connection.Open();

                connection.Execute("truncate table ml_learned_import_extended;");

                //load ML results file
                var s = $@"COPY ml_learned_import_extended FROM '{config.OutputFile.AppToDbDirectory()}' delimiter ',' CSV HEADER;
                    insert into ml_learned_recommendations
                        (campaign, cloudid, itemid, created, iteration)
                    select '{config.Campaign.AppToDbDirectory()}', cloudid, itemid, now(), iteration
                    from ml_learned_import_extended;";
                connection.Execute(s);

                //gen report
                s = $@"copy (
                    select a.username, c.cats as preference, r.iteration, count(*) as count
                        from ml_learned_recommendations as r, agents as a, ml_categories as c, ml_sites as s
                        where r.itemid = s.id and s.domain = c.url and c.cats not like '%|%'
                            and a.cloudid = r.cloudid and r.campaign = '{config.Campaign}'
                        group by r.iteration, a.username, c.cats
                        order by iteration, username, cats
                ) To '{config.ReportFile.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);
                
                //gen final results
                s = $@"copy (
                    select distinct a.id, s.domain from ml_learned_recommendations as recs, agents as a, ml_sites as s where
                        recs.cloudid = a.cloudid and s.id = recs.itemid and recs.campaign = '{config.Campaign}'
                        order by a.id, s.domain
                ) To '{config.ResultFile.AppToDbDirectory()}' WITH (FORMAT CSV, HEADER);";
                connection.Execute(s);
            }
        }
    }
}