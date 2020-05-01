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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ghosts.Spectre.Infrastructure.Data;
using Ghosts.Spectre.Infrastructure.Extensions;
using Ghosts.Spectre.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Agent = Ghosts.Spectre.Infrastructure.Models.Agent;
using Persona = Ghosts.Spectre.Infrastructure.Models.Persona;

namespace Ghosts.Spectre.Infrastructure.Services
{
    public interface IAgentService
    {
        Task<IEnumerable<Agent>> GetAll(CancellationToken ct);
        Task<Agent> GetById(string id, CancellationToken ct);
        Task<bool> ExistsByUsername(string username, CancellationToken ct);
        Task<Agent> Create(AgentUpdate agent, int? personaId, CancellationToken ct);
        Task<Agent> Update(string id, AgentUpdate agent, CancellationToken ct);
        Task<bool> Delete(string id, CancellationToken ct);
        Task Sync(CancellationToken ct);
        Task<Agent> UpdateTag(string id, IList<AgentTagUpdate> agentTags, CancellationToken ct);
    }

    public class AgentService : IAgentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AgentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Agent>> GetAll(CancellationToken ct)
        {
            var s = await _context.Agents.ProjectTo<Agent>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
            return s;
        }

        public async Task<Agent> GetById(string id, CancellationToken ct)
        {
            var s = await _context.Agents.ProjectTo<Agent>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.Id.ToString() == id, ct);
            return s;
        }

        public async Task<bool> ExistsByUsername(string username, CancellationToken ct)
        {
            var s = await _context.Agents.AnyAsync(o => o.Username == username, ct);
            return s;
        }

        public async Task<Agent> Create(AgentUpdate agent, int? personaId, CancellationToken ct)
        {
            var createdAgent = _mapper.Map<Infrastructure.Repositories.Agent>(agent);
            await _context.Agents.AddAsync(createdAgent, ct);
            await _context.SaveChangesAsync(ct);

            var personaService = new PersonaService(this._context, this._mapper);

            Persona persona = null;
            if (personaId.HasValue)
            {
                persona = await personaService.GetById(personaId.Value, ct);
            }
            else
            {
                var personas = await personaService.GetAll(ct);
                if (personas.Any())
                {
                    persona = personas.RandomElement();
                }
            }

            if (persona != null)
            {
                var tags = new List<AgentTagUpdate>();
                var r = new Random();
                foreach (var personaTag in persona.PersonaTags)
                {
                    tags.Add(new AgentTagUpdate { TagId = personaTag.TagId, Score = r.Next(personaTag.Low, personaTag.High) });
                }

                await this.UpdateTag(createdAgent.Id.ToString(), tags, ct);
            }

            return _mapper.Map<Agent>(createdAgent);
        }

        public async Task Sync(CancellationToken ct)
        {
            var personaService = new PersonaService(this._context, this._mapper);
            var personas = await personaService.GetAll(ct);

            var agents = await _context.Agents.Include(o => o.Tags).Where(o => !o.Tags.Any()).ToListAsync(ct);
            foreach (var agent in agents)
            {
                var persona = personas.RandomElement();

                if (persona != null)
                {
                    var tags = new List<AgentTagUpdate>();
                    var r = new Random();
                    foreach (var personaTag in persona.PersonaTags)
                    {
                        tags.Add(new AgentTagUpdate { TagId = personaTag.TagId, Score = r.Next(personaTag.Low, personaTag.High) });
                    }

                    await this.UpdateTag(agent.Id.ToString(), tags, ct);
                }
            }
        }

        public async Task<Agent> Update(string id, AgentUpdate agent, CancellationToken ct)
        {
            var repo = _mapper.Map<Infrastructure.Repositories.Agent>(agent);
            _context.Agents.Update(repo);
            await _context.SaveChangesAsync(ct);
            return _mapper.Map<Agent>(repo);
        }

        public async Task<bool> Delete(string id, CancellationToken ct)
        {
            var agent = await _context.Agents
                .FirstOrDefaultAsync(o => o.Id.ToString() == id, ct);
            if (agent != null)
            {
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync(ct);
                return true;
            }

            return false;
        }

        public async Task<Agent> UpdateTag(string id, IList<AgentTagUpdate> agentTags, CancellationToken ct)
        {
            var agent = await _context.Agents.ProjectTo<Agent>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.Id.ToString() == id, ct);
            foreach (var tag in agentTags)
            {
                if (agent.Tags.Any(o => o.Tag.Id == tag.TagId))
                {
                    //update
                    var existingTag = await _context.AgentTags
                        .Include(o => o.Tag)
                        .FirstOrDefaultAsync(o => o.Tag.Id == tag.TagId, ct);

                    //save off to history
                    _context.AgentTagHistories.Add(existingTag.ToAgentTagHistory());

                    existingTag.Score = tag.Score;
                    _context.AgentTags.Update(existingTag);
                }
                else
                {
                    //add
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(o => o.Id == tag.TagId);
                    _context.AgentTags.Add(new Repositories.AgentTag { AgentId = agent.Id, Score = tag.Score, Tag = existingTag });
                }
            }

            await _context.SaveChangesAsync(ct);

            var s = await _context.Agents.ProjectTo<Agent>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.Id.ToString() == id, ct);
            return s;
        }
    }
}