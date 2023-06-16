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
using System.Globalization;
using static Ghosts.Spectre.Infrastructure.ML.Models;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Ghosts.Spectre.Infrastructure.Extensions;
using Ghosts.Spectre.Infrastructure.Services;
using NLog;
using static Ghosts.Spectre.Infrastructure.ML.MLModels;

namespace Ghosts.Spectre.Infrastructure.ML
{
    public static class Evaluator
    {
        private static Configuration Config;
        private static List<Agent> Agents;
        private static List<Site> Sites;
        private static readonly List<string> results = new();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static BrowseRecommendationsResults Run(Configuration config = null)
        {
            Config = config ?? new Configuration();
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            results.Add($"Building test {Config.TestNumber} directories...");
            if (!Directory.Exists(Configuration.BaseDirectory))
                Directory.CreateDirectory(Configuration.BaseDirectory);
            if (!Directory.Exists($"{Configuration.BaseDirectory}/{Config.TestNumber}"))
                Directory.CreateDirectory($"{Configuration.BaseDirectory}/{Config.TestNumber}");
            if (!Directory.Exists($"{Configuration.BaseDirectory}/dependencies"))
                Directory.CreateDirectory($"{Configuration.BaseDirectory}/dependencies");

            var agents = 0;
            var sites = 0;
            var browse = 0;
            if (!File.Exists(Config.AgentsFile))
            {
                results.Add("Generating agents file...");
                agents = Generators.GenerateAgentsFile(Config);
            }
            if (!File.Exists(Config.SitesFile))
            {
                results.Add("Generating sites file...");
                sites = Generators.GenerateSitesFile(Config);
            }

            if (!File.Exists(Config.InputFilePref) || !File.Exists(Config.InputFileRand))
            {
                results.Add("Generating browse history files...");
                browse = Generators.GenerateNewBrowseFiles(Config);
            }

            // not enough data generated
            if (sites == 0 && browse == 0)
            {
                foreach (var result in results)
                {
                    log.Trace(result);
                }
                log.Trace($"Sorry, not enough data generated. Browse history was {browse} and sites was {sites} over {agents} agents. Exiting...");
                return new BrowseRecommendationsResults();
            }

            var typesToProcess = new[] { "pref", "rand" };
            foreach (var typeToProcess in typesToProcess)
            {
                Config.CurrentType = typeToProcess;
                results.Add($"Initializing {Config.CurrentType}...");

                results.Add("Extracting test file...");
                if (!File.Exists(Config.TestFile))
                {
                    //build test file from input
                    var lines = File.ReadAllLines(Config.InputFile);

                    var numberForTest = (lines.Length * Config.PercentOfDataIsTest);
                    var linesToRemove = new List<int>();
                    using (StreamWriter w = File.AppendText(Config.TestFile))
                    {
                        w.WriteLine("user_id,item_id,rating,timestamp,iteration".ToLower());

                        int recordsCopied = 0;
                        while (recordsCopied < numberForTest)
                        {
                            var randomLineNumber = 0;
                            if (lines.Length > 1)
                            {
                                var r = new Random();
                                randomLineNumber = r.Next(1, lines.Length - 1);
                                while (linesToRemove.Contains(randomLineNumber))
                                {
                                    randomLineNumber = r.Next(1, lines.Length - 1);
                                }
                            }
                            var line = lines[randomLineNumber];
                            w.WriteLine(line);
                            linesToRemove.Add(randomLineNumber);
                            recordsCopied++;
                        }
                    }

                    //remove test data from input file
                    if (File.Exists(Config.InputFile + ".backup"))
                        File.Delete(Config.InputFile + ".backup");
                    File.Move(Config.InputFile, Config.InputFile + ".backup");
                    using (StreamWriter w = File.AppendText(Config.InputFile))
                    {
                        w.WriteLine("user_id,item_id,rating,timestamp,iteration".ToLower());

                        int i = -1;
                        foreach (var line in lines)
                        {
                            i++;
                            if (i == 0 || linesToRemove.Contains(i))
                            {

                                continue;
                            }
                            w.WriteLine(line);
                        }
                    }
                }

                MLContext mlContext = new MLContext();
                (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);

                Agents = new List<Agent>();

                using (var fileStream = File.OpenRead(Config.AgentsFile))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                    {
                        var i = -1;
                        while (streamReader.ReadLine() is { } line)
                        {
                            i++;
                            if (i == 0) continue;
                            var o = line.Split(Convert.ToChar(","));
                            Agents.Add(new Agent(Convert.ToInt32(o[0]), o[1], Convert.ToInt32(Convert.ToDouble(o[2]))));
                        }
                    }
                }

                Sites = new List<Site>();
                using (var fileStream = File.OpenRead(Config.SitesFile))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                    {
                        while (streamReader.ReadLine() is { } line)
                        {
                            var o = line.Split(Convert.ToChar(","));
                            try
                            {
                                Sites.Add(new Site(Convert.ToInt32(o[0]), o[1]));
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }

                // did files generate required data?
                
                
                results.Add($"Initializing model and associated requirements...");
                if (!File.Exists(Config.ModelFile))
                {
                    ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);
                    EvaluateModel(mlContext, testDataView, model);
                    UseModelForSinglePrediction(mlContext, model);
                    SaveModel(mlContext, trainingDataView.Schema, model);
                }


                /*
                results.Add("=============== Running Experiment ===============");
                var experimentSettings = new RecommendationExperimentSettings();
                experimentSettings.MaxExperimentTimeInSeconds = 3600;
                experimentSettings.OptimizingMetric = RegressionMetric.MeanSquaredError;
                var experiment = mlContext.Auto().CreateRecommendationExperiment(experimentSettings);
                ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                    .CreateRecommendationExperiment(new RecommendationExperimentSettings() { MaxExperimentTimeInSeconds = 3600 })
                    .Execute(trainingDataView, testDataView,
                        new ColumnInformation()
                        {
                            LabelColumnName = "Label",
                            UserIdColumnName = "userId",
                            ItemIdColumnName = "itemId"
                        });
                // STEP 3: Print metric from best model
                RunDetail<RegressionMetrics> bestRun = experimentResult.BestRun;
                results.Add($"Total models produced: {experimentResult.RunDetails.Count()}");
                results.Add($"Best model's trainer: {bestRun.TrainerName}");
                results.Add($"Metrics of best model from validation data --");
                PrintMetrics(bestRun.ValidationMetrics);
                Environment.Exit(1);
                */


                //now that we have a model, we'll loop through that model x times - same model, growing dataset over iteration
                for (var i = 1; i < Config.Iterations; i++)
                {
                    Config.CurrentIteration = i;
                    //Define DataViewSchema for data preparation pipeline and trained model
                    // Load trained model
                    var trainedModel = mlContext.Model.Load(Config.ModelFile, out _);

                    // Load data preparation pipeline and trained model
                    UseModelForSinglePrediction(mlContext, trainedModel);
                }

                results.Add("Generating final reports...");
                Generators.GenerateReportFile(Config);

                results.Add($"{Config.CurrentType} completed in {stopwatch.ElapsedMilliseconds} ms");
            }

            stopwatch.Stop();
            results.Add($"Test completed in {stopwatch.ElapsedMilliseconds} ms");

            //load result file
            var recommendations = RecommendationsService.Load(config.ResultFileOut);
            var browseRecommendationsResults = new BrowseRecommendationsResults {JobOutput = results, Recommendations = recommendations};

            return browseRecommendationsResults;
        }

