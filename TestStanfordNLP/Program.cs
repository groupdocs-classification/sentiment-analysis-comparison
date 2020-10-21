namespace TestStanfordNLP
{
    using System;
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

            SentimentAnalysisStanfordNLP();
        }

        /// <summary>
        /// Sentiment Analysis test for StanfordNLP.
        /// </summary>
        static void SentimentAnalysisStanfordNLP()
        {
            var outputFilePath = "StanfordNLP-results.csv";
            var lines = File.ReadLines(TestFilePath);
            var correctLines = 0f;
            var cntr = 0;
            foreach (var line in lines)
            {
                if (cntr > 0)
                {
                    var strings = line.Split('\t');
                    var text = strings[0];

                    // OOM occurs when text.Length > 1000
                    text = text.Length > 1000 ? text.Substring(0, 1000) : text;
                    var label = strings[1] == "0" ? "Negative" : "Positive";
                    var results = (new edu.stanford.nlp.simple.Sentence(text)).sentiment();
                    var correct = (label == (results.isNegative() ? "Negative" : "Positive")) ? 1 : 0;
                    File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"{cntr};{label};{correct}\n");
                    correctLines += correct;
                }

                cntr++;
            }

            var accuracy = correctLines / (lines.Count() - 1);
            File.AppendAllText(Path.Combine(OutputDirectory, outputFilePath), $"final accuracy/correct lines;{accuracy};{correctLines}");
            Console.WriteLine($"StanfordNLP accuracy: {accuracy}, {correctLines}");
        }
    }
}