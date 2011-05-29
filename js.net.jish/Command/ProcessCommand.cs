﻿using System;
using System.Diagnostics;

namespace js.net.jish.Command
{
  public class ProcessCommand : ParseInputCommand, ICommand
  {
    private readonly JSConsole console;

    public ProcessCommand(ICommandLineInterpreter cli, JSConsole console)
    {
      this.console = console;
    }

    #region ICommand Members

    public string GetName()
    {
      return "process";
    }

    public string GetHelpDescription()
    {
      return "Executes the command in a separate Process.";
    }

    public void Execute(string input)
    {
      string commandAndArgs = GetCommandAndArgsFromInput(input);

      string args, command;
      ParseCommandAndArguments(commandAndArgs, out command, out args);
      RunCommandWithArgs(commandAndArgs, command, args);
    }

    #endregion

    private string GetCommandAndArgsFromInput(string input)
    {
      string commandAndArgs = ParseFileOrTypeName(input);
      if (commandAndArgs.StartsWith("'") || commandAndArgs.StartsWith("\""))
        commandAndArgs = commandAndArgs.Substring(1);
      if (commandAndArgs.EndsWith("'") || commandAndArgs.StartsWith("\""))
        commandAndArgs = commandAndArgs.Substring(0, commandAndArgs.Length - 1);
      return commandAndArgs;
    }

    private void ParseCommandAndArguments(string commandAndArgs, out string command, out string args)
    {
      int spaceIdx = commandAndArgs.IndexOf(' ');

      command = commandAndArgs;
      args = String.Empty;
      if (spaceIdx <= 0) return;

      command = commandAndArgs.Substring(0, commandAndArgs.IndexOf(' '));
      args = commandAndArgs.Substring(commandAndArgs.IndexOf(' ') + 1);
    }

    private void RunCommandWithArgs(string commandAndArgs, string command, string args)
    {      
      using (var process = new Process
                      {
                        StartInfo =
                          {
                            FileName = command,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                          }
                      })
      {
        process.Start();
        string err = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();
        if (!String.IsNullOrWhiteSpace(err)) console.log(err);
        if (!String.IsNullOrWhiteSpace(output)) console.log(output);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
          throw new ApplicationException("Process " + commandAndArgs + " exited with code: " + process.ExitCode);
        }
      }
    }
  }
}