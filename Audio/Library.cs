using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orcus.Audio
{
    public class Library : IEnumerable<Sound>
    {

        public Dictionary<string, Sound> Sounds
        { get; set; }

        public Library()
        {
            this.Sounds = new Dictionary<string, Sound>();
        }

        public Sound this[string name]
        {
            get
            {
                return Sounds[name];
            }
            set
            {
                Sounds[name] = value;
            }
        }

        public bool Contains(string name)
        {
            return this.Sounds.ContainsKey(name);
        }

        public void Open(string name, string path)
        {
            if (!this.Sounds.ContainsKey(name))
                this.Sounds.Add(name, new Sound(path, name));
            else
            {
                this.Sounds[name].Close();
                this.Sounds[name] = new Sound(path, name);
            }
        }

        public void Close(string name)
        {
            if (this.Contains(name))
            {
                this[name].Close();
                this.Sounds.Remove(name);
            }
        }

        public void Play(string name)
        {
            if (this.Sounds.ContainsKey(name))
                this[name].Play();
        }

        public void Pause(string name)
        {
            if (this.Sounds.ContainsKey(name))
                this[name].Pause();
        }

        public void Stop(string name)
        {
            if (this.Sounds.ContainsKey(name))
                this[name].Stop();
        }

        public void PlayAll()
        {
            foreach (Sound s in this.Sounds.Values)
                s.Play();
        }

        public void PauseAll()
        {
            foreach (Sound s in this.Sounds.Values)
                s.Pause();
        }

        public void StopAll()
        {
            foreach (Sound s in this.Sounds.Values)
                s.Stop();
        }

        public IEnumerator<Sound> GetEnumerator()
        {
            return this.Sounds.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
