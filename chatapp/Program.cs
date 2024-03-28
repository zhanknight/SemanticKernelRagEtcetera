using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


string apikey = Environment.GetEnvironmentVariable("chatappApiKey")!;
string endpoint = Environment.GetEnvironmentVariable("chatappEndpoint")!;
string deployment = Environment.GetEnvironmentVariable("chatappDeploymentName")!;

// Initialize the kernel
IKernelBuilder kb = Kernel.CreateBuilder();
kb.AddAzureOpenAIChatCompletion(deployment!, endpoint!, apiKey: apikey);
kb.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = kb.Build();

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();
OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, settings, kernel))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());
}