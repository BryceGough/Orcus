using Orcus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Orcus.Video
{
    public class Player
    {

        public string Name
        { get; protected set; }

        public string FileName
        { get; protected set; }

        public bool IsOpen
        { get; protected set; }

        public Control Parent
        { get; protected set; }

        public bool Playing
        { get; protected set; }

        public bool Paused
        { get; protected set; }


        public Player(Control c, string name)
        {
            this.IsOpen = false;
            this.FileName = string.Empty;
            this.Name = name;
            this.Parent = c;
        }

        public void FitToControl()
        {
            MCI.Run(string.Format("put {0} window at {1} {2} {3} {4}", this.Name, this.Parent.Location.X, this.Parent.Location.Y, this.Parent.Width, this.Parent.Height));
        }

        public void Open(string fileName)
        {
            if (this.IsOpen)
                throw new AlreadyOpenException();

            MCI.Run(string.Format("open \"{0}\" type mpegvideo alias {1}", fileName, this.Name));
            MCI.Run(string.Format("window {0} handle {1}", this.Name, this.Parent.Handle));
            MCI.Run(string.Format("put {0} destination", this.Name));
            this.FitToControl();

            this.FileName = fileName;
            this.IsOpen = true;
        }

        public void Close()
        {
            MCI.Run(string.Format("close {0}", this.Name));

            this.FileName = string.Empty;
            this.IsOpen = false;
            this.Playing = false;

        }

        public void Play(bool loop = false)
        {
            if (this.IsOpen && (this.Paused || !this.Playing))
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
            if (!this.IsOpen)
                throw new NotOpenException();
            if (!this.Playing || this.Paused)
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

    }
}
