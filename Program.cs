using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace CreditCardDefault
{
    
    /*
    get test data location
    get list of output in a seperate variable
    predict with test data without one column
    get predicted output in a new list
    get percentage of right output
    */
    class Program
    {
       static DataTable testdata;
        static DataTable traindata;
        static void Main(string[] args)
        {

            Console.WriteLine("Decison tree classifier");


            var dataFrame = new DataTable();


            // data will be imported from csv file
            Console.WriteLine("Using csv file provided");
            Console.ResetColor();
            var input = "test.csv";// Console.ReadLine().TrimStart().TrimEnd();

            



            dataFrame = ImportFromCsvFile(input);
            //remove id column
            dataFrame.Columns.Remove("ID");

            //split datatable into two
          (traindata , testdata) =  SplitDatable(dataFrame, 0.8);

          //  ValidateAccuracy(testdata);

            if (traindata == null)
            {
                Console.WriteLine("An error occured while importing the data from the .csv file.");

            }
            else
            {
                CreateTreeAndHandleUserOperation(traindata);


            }

            Console.ReadLine();
        }

        private static void CreateTreeAndHandleUserOperation(DataTable data)
        {
            var decisionTree = new DecisionTree();
            decisionTree.Root = DecisionTree.Learn(data, "");

            var returnToMainMenu = false;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDecision tree created");
            Console.ResetColor();

            ValidateAccuracy(testdata, decisionTree.Root);

            var valuesForQuery = new Dictionary<string, string>();

            // loop for data input for the query and some special commands
            for (var i = 0; i < data.Columns.Count - 1; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nEnter your value for {data.Columns[i]} or help for a list of the additional instructions");
                Console.ResetColor();
                var input = Console.ReadLine().TrimStart().TrimEnd();

                if (input.ToUpper().Equals("PRINT"))
                {
                    Console.WriteLine();
                    DecisionTree.Print(decisionTree.Root, decisionTree.Root.Name.ToUpper());
                    DecisionTree.PrintLegend("Due to the limitation of the console the tree is displayed as a list of every possible route. The colors indicate the following values:");

                    i--;
                }
                else if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("The attribute can't be empty or a white space");
                    i--;
                }
                else
                {
                    valuesForQuery.Add(data.Columns[i].ToString(), input);
                }
            }

            // if input was not to return to the main menu, the query will be processed
            if (!returnToMainMenu)
            {

                var result = DecisionTree.CalculateResult(decisionTree.Root, valuesForQuery, "");

                Console.WriteLine();

                if (result.Contains("Attribute not found"))
                {
                    Console.WriteLine("Can't caluclate outcome. Na valid route through the tree was found");
                }
                else
                {
                    DecisionTree.Print(null, result);
                    DecisionTree.PrintLegend("The colors indicate the following values:");
                }
            }
        }


        public static DataTable ImportFromCsvFile(string filePath)
        {
            var rows = 0;
            var data = new DataTable();

            try
            {
                using (var reader = new StreamReader(File.OpenRead(filePath)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Substring(0, line.Length).Split(',');

                        foreach (var item in values)
                        {
                            if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item))
                            {
                                throw new Exception("Value can't be empty");
                            }

                            if (rows == 0)
                            {
                                data.Columns.Add(item);

                            }
                        }

                        if (rows > 0)
                        {
                            data.Rows.Add(values);
                        }

                        rows++;

                        if (values.Length != data.Columns.Count)
                        {
                            throw new Exception("Row is shorter or longer than title row");
                        }
                    }
                }

                var differentValuesOfLastColumn = attrib.GetDifferentAttributeNamesOfColumn(data, data.Columns.Count - 1);

                if (differentValuesOfLastColumn.Count > 2)
                {
                    throw new Exception("The last column is the result column and can contain only 2 different values");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                data = null;
            }

            // if no rows are entered or data == null, return null
            return data?.Rows.Count > 0 ? data : null;
        }

        public static (DataTable,DataTable) SplitDatable(DataTable data, double percentTest = 0.9)
        {
            var totalrows = data.Rows.Count;
           
            int cutoffpoint = (int)(totalrows * percentTest);

            var traindata = data.AsEnumerable().Take(cutoffpoint).CopyToDataTable();
            var testdata = data.AsEnumerable().Skip(cutoffpoint).Take(totalrows - cutoffpoint).CopyToDataTable();


            return (traindata, testdata) ;
        }

        public static void ValidateAccuracy(DataTable testdata, TreeNode tn)
        {
            double correct = 0;
            double incorrect = 0;
            var orgOutput = testdata.AsEnumerable().Select(x => x.Field<string>("default payment next month")).ToList();

            testdata.Columns.Remove("default payment next month");
            List<string> predictedOutput = new List<string>();

           
            foreach (var item in testdata.AsEnumerable())
            {
                var dict = item.Table.Columns.Cast<DataColumn>().ToDictionary(x => x.ColumnName, x => item[x].ToString());



                predictedOutput.Add(DecisionTree.CalculateResult(tn, dict, ""));
            }


            foreach (var x in predictedOutput)
            {
                if (!x.Contains("Attribute not found"))
                {
                    int index = predictedOutput.IndexOf(x);

                    var value = x.Last().ToString();

                    if (orgOutput.ElementAt(index).Equals(value))
                    {
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }
            }

            var total = incorrect + correct;

            double percentcorrect = correct / total * 100;

            Console.WriteLine($"Validation finished. Model accuracy {percentcorrect.ToString("00.00")} %");
        }
    }
}
