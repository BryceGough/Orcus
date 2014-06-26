using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Orcus.Exceptions;

namespace Orcus.Audio
{
    public class Sound
    {
        #region Initialization

        [DllImport("winmm.dll")]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
        private double dLength;

        public Sound(string path, string name)
        {
            if (!File.Exists(path))
                throw new Exception("File doesn't exist!"); // TODO Custom exceptioons?
            this.Name = name;
            this.Volume = 1000;
            this.Bass = 1000;
            this.Treble = 1000;
            this.Muted = false;
            this.Path = path;
            this.Playing = false;
            this.Loop = false;
            this.Paused = false;
            this.Closed = false;

            this.Run(String.Format("open \"{0}\" type mpegvideo alias {1}", path, this.Name));
            this.Run(String.Format("set {0} time format milliseconds", this.Name));
            this.Run(String.Format("set {0} seek exactly on", this.Name));

            if (!CalculateLength())
                throw new Exception("Failed to calculate length!");
        }

        public Sound(string path)
            : this(path, "Orcus") { }

        #endregion


        #region Properties

        public string Name
        { get; protected set; }

        public bool Closed
        { get; protected set; }

        public bool Loop
        { get; set; }

        public bool Playing
        { get; protected set; }

        public bool Paused
        { get; protected set; }

        public int Pan
        { get; protected set; }

        public string Path
        { get; protected set; }

        public int Volume
        { get; protected set; }

        public bool Muted
        { get; protected set; }

        public int Bass
        { get; protected set; }

        public int Treble
        { get; protected set; }

        public TimeSpan Length 
        {
            get
            {
                return TimeSpan.FromMilliseconds(dLength);
            }
        }

        public TimeSpan Position 
        {
            get
            {
                StringBuilder s = new StringBuilder(128);
                this.Run(String.Format("status {0} position", this.Name), s);
                double position = Convert.ToDouble(s.ToString());
                return TimeSpan.FromMilliseconds(position);
            }
        }

        #endregion


        #region Volume Control

        /// <summary>
        /// Sets the audio volume to the volume specified
        /// </summary>
        /// <param name="value">A value between 0 and 1000</param>
        public void SetVolume(int value)
        {
            if (value >= 0 && value <= 1000)
            {
                this.Volume = value;
                this.SetPan(this.Pan); // Volume is set by recalling the pan method.
                // this.Run(String.Format("setaudio {0} volume to {1}", this.Name, value));
            }
            else
                throw new OutOfRangeException();
        }

        /// <summary>
        /// Mutes audio
        /// </summary>
        public void Mute()
        {
            if (this.Muted)
                return;
            this.Run(String.Format("setaudio {0} off", this.Name));
            this.Muted = true;
        }

        /// <summary>
        /// Unmutes audio
        /// </summary>
        public void Unmute()
        {
            if (!this.Muted)
                return;
            this.Run(String.Format("setaudio {0} on", this.Name));
            this.Muted = false;
        }

        /// <summary>
        /// Sets the bass volume
        /// </summary>
        /// <param name="value">A value between 0 and 1000</param>
        public void SetBass(int value)
        {
            this.Run(String.Format("setaudio {0} bass to {1}", this.Name, value));
            this.Bass = value;
        }

        /// <summary>
        /// Sets the treble volume
        /// </summary>
        /// <param name="value">A value between 0 and 1000</param>
        public void SetTreble(int value)
        {
            this.Run(String.Format("setaudio {0} treble to {1}", this.Name, value));
            this.Treble = value;
        }

        /// <summary>
        /// Sets the audio panning
        /// </summary>
        /// <param name="value">A value between -1000 and 1000</param>
        public void SetPan(int value)
        {
            if (value < -1000 || value > 1000)
                throw new OutOfRangeException();

            int left = this.Volume;
            int right = this.Volume;
            if (value < 0)
                right = this.Volume + value;
            else if (value > 0)
                left = this.Volume - value;

            right = right < 0 ? 0 : right;
            left = left < 0 ? 0 : left;

            this.Run(String.Format("setaudio {0} left volume to {1}", this.Name, left));
            this.Run(String.Format("setaudio {0} right volume to {1}", this.Name, right));
            this.Pan = value;


        }

        #endregion


        #region Pause/Play/Stop/Seek

        /// <summary>
        /// Plays the audio
        /// </summary>
        public void Play()
        {
            if (!this.Closed && (this.Paused || !this.Playing))
            {
                this.Run(String.Format("play {0}{1}", this.Name, this.Loop ? " repeat" : string.Empty));
                if (!Playing)
                    this.Playing = true;
                else if (Paused)
                    Paused = false;
            }
        }

        public void Pause()
        {
            if (this.Closed || !this.Playing || this.Paused)
            return;

             this.Run(String.Format("pause {0}", this.Name));
            this.Paused = true;
        }

        public void Stop()
        {
            this.Run(String.Format("seek {0} to start", this.Name));
            this.Run(String.Format("stop {0}", this.Name));
            this.Playing = false;
            this.Paused = false;
        }

        public void Seek(TimeSpan time)
        {
            double milliseconds = time.TotalMilliseconds;
            if (this.Closed || milliseconds > this.dLength || !this.Playing)
                return;

            this.Run(String.Format("seek {0} to {1}", this.Name, milliseconds));

            if (this.Paused)
                this.Play();
        }

        #endregion


