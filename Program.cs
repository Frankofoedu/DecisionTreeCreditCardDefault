using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace CreditCardDefault
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to the decison tree classifier");
            Console.WriteLine("---------------------------------------");


            var dataFrame = new DataTable();


            // data will be imported from csv file
            Console.WriteLine("\nEnter the path to the .csv file which you want to import");
            Console.ResetColor();
            var input = Console.ReadLine().TrimStart().TrimEnd();

            dataFrame = ImportFromCsvFile(input);

            if (dataFrame == null)
            {
                Console.WriteLine("An error occured while importing the data from the .csv file. Press any key to close the program.");

            }
            else
            {
                CreateTreeAndHandleUserOperation(dataFrame);
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

    }
}
