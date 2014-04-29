﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Reflection;
using System.Resources;

using Microsoft.Build.Framework;
using Microsoft.Win32;
using Microsoft.Build.Utilities;

using AndroidPlusPlus.MsBuild.Common;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace AndroidPlusPlus.MsBuild.DeployTasks
{

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  public class JavaArchive : TrackedOutOfDateToolTask, ITask
  {

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private string m_tempWorkingDirectory;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public JavaArchive ()
      : base (new ResourceManager ("AndroidPlusPlus.MsBuild.DeployTasks.Properties.Resources", Assembly.GetExecutingAssembly ()))
    {
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Required]
    public ITaskItem OutputFile { get; set; }

    public ITaskItem ManifestFile { get; set; }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override bool Setup ()
    {
      m_tempWorkingDirectory = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ());

      if (Directory.Exists (m_tempWorkingDirectory))
      {
        Directory.Delete (m_tempWorkingDirectory, true);
      }

      Directory.CreateDirectory (m_tempWorkingDirectory);

      if (!Directory.Exists (m_tempWorkingDirectory))
      {
        Log.LogError ("Failed to create required working directory. Tried: " + m_tempWorkingDirectory);

        return false;
      }

      return base.Setup ();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override int TrackedExecuteTool (string pathToTool, string responseFileCommands, string commandLineCommands)
    {
      int retCode = base.TrackedExecuteTool (pathToTool, responseFileCommands, commandLineCommands);

      if (retCode == 0)
      {
        OutputFiles = new ITaskItem [] { OutputFile };

        // 
        // Construct a simple dependency file for tracking purposes.
        // 

        using (StreamWriter writer = new StreamWriter (OutputFile.GetMetadata ("FullPath") + ".d", false, Encoding.Unicode))
        {
          writer.WriteLine (string.Format ("{0}: \\", GccUtilities.ConvertPathWindowsToGccDependency (OutputFile.GetMetadata ("FullPath"))));

          foreach (ITaskItem source in Sources)
          {
            writer.WriteLine (string.Format ("  {0} \\", GccUtilities.ConvertPathWindowsToGccDependency (source.GetMetadata ("FullPath"))));
          }
        }
      }

      return retCode;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override string GenerateResponseFileCommands ()
    {
      StringBuilder responseFileArguments = new StringBuilder ();

      StringBuilder responseFileCommands = new StringBuilder (GccUtilities.CommandLineLength);

      // 
      // c    create new archive
      // f    specify archive file name
      // m    specify manifest file name
      // 0    store only; use no ZIP compression
      // 

      responseFileArguments.Append ("c0");

      if (ManifestFile != null)
      {
        responseFileArguments.Append ("m");

        responseFileCommands.Append (GccUtilities.ConvertPathWindowsToPosix (ManifestFile.GetMetadata ("FullPath")) + " ");
      }

      if (OutputFile != null)
      {
        responseFileArguments.Append ("f");

        responseFileCommands.Append (GccUtilities.ConvertPathWindowsToPosix (OutputFile.GetMetadata ("FullPath")) + " ");
      }

      // 
      // jar tool is a rather pants as it requires classes to be in package mapped directory structures. Use a temp directory for this.
      // 

      try
      {
        foreach (ITaskItem source in Sources)
        {
          string sourceFullPath = Path.GetFullPath (source.ItemSpec);

          string sourceFileName = Path.GetFileName (sourceFullPath);

          string classOutputPath = Path.GetFullPath (source.GetMetadata ("ClassOutputDirectory"));

          string packageDirectories = sourceFullPath.Replace (classOutputPath, "").Replace (sourceFileName, "").Trim (new char [] { '\\', '/' });

          DirectoryInfo packageDirectory = Directory.CreateDirectory (Path.Combine (m_tempWorkingDirectory, packageDirectories));

          File.Copy (sourceFullPath, Path.Combine (packageDirectory.FullName, sourceFileName), true);
        }

        responseFileCommands.Append (" -C " + GccUtilities.ConvertPathWindowsToPosix (m_tempWorkingDirectory) + " . ");
      }
      catch (Exception e)
      {
        Log.LogErrorFromException (e, true);
      }

      return responseFileArguments.ToString () + " " + responseFileCommands.ToString ();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override bool AppendSourcesToCommandLine
    {
      get
      {
        return false;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override string ToolName
    {
      get
      {
        return "JavaArchive";
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
