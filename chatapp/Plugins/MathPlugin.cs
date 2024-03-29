using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatapp.Plugins;

public sealed class MathPlugin
{
    [KernelFunction, Description("Multiply two numbers together")]
    public static double Multiply(
        [Description("The first number to multiply")] double a, 
        [Description("The second number to multiply")] double b)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Multiplication of two numbers");
       Console.ResetColor();
        return a * b;
    }
}