        private static (IDataView training, IDataView test) LoadData(MLContext mlContext)
        {
            if (!File.Exists(Config.OutputFile))
            {
                File.Copy(Config.InputFile, Config.OutputFile, true);
            }

            if (!File.Exists(Config.StatsFile))
            {
                using var w = File.AppendText(Config.StatsFile);
                w.WriteLine("iteration,RootMeanSquaredError,RSquared,LossFunction,MeanAbsoluteError,MeanSquaredError".ToLower());
            }

            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<BrowseHistory>(Config.InputFile, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<BrowseHistory>(Config.TestFile, hasHeader: true, separatorChar: ',');

            return (trainingDataView, testDataView);
        }

        private static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion
                .MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "userId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "itemIdEncoded", inputColumnName: "itemId"));
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "itemIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 1000,
                ApproximationRank = 100,
                // options.ApproximationRank = 2;
                // options.C = 0;
                // options.Lambda = 0.025;
                // options.Alpha = 0.01;
                //options.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossRegression;
                Quiet = true
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
            results.Add("Training model...");
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
        }

        private static void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
        {
            using var w = File.AppendText(Config.StatsFile);
            results.Add("Evaluating model...");
            var prediction = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
            results.Add("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString(CultureInfo.InvariantCulture));
            results.Add("RSquared: " + metrics.RSquared.ToString(CultureInfo.InvariantCulture));
            w.WriteLine($"{Config.CurrentIteration},{metrics.RootMeanSquaredError},{metrics.RSquared},{metrics.LossFunction},{metrics.MeanAbsoluteError},{metrics.MeanSquaredError}");
        }

        private static void UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
        {
            OsExtensions.WriteOver($"Processing predictions, pass {Config.CurrentIteration}");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BrowseHistory, BrowsePrediction>(model);

            foreach (var agent in Agents)
            {
                var recs = new List<BrowsePrediction>();
                for (var i = 1; i < 500000; i++)
                {
                    var testInput = new BrowseHistory { userId = agent.Id, itemId = i };

                    var itemPrediction = predictionEngine.Predict(testInput);

                    itemPrediction.Iteration = Config.CurrentIteration;
                    itemPrediction.UserId = testInput.userId;
                    itemPrediction.ItemId = testInput.itemId;

                    if (Math.Round(itemPrediction.Score, 1) > 3.5)
                    {
                        var site = Sites.FirstOrDefault(o => Math.Abs(o.Id - itemPrediction.ItemId) < .01);

                        if (site == null)
                        {
                            continue;
                        }

                        if (agent.Preference == site.Category)
                        {
                            // add matching sites with positive correlation
                            itemPrediction.Score = 5;
                            recs.Add(itemPrediction);
                            //results.Add($"Item {testInput.itemId} is recommended for user {testInput.userId} at {Math.Round(itemPrediction.Score, 1)}");
                        }
                        else
                        {
                            // add but rate as poor match
                            itemPrediction.Score = 1;
                            recs.Add(itemPrediction);
                        }
                    }
                }

                using var w = File.AppendText(Config.OutputFile);
                //var rnd = new Random();
                //var choices = recs.OrderBy(x => rnd.Next()).Take(25);
                var choices = recs;

                foreach (var rec in choices)
                {
                    var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    var secondsSinceEpoch = (int)t.TotalSeconds;

                    w.WriteLine($"{rec.UserId},{rec.ItemId},{Math.Round(rec.Score, 1)},{secondsSinceEpoch},{rec.Iteration}"); //user_id,item_id,timestamp
                }
            }
        }

        private static void SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            results.Add("Saving the model to a file...");
            mlContext.Model.Save(model, trainingDataViewSchema, Config.ModelFile);
        }

        public static void RunExperiment()
        {
            /*
                results.Add("=============== Running Experiment ===============");
                var experimentSettings = new RecommendationExperimentSettings();
                experimentSettings.MaxExperimentTimeInSeconds = 3600;
                experimentSettings.OptimizingMetric = RegressionMetric.MeanSquaredError;
                var experiment = mlContext.Auto().CreateRecommendationExperiment(experimentSettings);
                ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                    .CreateRecommendationExperiment(new RecommendationExperimentSettings() { MaxExperimentTimeInSeconds = 3600 })
                    .Execute(trainingDataView, testDataView,
                        new ColumnInformation()
                        {
                            LabelColumnName = "Label",
                            UserIdColumnName = "userId",
                            ItemIdColumnName = "itemId"
                        });
                // STEP 3: Print metric from best model
                RunDetail<RegressionMetrics> bestRun = experimentResult.BestRun;
                results.Add($"Total models produced: {experimentResult.RunDetails.Count()}");
                results.Add($"Best model's trainer: {bestRun.TrainerName}");
                results.Add($"Metrics of best model from validation data --");
                PrintMetrics(bestRun.ValidationMetrics);
                Environment.Exit(1);
                */
        }
    }
}