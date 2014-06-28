using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Orcus.Exceptions;

namespace Orcus.Audio
{
    /*
     *  Please note the way this class is called is changing very soon -Bryce
     */

    public class Sound
    {
        #region Initialization

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
            this.Paused = false;
            this.Closed = false;

            MCI.Run(String.Format("open \"{0}\" type mpegvideo alias {1}", path, this.Name));
            MCI.Run(String.Format("set {0} time format milliseconds", this.Name));
            MCI.Run(String.Format("set {0} seek exactly on", this.Name));

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
                MCI.Run(String.Format("status {0} position", this.Name), s);
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
            MCI.Run(String.Format("setaudio {0} off", this.Name));
            this.Muted = true;
        }

        /// <summary>
        /// Unmutes audio
        /// </summary>
        public void Unmute()
        {
            if (!this.Muted)
                return;
            MCI.Run(String.Format("setaudio {0} on", this.Name));
            this.Muted = false;
        }

        /// <summary>
        /// Sets the bass volume
        /// </summary>
        /// <param name="value">A value between 0 and 1000</param>
        public void SetBass(int value)
        {
            MCI.Run(String.Format("setaudio {0} bass to {1}", this.Name, value));
            this.Bass = value;
        }

        /// <summary>
        /// Sets the treble volume
        /// </summary>
        /// <param name="value">A value between 0 and 1000</param>
        public void SetTreble(int value)
        {
            MCI.Run(String.Format("setaudio {0} treble to {1}", this.Name, value));
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

            MCI.Run(String.Format("setaudio {0} left volume to {1}", this.Name, left));
            MCI.Run(String.Format("setaudio {0} right volume to {1}", this.Name, right));
            this.Pan = value;


        }

        #endregion


        #region Pause/Play/Stop/Seek

        /// <summary>
        /// Plays the audio
        /// </summary>
        public void Play(bool loop = false)
        {
            if (!this.Closed && (this.Paused || !this.Playing))
            {
                MCI.Run(String.Format("play {0}{1}", this.Name, loop ? " repeat" : string.Empty));
                if (!Playing)
                    this.Playing = true;
                else if (this.Paused)
                    this.Paused = false;
            }
        }

        public void Pause()
        {
            if (this.Closed || !this.Playing || this.Paused)
            return;

            MCI.Run(String.Format("pause {0}", this.Name));
            this.Paused = true;
        }

        public void Stop()
        {
            MCI.Run(String.Format("seek {0} to start", this.Name));
            MCI.Run(String.Format("stop {0}", this.Name));
            this.Playing = false;
            this.Paused = false;
        }

        public void Seek(TimeSpan time)
        {
            double milliseconds = time.TotalMilliseconds;
            if (this.Closed || milliseconds > this.dLength || !this.Playing)
                return;

            MCI.Run(String.Format("seek {0} to {1}", this.Name, milliseconds));

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
                MCI.Run(String.Format("status {0} length", this.Name), result);
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
            MCI.Run(String.Format("close {0}", this.Name));
            this.Path = string.Empty;
            this.Playing = false;
            this.Paused = false;
            this.Closed = true;
        }

        #endregion
    }
}
