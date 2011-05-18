﻿using System;
using System.IO;
using js.net.Engine;
using js.net.NodeFacade;
using NUnit.Framework;

namespace js.net.node.tests
{
  [TestFixture] public class NodeTestSuite
  {
    private const string NODE_TEST_DIR = @"C:\dev\projects\labs\js.net\lib\Nodelib\test\";

    private JSNetEngineAdapter engine;
    private NodeContext ctx;

    [SetUp] public void SetUp()
    {
      ctx = new NodeContext(engine = new JSNetEngineAdapter(), ".");
      ctx.Initialise();
    }

    [TearDown] public void TearDown()
    {
      engine.Dispose();
    }

    [Test] public void RunAllTests()
    {
      string[] tests = Directory.GetFiles(NODE_TEST_DIR, "test-*.js", SearchOption.AllDirectories);
      foreach (string test in tests)
      {
        Console.WriteLine("Running Test: " + test.Replace(NODE_TEST_DIR, ""));
        engine.Run(File.ReadAllText(test));
      }
    }
  }
}
