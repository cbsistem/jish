﻿using System.Diagnostics;
using System.IO;

namespace js.net
{
  public class HtmlFileScriptExtractor
  {
    private readonly string file;
    public HtmlFileScriptExtractor(string file) 
    {
      Trace.Assert(file.EndsWith(".html") || file.EndsWith(".htm"));

      this.file = file;
    }

    public string GetScriptContents()
    {
      string html = File.ReadAllText(file);
      string js = "";
      while (html.IndexOf("\n<script") >= 0)
      {
        html = html.Substring(html.IndexOf(">", html.IndexOf("\n<script")) + 1);
        js += "\n" + html.Substring(0, html.IndexOf("</script>"));
      }
      File.WriteAllText(file.Replace(".html", ".debug.js").Replace(".htm", ".debug.js"), js);
      return js;
    }
  }
}