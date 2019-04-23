using UnityEngine;
public class CMDHelper
    {
        public static string ProcessCommand(string command, string argument)
        {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(command);
            start.Arguments = argument;
            start.CreateNoWindow = false;
            start.ErrorDialog = true;
            start.UseShellExecute = false;

            if (start.UseShellExecute)
            {
                start.RedirectStandardOutput = false;
                start.RedirectStandardError = false;
                start.RedirectStandardInput = false;

                System.Diagnostics.Process p = System.Diagnostics.Process.Start(start);
                p.WaitForExit();
                p.Close();
                return "";
            }
            else
            {
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                // start.RedirectStandardInput = true;
                start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                start.StandardErrorEncoding = System.Text.Encoding.UTF8;

                System.Diagnostics.Process p = System.Diagnostics.Process.Start(start);
                p.WaitForExit();
                string error = p.StandardError.ReadToEnd();
                string output = p.StandardOutput.ReadToEnd();
                p.Close();
                Debug.Log("ProcessCommand OutPut:" + output);
                return error;
            }

            
        }

		public static void ProcessCommandEx(string command, string argument)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process ();
			process.StartInfo.FileName = command;
			process.StartInfo.Arguments = argument;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler (OnOutputDataReceived);
			process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler (OnErrorDataReceived);
			process.Start ();
			process.BeginOutputReadLine ();
			process.BeginErrorReadLine ();
			process.WaitForExit ();
		}

		static void OnOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs args)
		{
			if (!string.IsNullOrEmpty (args.Data)) {
				UnityEngine.Debug.Log (args.Data);
			}
		}

		static void OnErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs args)
		{
			if (!string.IsNullOrEmpty (args.Data)) {
				UnityEngine.Debug.LogError (args.Data);
			}
		}
    }