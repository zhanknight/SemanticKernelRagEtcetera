using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatapp.Plugins;

public sealed class TestPlugin
{
    [KernelFunction, Description("Gets Remy's favorite food")]
    public static string RemyFood()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Remy Food");
       Console.ResetColor();

        return "French Fries";
    }
}
