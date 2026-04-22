using BackendCore;

var name = args.Length > 0 ? args[0] : string.Empty;
Console.WriteLine(GreetingService.Greet(name));
