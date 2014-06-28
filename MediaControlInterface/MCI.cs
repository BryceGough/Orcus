using Orcus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Orcus
{
    class MCI
    {

        [DllImport("winmm.dll")]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        public static void Run(string cmd, StringBuilder output)
        {
            int err = mciSendString(cmd, output, output == null ? 0 : output.Capacity, IntPtr.Zero);
            if (err > 0)
                CallException(err, cmd);
        }

        public static void Run(string cmd)
        {
            Run(cmd, null);
        }

        private static void CallException(int err, string cmd = "")
        {
            MCIError ex = (MCIError)err;
            switch (ex)
            {
                case MCIError.UNSUPPORTED_FUNCTION:
                    throw new UnsupportedFunctionException();
                default:
                    throw new OrcusException(string.Format("Error {0}({1})", (cmd.Length == 0) ? string.Empty : cmd + " ", ex.ToString()));
            }
        }

        public enum MCIError
        {
            BASE = 256,
            INVALID_DEVICE_ID = 257,
            UNRECOGNIZED_KEYWORD = 259,
            UNRECOGNIZED_COMMAND = 261,
            HARDWARE = 262,
            INVALID_DEVICE_NAME = 263,
            OUT_OF_MEMORY = 264,
            DEVICE_OPEN = 265,
            CANNOT_LOAD_DRIVER = 266,
            MISSING_COMMAND_STRING = 267,
            PARAM_OVERFLOW = 268,
            MISSING_STRING_ARGUMENT = 269,
            BAD_INTEGER = 270,
            PARSER_INTERNAL = 271,
            DRIVER_INTERNAL = 272,
            MISSING_PARAMETER = 273,
            UNSUPPORTED_FUNCTION = 274,
            FILE_NOT_FOUND = 275,
            DEVICE_NOT_READY = 276,
            INTERNAL = 277,
            DRIVER = 278,
            CANNOT_USE_ALL = 279,
            MULTIPLE = 280,
            EXTENSION_NOT_FOUND = 281,
            OUTOFRANGE = 282,
            FLAGS_NOT_COMPATIBLE = 283,
            FILE_NOT_SAVED = 286,
            DEVICE_TYPE_REQUIRED = 287,
            DEVICE_LOCKED = 288,
            DUPLICATE_ALIAS = 289,
            BAD_CONSTANT = 290,
            MUST_USE_SHAREABLE = 291,
            MISSING_DEVICE_NAME = 292,
            BAD_TIME_FORMAT = 293,
            NO_CLOSING_QUOTE = 294,
            DUPLICATE_FLAGS = 295,
            INVALID_FILE = 296,
            NULL_PARAMETER_BLOCK = 297,
            UNNAMED_RESOURCE = 298,
            NEW_REQUIRES_ALIAS = 299,
            NOTIFY_ON_AUTO_OPEN = 300,
            NO_ELEMENT_ALLOWED = 301,
            NONAPPLICABLE_FUNCTION = 302,
            ILLEGAL_FOR_AUTO_OPEN = 303,
            FILENAME_REQUIRED = 304,
            EXTRA_CHARACTERS = 305,
            DEVICE_NOT_INSTALLED = 306,
            GET_CD = 307,
            SET_CD = 308,
            SET_DRIVE = 309,
            DEVICE_LENGTH = 310,
            DEVICE_ORD_LENGTH = 311,
            NO_INTEGER = 312,
            WAVE_OUTPUTSINUSE = 320,
            WAVE_SETOUTPUTINUSE = 321,
            WAVE_INPUTSINUSE = 322,
            WAVE_SETINPUTINUSE = 323,
            WAVE_OUTPUTUNSPECIFIED = 324,
            WAVE_INPUTUNSPECIFIED = 325,
            WAVE_OUTPUTSUNSUITABLE = 326,
            WAVE_SETOUTPUTUNSUITABLE = 327,
            WAVE_INPUTSUNSUITABLE = 328,
            WAVE_SETINPUTUNSUITABLE = 329,
            SEQ_DIV_INCOMPATIBLE = 336,
            SEQ_PORT_INUSE = 337,
            SEQ_PORT_NONEXISTENT = 338,
            SEQ_PORT_MAPNODEVICE = 339,
            SEQ_PORT_MISCERROR = 340,
            SEQ_TIMER = 341,
            SEQ_PORTUNSPECIFIED = 342,
            SEQ_NOMIDIPRESENT = 343,
            NO_WINDOW = 346,
            CREATEWINDOW = 347,
            FILE_READ = 348,
            FILE_WRITE = 349,
            CUSTOM_DRIVER_BASE = 512
        }

    }
}
