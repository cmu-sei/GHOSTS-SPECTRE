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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ghosts.Spectre.Infrastructure.ML
{
    public class Entities
    {
        [Table("ml_temp_agent_browse_history")]
        public class TempAgentBrowseHistory
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [ForeignKey("AgentId")]
            public Guid AgentId { get; set; }
            [ForeignKey("SiteId")]
            public int SiteId { get; set; }
            public DateTime Created { get; set; }
        }
        
        [Table("ml_agent_browse_history")]
        public class AgentBrowseHistory
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [ForeignKey("AgentId")]
            public Guid AgentId { get; set; }
            [ForeignKey("SiteId")]
            public int SiteId { get; set; }
            public DateTime Created { get; set; }
        }
        
        [Table("ml_agent_browse_history_random")]
        public class AgentBrowseHistoryRandom
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [ForeignKey("AgentId")]
            public Guid AgentId { get; set; }
            [ForeignKey("SiteId")]
            public int SiteId { get; set; }
            public DateTime Created { get; set; }
        }
        
        [Table("ml_learned_import")]
        public class LearnedImport
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [ForeignKey("CloudId")]
            public int CloudId { get; set; }
            [ForeignKey("SiteId")]
            public int ItemId { get; set; }
        }
        
        [Table("ml_learned_import_extended")]
        public class LearnedImportExtended
        {
            [Key]
            [ForeignKey("CloudId")]
            public int CloudId { get; set; }
            public int ItemId { get; set; }
            public double Rating { get; set; }
            public string Created { get; set; }
            public int Iteration { get; set; } 
        }
        
        [Table("ml_learned_recommendations")]
        public class LearnedRecommendations
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [ForeignKey("CloudId")]
            public int CloudId { get; set; }
            public string Campaign { get; set; } 
            public int ItemId { get; set; }
            public int Iteration { get; set; }
            public DateTime Created { get; set; }
        }

        [Table("ml_categories")]
        public class Categories
        {
            public string Url { get; set; }
            public string Cats { get; set; }
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
        }

        [Table("ml_sites")]
        public class Sites
        {
            public int GlobalRank { get; set; }
            public string TldRank { get; set; }
            public string Domain { get; set; }
            public string Tld { get; set; }
            public string Refsubnets  { get; set; }
            public string Refips { get; set; }
            public string Idn_domain { get; set; }
            public string Idn_tld { get; set; }
            public string Prevglobalrank { get; set; }
            public string Prevtldrank { get; set; }
            public string Prevrefsubnets { get; set; }
            public string Prevrefips { get; set; }
            
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
        }

        [Table("ml_temp_sites")]
        public class TempSites
        {
            [Key] [ForeignKey("AgentId")] 
            public Guid AgentId { get; set; }
            public string Url { get; set; }
            public DateTime Created { get; set; }
        }
    }
}