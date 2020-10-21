namespace TestOfflineSentimentAnalyzers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    class Program
    {
        private const string OutputDirectory = "YOUR OUTPUT DIRECTORY";
        // You can download original SST-2 dataset here: https://www.kaggle.com/jgggjkmf/binary-sst2-dataset
        // Stanford Sentiment Treebank (Socher et al. 2013. Recursive deep models for semantic compositionality over a sentiment treebank. In proc. EMNLP)

        // The dataset should be in tsv format: text\tlabel for this project.
        private const string TestFilePath = @"../../../../TestData/sst2.tsv";

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // This code will successfully run in Evaluation mode.
            var isLicensedGroupDocs = false;
            /**
             *  Applying product license
             *  Please uncomment the statement if you do have license.
             */
            // const string licensePath = "../../../../../GroupDocs.Classification.NET.lic";
            // new GroupDocs.Classification.License().SetLicense(licensePath);
            // isLicensedGroupDocs = true;

            SentimentAnalysisVaderSharp();
            SentimentAnalysisGroupDocs(isLicensedGroupDocs);
            SentimentAnalysisSentimentAnalyzer();
        }

        /// <summary>
        /// Sentiment Analysis test for VaderSharp.
        /// </summary>
        static void SentimentAnalysisVaderSharp()
        {
            var outputFilePath = "VaderSharp-results.csv";
            var lines = File.ReadLines(TestFilePath);
            var sentimentAnalyzer = new VaderSharp.SentimentIntensityAnalyzer();
            var correctLines = 0f;
            var cntr = 0;
            foreach (var line in lines)
            {
                if (cntr > 0)
                {
                    var strings = line.Split('\t');
                    var text = strings[0];
                    var label = strings[1] == "0" ? "Negative" : "Positive";
                    var results = sentimentAnalyzer.PolarityScores(text);
                    var correct = (label == (results.Compound > 0 ? "Positive" : "Negative")) ? 1 : 0;
                    File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"{cntr};{label};{correct};\n");
                    correctLines += correct;
                }

                cntr++;
            }

            var accuracy = correctLines / (lines.Count() - 1);
            File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"final accuracy/correct lines;{accuracy};{correctLines}");
            System.Console.WriteLine($"VaderSharp accuracy: {accuracy}, {correctLines}");
        }

        /// <summary>
        /// Sentiment Analysis test for GroupDocs.Classification.
        /// </summary>
        /// <param name="isLicensed">Has the license been applied?</param>
        static void SentimentAnalysisGroupDocs(bool isLicensed)
        {
            var outputFilePath = "GroupDocs-results.csv";
            var lines = File.ReadLines(TestFilePath);
            var sentimentAnalyzer = new GroupDocs.Classification.SentimentClassifier();
            var correctLines = 0f;
            var cntr = 0;
            foreach (var line in lines)
            {
                if (cntr > 0)
                {
                    var strings = line.Split('\t');
                    var text = strings[0];
                    var label = strings[1] == "0" ? "Negative" : "Positive";
                    var results = 0.5f;
                    if (isLicensed)
                    {
                        results = sentimentAnalyzer.PositiveProbability(text);
                    }
                    else
                    {
                        // Evaluation mode allows only 100 characters for the Snetiment Analysis.
                        results = SplitString(text, 100).Select(x => sentimentAnalyzer.PositiveProbability(x)).Average();
                    }

                    var correct = (label == (results < 0.5 ? "Negative" : "Positive")) ? 1 : 0;
                    File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"{cntr};{label};{correct}\n");
                    correctLines += correct;
                }

                cntr++;
            }

            var accuracy = correctLines / (lines.Count() - 1);
            File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"final accuracy/correct lines;{accuracy};{correctLines}");
            System.Console.WriteLine($"GroupDocs accuracy: {accuracy}, {correctLines}");
        }

        /// <summary>
        /// Sentiment Analysis test for SentimentAnalyzer.
        /// </summary>
        static void SentimentAnalysisSentimentAnalyzer()
        {
            var bestThreshold = 0;
            var outputFilePath = "SentimentAnalyzer-results.csv";
            var lines = File.ReadLines(TestFilePath);

            var correctLines = 0f;
            var cntr = 0;
            foreach (var line in lines)
            {
                if (cntr > 0)
                {
                    var strings = line.Split('\t');
                    var text = strings[0];
                    var label = strings[1] == "0" ? "Negative" : "Positive";
                    var results = SentimentAnalyzer.Sentiments.Predict(text);
                    var correct = (label == (results.Score < bestThreshold ? "Negative" : "Positive")) ? 1 : 0;
                    File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"{cntr};{label};{correct}\n");
                    correctLines += correct;
                }

                cntr++;
            }

            var accuracy = correctLines / (lines.Count() - 1);
            File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"final accuracy/correct lines;{accuracy};{correctLines}");
            System.Console.WriteLine($"SentimentAnalyzer accuracy: {accuracy}, {correctLines}");
        }

        private static IEnumerable<string> SplitString(string str, int chunkSize)
        {
            return Enumerable.Range(0, 1 + str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, ((i + 1) * chunkSize > str.Length) ? (str.Length - i * chunkSize) : chunkSize));
        }
    }
}
