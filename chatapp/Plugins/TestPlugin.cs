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

    [KernelFunction, Description("Checks if an item is in stock or out of stock")]
    public static string CheckStock(
    [Description("The ID of the item to check")] string item)
    {
        List<string> stock = [ "123abc", "doughnuts", "99999" ];

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Checking stock..");
        Console.ResetColor();

        if (stock.Contains(item))
        {
            return "In stock";
        }
        else
        {
            return "Out of stock";
        }
    }
}
