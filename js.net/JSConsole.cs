﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace js.net
{
  public class JSConsole
  {
    public void print(string error) { log(error); }
    public void error(string error) { log(error); }
    public void debug(string message) { log(message); }
    public void info(string message) { log(message); }
    public void dir(object message) { log(message); }

    public virtual void log(object message)
    {
      Trace.Assert(message != null);

      Console.WriteLine(GetMessageObjectDescription(message));      
    }

    private string GetMessageObjectDescription(object msg)
    {
      if (msg == null)
      {
        return String.Empty; 
      }

      if (msg is object[])
      {
        return GetArrayDescription(msg);
      }

      if (msg is IDictionary<string, object>)
      {
        return GetDictionaryDescription(msg);
      }

      return msg.ToString();
    }

    
    private string GetArrayDescription(object msg)
    {
      object[] arr = (object[]) msg;
      return String.Join(", ", arr.Select(GetMessageObjectDescription));
    }

    private string GetDictionaryDescription(object msg)
    {
      var objectMessage = (IDictionary<string, object>) msg;
      var obj = new StringBuilder('{');
      foreach (var prop in objectMessage)
      {
        obj.Append("\n ").Append(prop.Key).Append(": ");
        obj.Append(GetMessageObjectDescription(prop.Value));        
      }
      obj.Append("\n}");
      return obj.ToString();
    }
  }
}