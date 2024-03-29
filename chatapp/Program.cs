using Azure.AI.OpenAI;
using chatapp.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0055, SKEXP0001, SKEXP0010, SKEXP0050, SKEXP0020 // Experimental

string apikey = Environment.GetEnvironmentVariable("chatappApiKey")!;
string endpoint = Environment.GetEnvironmentVariable("chatappEndpoint")!;
string deployment = Environment.GetEnvironmentVariable("chatappDeploymentName")!;

// Initialize the kernel
IKernelBuilder kb = Kernel.CreateBuilder();
kb.AddAzureOpenAIChatCompletion(deployment!, endpoint!, apiKey: apikey);
kb.Plugins.AddFromType<MathPlugin>();
kb.Plugins.AddFromType<TestPlugin>();

kb.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Information));
kb.Services.ConfigureHttpClientDefaults(x => x.AddStandardResilienceHandler());
Kernel kernel = kb.Build();

// create embeddings for a document
ISemanticTextMemory memory = new MemoryBuilder()
    .WithLoggerFactory(kernel.LoggerFactory)
    //.WithMemoryStore(await SqliteMemoryStore.ConnectAsync("wikidata.db"))
    .WithMemoryStore(new QdrantMemoryStore("http://localhost:6333/", 3072))
    .WithAzureOpenAITextEmbeddingGeneration("OAIZKEMBED", endpoint, apikey)
    .Build();

IList<string> collections = await memory.GetCollectionsAsync();
string collectionName = "article_q";
//if (collections.Contains(collectionName))
    if (true)
    {
    Console.WriteLine("Found database");
}
else
{
    using HttpClient client = new();
    
        string s = await client.GetStringAsync("https://simple.wikipedia.org/wiki/Francis_Scott_Key_Bridge_collapse");
        var paragraphs =
            TextChunker.SplitPlainTextParagraphs(
                TextChunker.SplitPlainTextLines(
                    WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", "")),
                    128),
                1024);
        for (int i = 0; i < paragraphs.Count; i++)
            await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
            
    Console.WriteLine("Generated database");
}
// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();

OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


string answer = await kernel.InvokeAsync<string>("TestPlugin", "RemyFood");
Console.WriteLine($"result: :::::  {answer}.");


// Q&A loop
while (true)
{
    Console.Write("Question: ");
    string question = Console.ReadLine()!;

    builder.Clear();
    await foreach (var result in memory.SearchAsync(collectionName, question, limit: 5))
        builder.AppendLine(result.Metadata.Text);
    //embeddings presently just finding and appending the beginning of html doc
    int contextToRemove = -1;
    if (builder.Length !=0)
    {
        builder.Insert(0, "Here's some additional information: ");
        contextToRemove = chat.Count;
        chat.AddUserMessage(builder.ToString());
        // chat.AddUserMessage("On March 26, 2024, at 01:27 EDT (05:27 UTC), the main parts of the Francis Scott Key Bridge, across the Patapsco River in Baltimore Harbor, Baltimore, Maryland, United States, collapsed after the Singaporean-flagged container ship Dali struck one of its support pillars.");
    }

    chat.AddUserMessage(question);

    // Console.WriteLine(builder.ToString());
    builder.Clear();

    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, settings, kernel))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }

    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    //if (contextToRemove >= 0) chat.RemoveAt(contextToRemove);
    Console.WriteLine();

}