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
using System.ComponentModel;
using Ghosts.Spectre.Infrastructure.Services;

namespace Ghosts.Spectre.Infrastructure.ML
{
    /// <summary>
    /// Models for all ML jobs within the SPECTRE system
    /// </summary>
    public class MLModels
    {
        /// <summary>
        /// Configuration for executing this job
        /// </summary>
        public class ConfigureBrowseRecommendationsJob
        {
            /// <summary>
            /// Number of learning iterations to execute - higher numbers mean more time and bigger data files
            /// </summary>
            /// <example>25</example> 
            [DefaultValue(25)]
            public int Iterations { get; set; }

            /// <summary>
            /// Percent of data to extract (randomly) as test data (vs train data)
            /// </summary>
            /// <example>0.20</example> 
            [DefaultValue(0.20)]
            public double PercentOfDataIsTest { get; set; }
        }

        public class BrowseRecommendationsResults
        {
            /// <summary>
            /// The results of the execution job
            /// </summary>
            public IEnumerable<string> JobOutput { get; set; }
            public IEnumerable<RecommendationsService.RecommendationValues> Recommendations { get; set; }
        }
    }
}