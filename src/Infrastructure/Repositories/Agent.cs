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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ghosts.Spectre.Infrastructure.Repositories
{
    [Table("agents")]
    public class Agent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CloudId { get; set; }
        
        public Guid MachineId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<AgentTag> Tags { get; set; }
        
        /// <summary>
        /// 0 inactive, 1 active
        /// </summary>
        [DefaultValue(1)]
        public int Status { get; set; }
        public DateTime Created { get; set; }
        
        public Agent()
        {
            this.Tags = new List<AgentTag>();
            this.Created = DateTime.UtcNow;
        }
    }

    [Table("agent_tags")]
    public class AgentTag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid AgentId { get; set; }

        public virtual Tag Tag { get; set; }
        public double Score { get; set; }
        public DateTime Created { get; set; }

        public AgentTag()
        {
            this.Created = DateTime.UtcNow;
        }

        public AgentTagHistory ToAgentTagHistory()
        {
            var tag = new AgentTagHistory();
            tag.Created = this.Created;
            tag.Score = this.Score;
            tag.TagId = this.Tag.Id;
            tag.AgentId = this.AgentId;
            return tag;
        }
    }

    [Table("agent_tag_history")]
    public class AgentTagHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid AgentId { get; set; }

        public int TagId { get; set; }
        public double Score { get; set; }
        public DateTime Created { get; set; }
    }

    [Table("tags")]
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }

        public Tag()
        {
            this.Created = DateTime.UtcNow;
        }
    }
}