        #region Other
        private bool CalculateLength()
        {
            try
            {
                StringBuilder result = new StringBuilder(128);
                this.Run(String.Format("status {0} length", this.Name), result);
                this.dLength = Convert.ToUInt64(result.ToString());
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void Close()
        {
            if (!this.Closed)
                return;
            this.Run(String.Format("close {0}", this.Name));
            this.Path = string.Empty;
            this.Playing = false;
            this.Paused = false;
            this.Closed = true;
        }

        public void Run(string cmd, StringBuilder output)
        {
            int err = mciSendString(cmd, output, output == null ? 0 : output.Capacity, IntPtr.Zero);
            if (err > 0)
                CallException(err);
        }

        public void Run(string cmd)
        {
            this.Run(cmd, null);
        }

        private void CallException(int err)
        {
            this.Stop();
            MCIError ex = (MCIError)err;
            switch (ex)
            {
                case MCIError.MCIERR_UNSUPPORTED_FUNCTION:
                    throw new UnsupportedFunctionException();
                default:
                    throw new OrcusException(string.Format("Unknown ({0})", err));
            }
        }

        public enum MCIError
        {
            MCIERR_BASE = 256,
            MCIERR_INVALID_DEVICE_ID = 257,
            MCIERR_UNRECOGNIZED_KEYWORD = 259,
            MCIERR_UNRECOGNIZED_COMMAND = 261,
            MCIERR_HARDWARE = 262,
            MCIERR_INVALID_DEVICE_NAME = 263,
            MCIERR_OUT_OF_MEMORY = 264,
            MCIERR_DEVICE_OPEN = 265,
            MCIERR_CANNOT_LOAD_DRIVER = 266,
            MCIERR_MISSING_COMMAND_STRING = 267,
            MCIERR_PARAM_OVERFLOW = 268,
            MCIERR_MISSING_STRING_ARGUMENT = 269,
            MCIERR_BAD_INTEGER = 270,
            MCIERR_PARSER_INTERNAL = 271,
            MCIERR_DRIVER_INTERNAL = 272,
            MCIERR_MISSING_PARAMETER = 273,
            MCIERR_UNSUPPORTED_FUNCTION = 274,
            MCIERR_FILE_NOT_FOUND = 275,
            MCIERR_DEVICE_NOT_READY = 276,
            MCIERR_INTERNAL = 277,
            MCIERR_DRIVER = 278,
            MCIERR_CANNOT_USE_ALL = 279,
            MCIERR_MULTIPLE = 280,
            MCIERR_EXTENSION_NOT_FOUND = 281,
            MCIERR_OUTOFRANGE = 282,
            MCIERR_FLAGS_NOT_COMPATIBLE = 283,
            MCIERR_FILE_NOT_SAVED = 286,
            MCIERR_DEVICE_TYPE_REQUIRED = 287,
            MCIERR_DEVICE_LOCKED = 288,
            MCIERR_DUPLICATE_ALIAS = 289,
            MCIERR_BAD_CONSTANT = 290,
            MCIERR_MUST_USE_SHAREABLE = 291,
            MCIERR_MISSING_DEVICE_NAME = 292,
            MCIERR_BAD_TIME_FORMAT = 293,
            MCIERR_NO_CLOSING_QUOTE = 294,
            MCIERR_DUPLICATE_FLAGS = 295,
            MCIERR_INVALID_FILE = 296,
            MCIERR_NULL_PARAMETER_BLOCK = 297,
            MCIERR_UNNAMED_RESOURCE = 298,
            MCIERR_NEW_REQUIRES_ALIAS = 299,
            MCIERR_NOTIFY_ON_AUTO_OPEN = 300,
            MCIERR_NO_ELEMENT_ALLOWED = 301,
            MCIERR_NONAPPLICABLE_FUNCTION = 302,
            MCIERR_ILLEGAL_FOR_AUTO_OPEN = 303,
            MCIERR_FILENAME_REQUIRED = 304,
            MCIERR_EXTRA_CHARACTERS = 305,
            MCIERR_DEVICE_NOT_INSTALLED = 306,
            MCIERR_GET_CD = 307,
            MCIERR_SET_CD = 308,
            MCIERR_SET_DRIVE = 309,
            MCIERR_DEVICE_LENGTH = 310,
            MCIERR_DEVICE_ORD_LENGTH = 311,
            MCIERR_NO_INTEGER = 312,
            MCIERR_WAVE_OUTPUTSINUSE = 320,
            MCIERR_WAVE_SETOUTPUTINUSE = 321,
            MCIERR_WAVE_INPUTSINUSE = 322,
            MCIERR_WAVE_SETINPUTINUSE = 323,
            MCIERR_WAVE_OUTPUTUNSPECIFIED = 324,
            MCIERR_WAVE_INPUTUNSPECIFIED = 325,
            MCIERR_WAVE_OUTPUTSUNSUITABLE = 326,
            MCIERR_WAVE_SETOUTPUTUNSUITABLE = 327,
            MCIERR_WAVE_INPUTSUNSUITABLE = 328,
            MCIERR_WAVE_SETINPUTUNSUITABLE = 329,
            MCIERR_SEQ_DIV_INCOMPATIBLE = 336,
            MCIERR_SEQ_PORT_INUSE = 337,
            MCIERR_SEQ_PORT_NONEXISTENT = 338,
            MCIERR_SEQ_PORT_MAPNODEVICE = 339,
            MCIERR_SEQ_PORT_MISCERROR = 340,
            MCIERR_SEQ_TIMER = 341,
            MCIERR_SEQ_PORTUNSPECIFIED = 342,
            MCIERR_SEQ_NOMIDIPRESENT = 343,
            MCIERR_NO_WINDOW = 346,
            MCIERR_CREATEWINDOW = 347,
            MCIERR_FILE_READ = 348,
            MCIERR_FILE_WRITE = 349,
            MCIERR_CUSTOM_DRIVER_BASE = 512
        }

        #endregion
    }
}
