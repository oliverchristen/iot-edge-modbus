﻿namespace AzureIoTEdgeModbus.Slave
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains the handle for this module. In this case, it is a list of active Modbus sessions.
    /// </summary>
    public class SessionsHandle : ISessionsHandle
    {
        public async Task<SessionsHandle> CreateHandleFromConfiguration(ModuleConfig config)
        {
            SessionsHandle sessionsHandle = null;
            foreach (var configPair in config.SlaveConfigs)
            {
                ModbusSlaveConfig slaveConfig = configPair.Value;

                switch (slaveConfig.GetConnectionType())
                {
                    case ConnectionType.ModbusTCP:
                        {
                            sessionsHandle = sessionsHandle ?? new SessionsHandle();

                            ModbusSlaveSession slave = new ModbusTCPSlaveSession(slaveConfig);
                            await slave.InitSessionAsync().ConfigureAwait(false);
                            sessionsHandle.ModbusSessionList.Add(slave);
                            break;
                        }
                    case ConnectionType.ModbusRTU:
                        {
                            sessionsHandle = sessionsHandle ?? new SessionsHandle();

                            ModbusSlaveSession slave = new ModbusRTUSlaveSession(slaveConfig);
                            await slave.InitSessionAsync().ConfigureAwait(false);
                            sessionsHandle.ModbusSessionList.Add(slave);
                            break;
                        }
                    case ConnectionType.ModbusASCII:
                        {
                            break;
                        }
                    case ConnectionType.Unknown:
                        {
                            break;
                        }
                }
            }
            return sessionsHandle;
        }

        public readonly List<ModbusSlaveSession> ModbusSessionList = new List<ModbusSlaveSession>();

        public void Release()
        {
            foreach (var session in this.ModbusSessionList)
            {
                session.ReleaseSessionAsync();
            }
            this.ModbusSessionList.Clear();
        }

        public async Task<List<ModbusOutContent>> CollectAndResetOutMessageFromSessionsAsync()
        {
            var contents = new List<ModbusOutContent>();

            foreach (ModbusSlaveSession session in this.ModbusSessionList)
            {
                var message = session.GetOutMessage();
                if (message != null)
                {
                    contents.Add(message);
                    await session.ClearOutMessageAsync().ConfigureAwait(false);
                }
            }
            return contents;
        }
    }
}