using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using MPTK.NAudio.Midi;

namespace DemoMVP
{
    /// <summary>@brief
    /// This demo is able to play a individual note only by script.\n
    /// In Unity Editor:\n
    ///    Create a GameObject or reuse an existing one\n
    ///    add this script to the GameObject\n
    ///    run and use key Space.
    /// </summary>
    public class TheSimplestNotesPlayer : MonoBehaviour
    {
        // This class is able to play MIDI event: play note, play chord, patch change, apply effect, ... see doc!
        // https://mptkapi.paxstellar.com/d9/d1e/class_midi_player_t_k_1_1_midi_stream_player.html
        private MidiStreamPlayer midiStreamPlayer;   

        // Description of the MIDI event which will hold the description of the note to played and 
        // information about the samples when playing.
        // https://mptkapi.paxstellar.com/d9/d50/class_midi_player_t_k_1_1_m_p_t_k_event.html
        private MPTKEvent mptkEvent;

        private void Awake()
        {
            Debug.Log("Awake: dynamically add MidiStreamPlayer component.");
            Debug.Log("<color=green>Use key <Space> to play a note.</color>");

            // MidiPlayerGlobal is a singleton: only one instance can be created. 
            if (MidiPlayerGlobal.Instance == null)
                gameObject.AddComponent<MidiPlayerGlobal>();

            // When running, this component will be added to this gameObject. Set essential parameters.
            midiStreamPlayer = gameObject.AddComponent<MidiStreamPlayer>();
            midiStreamPlayer.MPTK_CorePlayer = true;
            midiStreamPlayer.MPTK_DirectSendToPlayer = true;
            midiStreamPlayer.MPTK_KillByExclusiveClass = true; // v2.10.0 - Will be enabled by default 
            // v2.10.0 - Now these audio components are defined as RequireComponent in MidiStreamPlayer class, there is no need to add it
            // gameObject.AddComponent<AudioReverbFilter>();
            // gameObject.AddComponent<AudioChorusFilter>();
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Assign our "Hello, World!" (using MPTKEvent's defaults value, so duration = -1 for an infinite note playing
                // Value = 60 for playing a C5 (HelperNoteLabel class could be your friend)
                mptkEvent = new MPTKEvent() { Value = 60 };

                // Start playing our "Hello, World!" note C5
                midiStreamPlayer.MPTK_PlayEvent(mptkEvent);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // Stop playing our "Hello, World!" note C5
                midiStreamPlayer.MPTK_StopEvent(mptkEvent);
            }
        }
    }
}