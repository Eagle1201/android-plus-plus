﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Debugger.Interop;

using AndroidPlusPlus.Common;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace AndroidPlusPlus.VsDebugEngine
{

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  [ComVisible (true)]

  [Guid (DebugEngineGuids.guidDebugProgramProviderStringCLSID)]

  [ClassInterface (ClassInterfaceType.None)]

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  public class DebugProgramProvider : IDebugProgramProvider2
  {

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region IDebugProgramProvider2 Members

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    int IDebugProgramProvider2.GetProviderProcessData (enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 port, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA [] processArray)
    {
      // 
      // Retrieves a list of running programs from a specified process.
      // 

      LoggingUtils.PrintFunction ();

      try
      {
        processArray [0] = new PROVIDER_PROCESS_DATA ();

        if ((Flags & enum_PROVIDER_FLAGS.PFLAG_GET_PROGRAM_NODES) != 0)
        {
          // The debugger is asking the engine to return the program nodes it can debug. The 
          // sample engine claims that it can debug all processes, and returns exactly one
          // program node for each process. A full-featured debugger may wish to examine the
          // target process and determine if it understands how to debug it.

          IDebugProgramNode2 node = (IDebugProgramNode2)(new DebuggeeProgram (null));

          IntPtr [] programNodes = { Marshal.GetComInterfaceForObject (node, typeof (IDebugProgramNode2)) };

          IntPtr destinationArray = Marshal.AllocCoTaskMem (IntPtr.Size * programNodes.Length);

          Marshal.Copy (programNodes, 0, destinationArray, programNodes.Length);

          processArray [0].Fields = enum_PROVIDER_FIELDS.PFIELD_PROGRAM_NODES;

          processArray [0].ProgramNodes.Members = destinationArray;

          processArray [0].ProgramNodes.dwCount = (uint)programNodes.Length;

          return DebugEngineConstants.S_OK;
        }

        return DebugEngineConstants.S_FALSE;
      }
      catch (Exception e)
      {
        LoggingUtils.HandleException (e);

        return DebugEngineConstants.E_FAIL;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetProviderProgramNode (enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
    {
      // 
      // Retrieves the program node for a specific program. Not implemented. 
      // 

      LoggingUtils.PrintFunction ();

      ppProgramNode = null;

      // This method is used for Just-In-Time debugging support, which this program provider does not support
      return DebugEngineConstants.E_NOTIMPL; 
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int SetLocale (ushort wLangID)
    {
      // 
      // Establishes a locale to be used for any locale-specific resources.
      // 

      LoggingUtils.PrintFunction ();

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int WatchForProviderEvents (enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
    {
      // 
      // Allows the process to be notified of port events.
      //

      // The sample debug engine is a native debugger, and can therefore always provide a program node
      // in GetProviderProcessData. Non-native debuggers may wish to implement this method as a way
      // of monitoring the process before code for their runtime starts. For example, if implementing a 
      // 'foo script' debug engine, one could attach to a process which might eventually run 'foo script'
      // before this 'foo script' started.
      //
      // To implement this method, an engine would monitor the target process and call AddProgramNode
      // when the target process started running code which was debuggable by the engine. The 
      // enum_PROVIDER_FLAGS.PFLAG_ATTACHED_TO_DEBUGGEE flag indicates if the request is to start
      // or stop watching the process.

      LoggingUtils.PrintFunction ();

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #endregion

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
