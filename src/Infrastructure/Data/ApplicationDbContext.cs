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
using System.Linq;
using Ghosts.Spectre.Infrastructure.Extensions;
using Ghosts.Spectre.Infrastructure.ML;
using Ghosts.Spectre.Infrastructure.Repositories;
using Ghosts.Spectre.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Ghosts.Spectre.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AgentTag> AgentTags { get; set; }
        public DbSet<AgentTagHistory> AgentTagHistories { get; set; }
        public DbSet<Persona> Personas { get; set; }
        
        public DbSet<ML.Entities.Categories> Categories { get; set; }
        public DbSet<ML.Entities.Sites> Sites { get; set; }
        public DbSet<ML.Entities.LearnedImport> LearnedImports { get; set; }
        public DbSet<ML.Entities.LearnedRecommendations> LearnedRecommendations { get; set; }
        public DbSet<ML.Entities.TempSites> TempSites { get; set; }
        public DbSet<ML.Entities.AgentBrowseHistory> AgentBrowseHistories { get; set; }
        public DbSet<ML.Entities.LearnedImportExtended> LearnedImportExtended { get; set; }
        public DbSet<ML.Entities.AgentBrowseHistoryRandom> AgentBrowseHistoryRandoms { get; set; }
        public DbSet<ML.Entities.TempAgentBrowseHistory> TempAgentBrowseHistories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToCondensedLowerCase());
                foreach (var property in entity.GetProperties())
                    property.SetColumnName(property.Name.ToCondensedLowerCase());
                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName().ToCondensedLowerCase());
                foreach (var key in entity.GetForeignKeys())
                    key.SetConstraintName(key.GetConstraintName().ToCondensedLowerCase());
                foreach (var index in entity.GetIndexes())
                    index.SetName(index.GetName().ToCondensedLowerCase());
            }
        }
        
        public static void Seed(ApplicationDbContext context)
        {
            var updateFile = $"{ConfigurationService.InstalledPath}{Path.DirectorySeparatorChar}config{Path.DirectorySeparatorChar}setup.sql";
            if (File.Exists(updateFile))
            {
                var sql = File.ReadAllText(updateFile);
                context.Database.ExecuteSqlRaw(sql);
                File.Delete(updateFile);
            }
            
            if (!context.Tags.Any())
            {
                //load persona service and create tag for each distinct tag found there
                var s = PersonaService.LoadDefaults();

                foreach (var item in s.GroupBy(elem => elem.Name).Select(group => group.First()))
                {
                    context.Personas.Add(new Persona { Name = item.Name });
                }
                foreach (var item in s.GroupBy(elem => elem.Tag).Select(group => group.First()))
                {
                    context.Tags.Add(new Tag { Name = item.Tag });
                }
                context.SaveChanges();

                foreach (var item in s)
                {
                    var tag = context.Tags.FirstOrDefault(o => o.Name == item.Tag);
                    var persona = context.Personas.FirstOrDefault(o => o.Name != null && o.Name == item.Name);
                    persona?.PersonaTags.Add(new PersonaTag { TagId = tag.Id, High = item.High, Low = item.Low });
                }
                context.SaveChanges();

                var agentId = Guid.NewGuid();
                context.Agents.Add(new Agent { Id = agentId, MachineId = Guid.NewGuid(), Username = "jed.eckert", FirstName = "Jed", LastName = "Eckert" });

                var random = new Random();
                var x = context.Personas.FirstOrDefault(o => o.Name.ToLower() == "sports");
                foreach (var personaTag in x?.PersonaTags)
                {
                    var tag = context.Tags.FirstOrDefault(o => o.Id == personaTag.TagId);
                    context.AgentTags.Add(new AgentTag { AgentId = agentId, Score = random.Next(personaTag.Low, personaTag.High), Tag = tag });
                }

                agentId = Guid.NewGuid();
                context.Agents.Add(new Agent { Id = agentId, MachineId = Guid.NewGuid(), Username = "andy11", FirstName = "Andy", LastName = "Tanner" });
                x = context.Personas.FirstOrDefault(o => o.Name.ToLower() == "news");
                foreach (var personaTag in x?.PersonaTags)
                {
                    var tag = context.Tags.FirstOrDefault(o => o.Id == personaTag.TagId);
                    context.AgentTags.Add(new AgentTag { AgentId = agentId, Score = random.Next(personaTag.Low, personaTag.High), Tag = tag });
                }

                context.SaveChanges();
                
                Loaders.LoadSeedData();
            }
        }
    }
}