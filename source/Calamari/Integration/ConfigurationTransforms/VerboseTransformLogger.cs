﻿using System;
#if USE_OCTOPUS_XMLT
using Octopus.Web.XmlTransform;
#else
using Microsoft.Web.XmlTransform;
#endif

namespace Calamari.Integration.ConfigurationTransforms
{
    public class VerboseTransformLogger : IXmlTransformationLogger
    {
        public event LogDelegate Warning;
        readonly bool _suppressWarnings;
        readonly bool _suppressLogging;

        public VerboseTransformLogger(bool suppressWarnings = false, bool suppressLogging = false)
        {
            _suppressWarnings = suppressWarnings;
            _suppressLogging = suppressLogging;
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                Log.VerboseFormat(message, messageArgs);
            }
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                LogMessage(message, messageArgs);
            }
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            if (Warning != null) { Warning(this, new WarningDelegateArgs(string.Format(message, messageArgs))); }
            if (_suppressWarnings)
            {
                Log.Info(message, messageArgs);
            }
            else
            {
                Log.WarnFormat(message, messageArgs);
            }
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            if (Warning != null) { Warning(this, new WarningDelegateArgs(string.Format("{0}: {1}", file, string.Format(message, messageArgs)))); }
            if (_suppressWarnings)
            {
                Log.Info("File {0}: ", file);
                Log.Info(message, messageArgs);
            }
            else
            {
                Log.WarnFormat("File {0}: ", file);
                Log.WarnFormat(message, messageArgs);
            }
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            if (Warning != null) { Warning(this, new WarningDelegateArgs(string.Format("{0}({1},{2}): {3}", file, lineNumber, linePosition, string.Format(message, messageArgs)))); }
            if (_suppressWarnings)
            {
                Log.Info("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
                Log.Info(message, messageArgs);
            }
            else
            {
                Log.WarnFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
                Log.WarnFormat(message, messageArgs);
            }
        }

        public void LogError(string message, params object[] messageArgs)
        {
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            Log.ErrorFormat("File {0}: ", file);
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            Log.ErrorFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
            Log.ErrorFormat(message, messageArgs);
        }

        public void LogErrorFromException(Exception ex)
        {
            Log.ErrorFormat(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file)
        {
            Log.ErrorFormat("File {0}: ", file);
            Log.ErrorFormat(ex.ToString());
        }

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            Log.ErrorFormat("File {0}, line {1}, position {2}: ", file, lineNumber, linePosition);
            Log.ErrorFormat(ex.ToString());
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                Log.VerboseFormat(message, messageArgs);
            }
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                StartSection(message, messageArgs);
            }
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                Log.VerboseFormat(message, messageArgs);
            }
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            if (!_suppressLogging)
            {
                EndSection(message, messageArgs);
            }
        }
    }
}