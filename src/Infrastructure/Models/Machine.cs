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

namespace Ghosts.Spectre.Infrastructure.Models
{
    public class Machine
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FQDN { get; set; }
        public string Domain { get; set; }
        public string Host { get; set; }
        public string ResolvedHost { get; set; }
        public string HostIp { get; set; }
        public string IPAddress { get; set; }
        public string CurrentUsername { get; set; }
        public string ClientVersion { get; set; }
        public string Status { get; set; }
        public DateTime CreatedUtc { get; set; }

        public string StatusUp { get; set; }
        public DateTime LastReportedUtc { get; set; }
    }
}