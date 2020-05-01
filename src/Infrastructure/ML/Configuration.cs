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

namespace Ghosts.Spectre.Infrastructure.ML
{
    public class Configuration
    {
        public string CurrentType { internal set; get; }
        public string TestNumber { private set; get; }
        public string Campaign
        {
            get { return $"{TestNumber}-{CurrentType}"; }
        }

        /// Defaults to 25
        public int Iterations { get; set; }

        ///defaults to 20% (0.2)
        public double PercentOfDataIsTest { get; set; }

        //get { return $"{Environment.CurrentDirectory}/_data"; }
        public static string BaseDirectory => Program.Configuration.DataDirectory;

        public string InputFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-in.csv"; }
        }

        public string InputFileRand
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/rand-in.csv"; }
        }

        public string InputFilePref
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/pref-in.csv"; }
        }

        public string TestFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-test.csv"; }
        }

        public string OutputFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-out.csv"; }
        }

        public string ReportFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-report.csv"; }
        }
        
        public string ResultFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-results.csv"; }
        }
        
        public string ResultFileOut
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/pref-results.csv"; }
        }

        public string StatsFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/{this.CurrentType}-outstats.csv"; }
        }

        public string AgentsFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/agents.csv"; }
        }

        public string ModelFile
        {
            get { return $"{BaseDirectory}/{this.TestNumber}/model.zip"; }
        }

        public string SitesFile
        {
            get { return $"{BaseDirectory}/dependencies/sites.csv"; }
        }

        public int CurrentIteration = 0;

        public Configuration()
        {
            TestNumber = Guid.NewGuid().ToString();
        }
    }
}