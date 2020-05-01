/*
GHOSTS SPECTRE
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0370
*/

using Microsoft.ML.Data;

namespace Ghosts.Spectre.Infrastructure.ML
{
    public class Models
    {
        public class BrowseHistory
        {
            [LoadColumn(0)]
            public float userId;

            [LoadColumn(1)]
            public float itemId;

            [LoadColumn(2)]
            public float Label;
        }

        public class BrowsePrediction
        {
            public float Label;
            public float UserId;
            public float ItemId;
            public float Score;
            public int Iteration;
        }

        public class Agent
        {
            public int Id;
            public string Preference;
            public int Score;

            public Agent() { }
            public Agent(int id, string preference, int score)
            {
                this.Id = id;
                this.Preference = preference;
                this.Score = score;
            }
        }

        public class Site
        {
            public int Id;
            public string Category;

            public Site() { }

            public Site(int id, string category)
            {
                this.Id = id;
                this.Category = category;
            }
        }
    }
}