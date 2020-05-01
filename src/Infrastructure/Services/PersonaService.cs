/*
GHOSTS SPECTRE
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0370
*/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileHelpers;
using Ghosts.Spectre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghosts.Spectre.Infrastructure.Services
{
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class PersonaSeedValues
    {
        public string Name;
        public string Tag;
        public int Low;
        public int High;
    }

    public interface IPersonaService
    {
        Task<IEnumerable<Models.Persona>> GetAll(CancellationToken ct);
        Task<Models.Persona> GetById(int id, CancellationToken ct);
    }

    public class PersonaService : IPersonaService
    {
        public IList<PersonaSeedValues> Values { get; set; }
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PersonaService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Models.Persona>> GetAll(CancellationToken ct)
        {
            var s = await _context.Personas.ProjectTo<Models.Persona>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
            return s;
        }

        public async Task<Models.Persona> GetById(int id, CancellationToken ct)
        {
            var s = await _context.Personas.ProjectTo<Models.Persona>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
            return s;
        }

        public static IEnumerable<PersonaSeedValues> LoadDefaults()
        {
            var engine = new FileHelperEngine<PersonaSeedValues>();
            return engine.ReadFile(
                $"{ConfigurationService.InstalledPath}{System.IO.Path.DirectorySeparatorChar}config{System.IO.Path.DirectorySeparatorChar}personas.csv");
        }
    }
}