using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cookie.BetterLogging
{
    public static partial class BetterLog
    {
#if UNITY_EDITOR // don't need the stack trace outside the editor
        /// <summary>
        ///     The path that shows up as the directory for unity's internal scripts in the stack trace
        /// </summary>
        private const string UnityBuildOutputPath = "/home/bokken/build/output";

        private static string FormatStackTrace(string originalTrace)
        {
            List<string> formattedTrace = new();
            bool didAddInternalsText = false;
            string[] splitTrace = originalTrace.Split(Environment.NewLine);

            foreach (string s in splitTrace)
            {
                if (s.Contains("Cookie.BetterLogging.BetterLog.Log"))
                    continue;

                string formatted = FormatProjectPath(s, out bool isInternal);
                if (isInternal && !didAddInternalsText)
                {
                    didAddInternalsText = true;
                    formattedTrace.Add("");
                    formattedTrace.Add("<b>Unity internals</b>");
                }

                formattedTrace.Add(formatted);
            }

            string trace = formattedTrace.Aggregate((s1, s2) => s1 + Environment.NewLine + s2);

            return trace;
        }

        private static string FormatProjectPath(string s, out bool isInternal)
        {
            isInternal = false;
            string dir = Directory.GetCurrentDirectory();
            int dirIndex = s.IndexOf(dir, StringComparison.Ordinal);

            if (dirIndex == -1)
            {
                int unityBuildOutput = s.IndexOf(UnityBuildOutputPath, StringComparison.Ordinal);

                if (unityBuildOutput == -1)
                    return s;

                StringBuilder sbUnity = new(s);
                sbUnity.Remove(unityBuildOutput, s.Length - unityBuildOutput);
                sbUnity.Append("Unity internals");

                isInternal = true;

                return sbUnity.ToString();
            }

            StringBuilder sb = new(s);
            sb.Remove(dirIndex, dir.Length + 1);
            sb.Insert(
                dirIndex,
                $"<link=\"{s[dirIndex..]}\"><color=#4c7eff>" // --unity-colors-link-text built-in uss variable
            );
            sb.Append("</link></color>");

            return sb.ToString();
        }
#endif
    }
}
