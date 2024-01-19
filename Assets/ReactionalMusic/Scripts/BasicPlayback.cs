using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Reactional.Core
{
    [DefaultExecutionOrder(5000)]
    public class BasicPlayback : MonoBehaviour
    {
        [SerializeField] private bool _autoplayTheme;
        [SerializeField] private bool _autoplayTrack;

        public void Start()
        {
            if (Reactional.Setup.IsValid)
            {                
                Play();
            } else {
                Debug.LogWarning("Reactional is not setup correctly. Please check the setup guide.");
            }
        }

        private async void Play()
        {           
            await Task.Delay(100);
            // Load playlist metadata tracks
            await Reactional.Playback.Playlist.LoadAll();

            // Loads the first theme defined and its assets
            await Reactional.Playback.Theme.LoadTheme();

            if (_autoplayTheme)
                Reactional.Playback.Theme.Start();
            if (_autoplayTrack)
                Reactional.Playback.Playlist.Start();

            Reactional.Setup.InitAudio();           
            
            await Task.Delay(200);
            Reactional.Setup.AllowPlay = true;               
        }
    }
}