/*
GHOSTS SPECTRE
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0370
*/

using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ghosts.Spectre.Infrastructure.Models;
using Ghosts.Spectre.Infrastructure.Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace Ghosts.Spectre.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _service;

        public AgentsController(IAgentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets agents currently within SPECTRE's control
        /// </summary>
        /// <returns>
        /// <see cref="Get"/>
        /// </returns>
        [ProducesResponseType(typeof(IEnumerable<Agent>), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Agent>))]
        [SwaggerOperation("getAgents")]
        [HttpGet]
        public async Task<IEnumerable<Agent>> Get(CancellationToken ct)
        {
            return await _service.GetAll(ct);
        }

        /// <summary>
        /// Gets a specific agent by its Id 
        /// </summary>
        /// <param name="id">The Id of the agent to get</param>
        /// <param name="ct"></param>
        /// <returns>
        /// <see cref="GetById"/>
        /// </returns>
        [ProducesResponseType(typeof(Agent), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Agent))]
        [SwaggerOperation("getAgentById")]
        [HttpGet("{id}")]
        public async Task<Agent> GetById([FromRoute] string id, CancellationToken ct)
        {
            return await _service.GetById(id, ct);
        }

        /// <summary>
        /// Create new agent in SPECTRE - note this agent must already exist in GHOSTS
        /// </summary>
        /// <param name="agent">An agent object</param>
        /// <param name="personaId">The persona to use in generating this agent's preferences</param>
        /// <param name="ct"></param>
        /// <returns>The created agent</returns>
        [ProducesResponseType(typeof(Agent), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Agent))]
        [SwaggerOperation("createAgent")]
        [HttpPost]
        public async Task<Agent> Create([FromBody] AgentUpdate agent, int? personaId, CancellationToken ct)
        {
            return await _service.Create(agent, personaId, ct);
        }

        /// <summary>
        /// Update agent in SPECTRE - note this agent must already exist in GHOSTS
        /// </summary>
        /// <param name="id">The Id of the agent to be updated</param>
        /// <param name="agent">The agent object to be updated</param>
        /// <param name="ct"></param>
        /// <returns>The updated agent</returns>
        [ProducesResponseType(typeof(Agent), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Agent))]
        [SwaggerOperation("createAgent")]
        [HttpPut("{id}")]
        public async Task<Agent> Update([FromRoute] string id, [FromBody] AgentUpdate agent, CancellationToken ct)
        {
            return await _service.Update(id, agent, ct);
        }

        /// <summary>
        /// Delete agent from SPECTRE - note this will not delete the agent in GHOSTS
        /// </summary>
        /// <param name="id">The Id of the agent to be removed</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("deleteAgent")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct)
        {
            if (await _service.Delete(id, ct))
                return NoContent();
            return NotFound("Agent not found");
        }

        /// <summary>
        /// Update the tags associated with a specific agent
        /// </summary>
        /// <param name="id">Agent Id</param>
        /// <param name="agentTags">The tags to be updated</param>
        /// <param name="ct"></param>
        /// <returns>The updated agent</returns>
        /// [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Agent), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Agent))]
        [SwaggerOperation("updateTags")]
        [HttpPost("{id}/tags")]
        public async Task<Agent> Update([FromRoute] string id, [FromBody] IList<AgentTagUpdate> agentTags, CancellationToken ct)
        {
            return await _service.UpdateTag(id, agentTags, ct);
        }
        
        /// <summary>
        /// Takes all SPECTRE users that do not have tags, randomly assigns them a persona, and then the appropriate tags
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerOperation("doPersonas")]
        [HttpPost("personas")]
        public async Task Personas(CancellationToken ct)
        {
            await _service.Sync(ct);
        }
        
        /// <summary>
        /// Pulls all users in SPECTRE from GHOSTS via the latter system's API, randomly assigns them a persona, and then the appropriate tags 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>A count of records synced</returns>
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string))]
        [SwaggerOperation("doSync")]
        [HttpPost("sync")]
        public async Task<string> Sync(CancellationToken ct)
        {
            var client = new RestClient($"{Program.Configuration.GhostsApiUrl}/api/Machines");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("accept", "application/json");
            var response = client.Execute(request);
            var machines = JsonConvert.DeserializeObject<IEnumerable<Machine>>(response.Content);

            var i = 0;
            foreach (var machine in machines)
            {
                if (await _service.ExistsByUsername(machine.CurrentUsername, ct)) continue;
                var agent = new Infrastructure.Models.Agent { MachineId = machine.Id, Username = machine.CurrentUsername };
                await _service.Create(agent, null, ct);
                i++;
            }

            return $"{i} records synced";
        }
    }
}