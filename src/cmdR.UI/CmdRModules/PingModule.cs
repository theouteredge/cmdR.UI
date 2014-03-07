using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cmdR.UI.CmdRModules
{
    public class PingModule : ICmdRModule
    {
        private CmdR _cmdR;

        public PingModule(CmdR cmdR)
        {
            _cmdR = cmdR;

            _cmdR.RegisterRoute("ping-tcp host port timeout?", PingTcp, "Attempts to open a Tcp connection to the supplied host and port");
        }


        private Exception _exception = null;
        private string _host = null;
        private int _port = -1;
        private int _timeout = 10;
        private bool _connected = false;
        private long _elapsedMilliseconds = 0;


        private void PingTcp(IDictionary<string, string> param, CmdR cmdR)
        {
            _exception = null;
            _port = int.Parse(param["port"]);
            _host = param["host"];
            _timeout = 10;
            _elapsedMilliseconds = -1;

            if (param.ContainsKey("timeout"))
                _timeout = int.Parse(param["timeout"]);

            
            // kick off the thread that tries to connect
            var thread = new Thread(new ThreadStart(BeginTcpConnect));
            thread.IsBackground = true; // So that a failed connection attempt 
            thread.Start();

            // wait for either the timeout or the thread to finish
            thread.Join(Convert.ToInt32(TimeSpan.FromSeconds(_timeout).TotalMilliseconds));

            if (_connected)
            {
                thread.Abort();
                _cmdR.Console.WriteLine("Connected to {0}:{1} in {2}ms", _host, _port, _elapsedMilliseconds);
            }
            else
            {
                if (_exception != null)
                {
                    // it crashed, so return the exception to the caller
                    thread.Abort();

                    _cmdR.Console.WriteLine("Unable to connect to {0}:{1}. An exception was thrown: {2}", _host, _port, _exception.Message);
                }
                else
                {
                    // if it gets here, it timed out, so abort the thread and throw an exception
                    thread.Abort();

                    _cmdR.Console.WriteLine("Unable to connect to {0}:{1} timed out exceeded", _host, _port);
                }
            }
        }


        protected void BeginTcpConnect()
        {
            TcpClient connection = null;
            
            try
            {
                var sw = new Stopwatch();

                sw.Start();
                connection = new TcpClient(_host, _port);
                sw.Stop();

                
                // record that it succeeded, for the main thread to return to the caller
                _connected = true;
                _elapsedMilliseconds = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                // record the exception for the main thread to re-throw back to the calling code
                _exception = ex;
                
                if (connection != null)
                    connection.Close();
                
            }
        }
    }
}