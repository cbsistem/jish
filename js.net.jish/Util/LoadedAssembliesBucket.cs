using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using js.net.jish.Command;
using js.net.jish.InlineCommand;
using Ninject;

namespace js.net.jish.Util
{
  public class LoadedAssembliesBucket
  {
    private readonly IDictionary<string, Type> commands = new Dictionary<string, Type>();    

    private readonly IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
    private readonly IDictionary<string, IDictionary<string, object>> assemblyFullyQualifiedCommands = new Dictionary<string, IDictionary<string, object>>();

    private readonly IKernel kernel;    
    private readonly HelpMgr helpManager;
    private readonly JSConsole console;

    public LoadedAssembliesBucket(HelpMgr helpManager, IKernel kernel, JSConsole console)
    {      
      this.console = console;
      this.kernel = kernel;
      this.helpManager = helpManager;
    }

    public IDictionary<string, object> AddAssembly(Assembly a)
    {
      string name = a.GetName().Name;
      if (assemblies.ContainsKey(name)) { return assemblyFullyQualifiedCommands[name]; }
      assemblies.Add(name, a);
      
      LoadAllCommandsFromAssembly(a);
      IDictionary<string, object> assemblyCommands = LoadAllInlineCommandsFromAssembly(a);
      assemblyFullyQualifiedCommands.Add(name, assemblyCommands);
      return assemblyCommands;
    }

    public bool ContainsAssembly(string assemblyName)
    {
      return assemblies.ContainsKey(GetShortNameFrom(assemblyName));
    }

    public Assembly GetAssembly(string assemblyName)
    {
      return assemblies[GetShortNameFrom(assemblyName)];
    }

    public void InterceptSpecialCommands(string input)
    {
      string commandName = new Regex(@"\.([A-z0-9])+").Match(input).Captures[0].Value.Substring(1).Trim();
      if (commandName.Equals("help"))
      {
        console.log(helpManager.GetHelpString());
        return;
      }
      if (!commands.ContainsKey(commandName))
      {
        console.log("Could not find command: " + input);
        return;
      }
      ICommand command = (ICommand) kernel.Get(commands[commandName]);
      command.Execute(ParseSpecialCommandInputs(input));
    }

    private string[] ParseSpecialCommandInputs(string input)
    {
      if (input.IndexOf('(') < 0) return null;
      input = input.Substring(input.IndexOf('(') + 1); 
      input = input.Substring(0, input.LastIndexOf(')')); 
      return Regex.Split(input, ",(?=(?:[^']*'[^']*')*[^']*$)").Select(s => s.Trim().Replace("\"", "").Replace("'", "")).ToArray();
    }

    private string GetShortNameFrom(string assemblyName)
    {
      if (assemblyName.IndexOf(',') < 0 ) return assemblyName;
      return assemblyName.Substring(0, assemblyName.IndexOf(','));
    }

    public IEnumerable<Assembly> GetAllAssemblies()
    {
      return assemblies.Values.ToArray();
    }

    private void LoadAllCommandsFromAssembly(Assembly assembly)
    {
      foreach (Type t in GetAllTypesThatImplement(assembly, typeof(ICommand)))
      {
        ICommand command = (ICommand) kernel.Get(t);
        commands.Add(command.GetName(), t);
        if (command.GetType().Assembly.FullName.IndexOf("js.net.test.module") < 0)
        {
          helpManager.AddHelpForSpecialCommand(command);
        }
      }
    }

    private IDictionary<string, object> LoadAllInlineCommandsFromAssembly(Assembly assembly)
    {
      IDictionary<string, IList<IInlineCommand>> icommands = new Dictionary<string, IList<IInlineCommand>>();
      foreach (Type t in GetAllTypesThatImplement(assembly, typeof(IInlineCommand)))
      {
        IInlineCommand icommand = (IInlineCommand) kernel.Get(t);        
        string ns = icommand.GetNameSpace();        
        if (String.IsNullOrWhiteSpace(ns)) { throw new ApplicationException("Could not load inline command from type[" + t.FullName + "].  No namespace specified.");}
        if (ns.IndexOf('.') > 0) { throw new ApplicationException("Nested namespaces (namespaces with '.' in them) are not supported."); }
        if (!icommands.ContainsKey(ns)) icommands.Add(ns, new List<IInlineCommand>());
        if (icommand.GetType().Assembly.FullName.IndexOf("js.net.test.module") < 0)
        {
          helpManager.AddHelpForInlineCommand(icommand);
        }      
        icommands[ns].Add(icommand);
      }
      
      // TODO: This is a hack, we should be returning a IL generated wrapper.
      // This would also clean up a lot of the dodgect JavaScript below.
      IDictionary<string, object> fullyQualifiedCommands = new Dictionary<string, object>();
      foreach (IInlineCommand command in icommands.Values.SelectMany(c => c))
      {
        fullyQualifiedCommands.Add(command.GetNameSpace() + "." + command.GetName(), command);
      }
      return fullyQualifiedCommands;
    }    

    private IEnumerable<Type> GetAllTypesThatImplement(Assembly assembly, Type iface)
    {
      try
      {
        Type[] types = assembly.GetTypes();
        return types.Where(t => !t.IsAbstract && iface.IsAssignableFrom(t));
      } catch (ReflectionTypeLoadException ex) {
        foreach(Exception inner in ex.LoaderExceptions) {
            Console.WriteLine(inner);
        }
        throw;
      }
    }

  }
}