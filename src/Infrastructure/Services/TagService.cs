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
using Ghosts.Spectre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ghosts.Spectre.Infrastructure.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Models.Tag>> GetAll(CancellationToken ct);
    }

    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TagService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Models.Tag>> GetAll(CancellationToken ct)
        {
            var s = await _context.Tags.ProjectTo<Models.Tag>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
            return s;
        }
    }
}