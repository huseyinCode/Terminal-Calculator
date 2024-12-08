using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TerminalCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Terminal Calculator with Parser!");
            bool continueCalculating = true;

            while (continueCalculating)
            {
                Console.Write("Enter an expression: ");
                string expression = Console.ReadLine();
                try
                {
                    ValidateExpression(expression);
                    double result = EvaluateExpression(expression);
                    Console.WriteLine($"Result: {result}");
                    Save(expression, result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.Write("Would you like to perform another calculation? (yes/no): ");
                continueCalculating = Console.ReadLine().ToLower() == "yes";
            }

            Console.Write("Would you like to view the calculation history? (yes/no): ");
            bool viewHistory = Console.ReadLine().ToLower() == "yes";
            if (viewHistory)
            {
                ShowHistory();
            }

            Console.WriteLine("Thank you for using Terminal Calculator with Parser. Goodbye!");
        }

        static void ValidateExpression(string expression)
        {
            var tokens = expression.Split(' ');

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "sqrt" && i + 1 < tokens.Length && double.TryParse(tokens[i + 1], out double number) && number < 0)
                {
                    throw new ArgumentException("Cannot take the square root of a negative number");
                }
            }
        }

        static double EvaluateExpression(string expression)
        {
            var outputQueue = new Queue<string>();
            var operatorStack = new Stack<string>();
            var tokens = expression.Split(' ');

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    outputQueue.Enqueue(token);
                }
                else if (IsOperator(token))
                {
                    while (operatorStack.Count > 0 && Precedence(operatorStack.Peek()) >= Precedence(token))
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                }
                else if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Pop();
                }
                else
                {
                    throw new ArgumentException($"Invalid token: {token}");
                }
            }

            while (operatorStack.Count > 0)
            {
                outputQueue.Enqueue(operatorStack.Pop());
            }

            var evaluationStack = new Stack<double>();
            while (outputQueue.Count > 0)
            {
                var token = outputQueue.Dequeue();
                if (double.TryParse(token, out double number))
                {
                    evaluationStack.Push(number);
                }
                else if (IsOperator(token))
                {
                    if (token == "sqrt")
                    {
                        var operand = evaluationStack.Pop();
                        evaluationStack.Push(Math.Sqrt(operand));
                    }
                    else
                    {
                        var right = evaluationStack.Pop();
                        var left = evaluationStack.Pop();
                        evaluationStack.Push(ApplyOperator(token, left, right));
                    }
                }
            }

            double result = evaluationStack.Pop();

            if (double.IsInfinity(result))
            {
                throw new InvalidOperationException("Result is infinity");
            }
            if (double.IsNaN(result))
            {
                throw new InvalidOperationException("Result is not a number (NaN)");
            }

            return result;
        }

        static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "sqrt";
        }

        static int Precedence(string op)
        {
            return op == "+" || op == "-" ? 1 : (op == "*" || op == "/" || op == "sqrt" ? 2 : 3);
        }

        static double ApplyOperator(string op, double left, double right)
        {
            return op switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                _ => throw new ArgumentException($"Invalid operator: {op}"),
            };
        }

        static void Save(string expression, double result)
        {
            string fileName = "historyCs.json";

            // Check if JSON file exists and load previous data if it does
            List<Calculation> history = new List<Calculation>();
            if (File.Exists(fileName))
            {
                string existingData = File.ReadAllText(fileName);
                if (!string.IsNullOrEmpty(existingData))
                {
                    try
                    {
                        var oldData = JsonSerializer.Deserialize<List<Calculation>>(existingData);
                        if (oldData != null)
                        {
                            history = oldData;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error reading JSON data: {ex.Message}");
                        // Optionally, clear the file if the data is corrupted
                        File.WriteAllText(fileName, string.Empty);
                        Console.WriteLine("Cleared existing JSON data.");
                    }
                }
            }

            // Add new result and expression to history
            history.Add(new Calculation { Expression = expression, Result = result.ToString() });

            // Write updated history to JSON file
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(history, options);

            // Manually replace encoded plus sign with actual sign, ensuring only one replacement
            jsonString = jsonString.Replace("\\u002B", "+");

            File.WriteAllText(fileName, jsonString);
            Console.WriteLine("Result saved to history.");
        }

        static void ShowHistory()
        {
            string fileName = "historyCs.json";
            if (File.Exists(fileName))
            {
                string existingData = File.ReadAllText(fileName);
                if (!string.IsNullOrEmpty(existingData))
                {
                    try
                    {
                        var history = JsonSerializer.Deserialize<List<Calculation>>(existingData);
                        if (history != null)
                        {
                            Console.WriteLine("Calculation History:");
                            foreach (var calc in history)
                            {
                                Console.WriteLine($"Expression: {calc.Expression}, Result: {calc.Result}");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error reading JSON data: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("No history found.");
                }
            }
            else
            {
                Console.WriteLine("No history file found.");
            }
        }

        class Calculation
        {
            public string Expression { get; set; }
            public string Result { get; set; }
        }
    }
}