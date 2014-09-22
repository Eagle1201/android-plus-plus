﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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

  [ComVisible(true)]

  [Guid(DebugEngineGuids.guidDebugPortSupplierStringCLSID)]

  [ClassInterface (ClassInterfaceType.None)]

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  public class DebugPortSupplier : IDebugPortSupplier3, IDebugPortSupplierDescription2
  {

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private class DebugPortEnumerator : DebugEnumerator <IDebugPort2, IEnumDebugPorts2>, IEnumDebugPorts2
    {
      public DebugPortEnumerator (List <IDebugPort2> ports)
        : base (ports)
      {
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private List <IDebugPort2> m_registeredPorts;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public DebugPortSupplier ()
    {
      m_registeredPorts = new List<IDebugPort2> ();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region IDebugPortSupplier2 Members

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int AddPort (IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
    {
      // 
      // Attempt to find a port matching the requested name, refreshes and iterates updated ports via EnumPorts.
      // 

      LoggingUtils.PrintFunction ();

      if (CanAddPort () == DebugEngineConstants.S_OK)
      {
        string requestPortName;

        IEnumDebugPorts2 portListEnum;

        if ((pRequest.GetPortName (out requestPortName) == DebugEngineConstants.S_OK) && (EnumPorts (out portListEnum) == DebugEngineConstants.S_OK))
        {
          foreach (IDebugPort2 registeredPort in m_registeredPorts)
          {
            string portName;

            if ((registeredPort.GetPortName (out portName) == DebugEngineConstants.S_OK) && (portName == requestPortName))
            {
              ppPort = registeredPort;

              return DebugEngineConstants.S_OK;
            }
          }
        }
      }

      ppPort = null;

      return DebugEngineConstants.E_FAIL;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int CanAddPort ()
    {
      // 
      // Verifies that a port supplier can add new ports.
      // 

      LoggingUtils.PrintFunction ();

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int EnumPorts (out IEnumDebugPorts2 ppEnum)
    {
      // 
      // Retrieves a list of all the ports supplied by a port supplier.
      // 

      LoggingUtils.PrintFunction ();

      AndroidAdb.Refresh ();

      AndroidDevice [] connectedDevices = AndroidAdb.GetConnectedDevices ();

      foreach (AndroidDevice device in connectedDevices)
      {
        DebuggeePort debugPort = new DebuggeePort (this, device);

        m_registeredPorts.Add (debugPort);
      }

      ppEnum = new DebugPortEnumerator (m_registeredPorts);

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetPort (ref Guid guidPort, out IDebugPort2 ppPort)
    {
      // 
      // Gets a port from a port supplier.
      // 

      LoggingUtils.PrintFunction ();

      ppPort = null;

      try
      {
        foreach (DebuggeePort registeredPort in m_registeredPorts)
        {
          Guid portGuid;

          LoggingUtils.RequireOk (registeredPort.GetPortId (out portGuid));

          if (portGuid.Equals (guidPort))
          {
            ppPort = registeredPort;

            return DebugEngineConstants.S_OK;
          }
        }
      }
      catch (Exception e)
      {
        LoggingUtils.HandleException (e);

        return DebugEngineConstants.E_FAIL;
      }

      return DebugEngineConstants.E_PORTSUPPLIER_NO_PORT;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetPortSupplierId (out Guid pguidPortSupplier)
    {
      // 
      // Gets the port supplier identifier.
      // 

      LoggingUtils.PrintFunction ();

      pguidPortSupplier = DebugEngineGuids.guidDebugPortSupplierID;

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetPortSupplierName (out string pbstrName)
    {
      // 
      // Gets the port supplier name.
      // 

      LoggingUtils.PrintFunction ();

      pbstrName = "Android++";

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int RemovePort (IDebugPort2 pPort)
    {
      // 
      // Removes a port. This method removes the port from the port supplier's internal list of active ports.
      // 

      LoggingUtils.PrintFunction ();

      try
      {
        if (!(pPort is DebuggeePort))
        {
          throw new ArgumentException ("pPort");
        }

        m_registeredPorts.Remove (pPort as DebuggeePort);

        return DebugEngineConstants.S_OK;
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

    #endregion

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region IDebugPortSupplier3 Members

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int CanPersistPorts ()
    {
      // 
      // This method determines whether the port supplier can persist ports (by writing them to disk) between invocations of the debugger.
      // 

      LoggingUtils.PrintFunction ();

      return DebugEngineConstants.S_OK;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int EnumPersistedPorts (BSTR_ARRAY PortNames, out IEnumDebugPorts2 ppEnum)
    {
      // 
      // This method retrieves an object that allows enumeration of the list of persisted ports.
      // 

      LoggingUtils.PrintFunction ();

      ppEnum = null;

      try
      {
        throw new NotImplementedException ();
      }
      catch (NotImplementedException e)
      {
        LoggingUtils.HandleException (e);

        return DebugEngineConstants.E_NOTIMPL;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #endregion

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region IDebugPortSupplierDescription2 Members

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public int GetDescription (enum_PORT_SUPPLIER_DESCRIPTION_FLAGS [] pdwFlags, out string pbstrText)
    {
      // 
      // Retrieves the description and description metadata for the port supplier.
      // 

      LoggingUtils.PrintFunction ();

      pbstrText = string.Empty;

      try
      {
        pdwFlags [0] = 0;

        pbstrText = "---";
      }
      catch (Exception e)
      {
        LoggingUtils.HandleException (e);

        return DebugEngineConstants.E_FAIL;
      }

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
