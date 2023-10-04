using MidiPlayerTK;
using MPTK.NAudio.Midi;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MIDIConvertor : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiFileLoader midiFileLoader;
    private int barNoteCount;
    private bool isBarAboutToChange;
    private List<MPTKEvent> firstBar = new List<MPTKEvent>();

    private bool isFirstBatch = false;

    public class instrumentChannelTrack
    {
        public string instrument { get; set; }
        public int channel { get; set; }
        public long track { get; set; }
    }

    // Start is called before the first frame update
    void Start()
    {
        midiFilePlayer.MPTK_PlayOnStart = false;
        isBarAboutToChange = false;
        Debug.Log($"Denomenator Time Sig: 4");

        // OnEventNotesMidi: Triggered when a group of MIDI events are read by the sequence and ready to play on the synthesizer
        // [A list of MPTKEvent are passed in the paremeter]

        midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);
    }


    public void NotesToPlay(List<MPTKEvent> midiEvents)
    {

        foreach (MPTKEvent midiEvent in midiEvents)
        {
            // Log if event is a note on
            if (midiEvent.Command == MPTKCommand.NoteOn)
            {

                int beat = midiEvent.Beat;

                // Bar has reset
                if (isBarAboutToChange == true && beat == 1)
                {
                    isBarAboutToChange = false;

                    // Send off batch of notes from the bar
                    Debug.Log("------------------------------------");
                }

                Debug.Log($"Note on Time:{midiEvent.RealTime} millisecond  Note:{midiEvent.Value}  Beat:{midiEvent.Beat}");

                // TODO: Convert to a List<List<MPTKEvents>> and parse through MidiFileLoader instead
                firstBar.Add(midiEvent);

                // Change to be from the time sig read from file
                if (beat == 4)
                {
                    isBarAboutToChange = true;
                }
            }
        }
    }
    public void ConvertToSongFile()
    {

    }

    public void LoadMidi()
    {

    }

    public void PlayMidi()
    {
        midiFilePlayer.MPTK_Play();
    }

    public void PauseMidi()
    {
        midiFilePlayer.MPTK_Pause();
    }

    public void StopMidi()
    {
        midiFilePlayer.MPTK_Stop();
    }


}
