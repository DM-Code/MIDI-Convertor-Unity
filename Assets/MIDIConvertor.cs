using MidiPlayerTK;
using MPTK.NAudio.Midi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MIDIConvertor : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiFileLoader midiFileLoader;
    public MidiStreamPlayer midiStreamPlayer;

    private MPTKInnerLoop innerLoop;

    private int barNoteCount;
    private bool isBarAboutToChange;
    private List<List<MPTKEvent>> midiEventsAsBars = new List<List<MPTKEvent>>();
    private List<MPTKEvent> midiEventBar = new List<MPTKEvent>();
    private bool loopFinished;

    public class instrumentChannelTrack
    {
        public string instrument { get; set; }
        public int channel { get; set; }
        public long track { get; set; }
    }

    // Start is called before the first frame update
    void Start()
    {

        // Initial required functions to preprocess MIDI
        SeperateEventsAsBars();
        innerLoop = midiFilePlayer.MPTK_InnerLoop;



        midiFilePlayer.MPTK_PlayOnStart = false;
        isBarAboutToChange = false;
        Debug.Log($"Denomenator Time Sig: 4");

        // OnEventNotesMidi: Triggered when a group of MIDI events are read by the sequence and ready to play on the synthesizer
        // [A list of MPTKEvent are passed in the paremeter]

        midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);


        ConvertToSongFile();
        //InnerLoopPractise();
    }

    public void Update()
    {
        if (loopFinished)
        {
            midiFilePlayer.MPTK_Stop();
        }
    }

    public void InnerLoopPractise()
    {
        innerLoop.Log = true;
        innerLoop.OnEventInnerLoop = (MPTKInnerLoop.InnerLoopPhase mode, long tickPlayer, long tickSeek, int count) =>
        {
            Debug.Log($"Inner Loop {mode} - MPTK_TickPlayer:{tickPlayer} --> TickSeek:{tickSeek} Count:{count}/{innerLoop.Max}");
            if (mode == MPTKInnerLoop.InnerLoopPhase.Exit)
            {
                loopFinished = true;
            }
            return true;


        };
        //1 - 1801
        innerLoop.Enabled = true;
        innerLoop.Max = 1;
        innerLoop.Start = 1;
        innerLoop.End = 1901;

        midiFilePlayer.MPTK_Play(alreadyLoaded: true);
    }

    public void ConvertToSongFile()
    {


        foreach (List<MPTKEvent> bar in midiEventsAsBars)
        {

        }

    }

    public void PlayBar(int barPosition)
    {

        if (barPosition > 0)
        {
            List<MPTKEvent> bar = midiEventsAsBars[barPosition - 1];
            int numberOfNotes = bar.Count;
            if (bar != null)
            {
                long startPosition = bar[barPosition - 1].Tick;
                long endPosition = midiEventsAsBars[barPosition][0].Tick;
                Debug.Log($"Bar {barPosition}");
                Debug.Log($"Note count: {numberOfNotes}");
                Debug.Log($"Bar Start Tick: {startPosition}");
                Debug.Log($"Bar End Tick: {endPosition}");

                innerLoop.Log = true;
                innerLoop.OnEventInnerLoop = (MPTKInnerLoop.InnerLoopPhase mode, long tickPlayer, long tickSeek, int count) =>
                {
                    Debug.Log($"Inner Loop {mode} - MPTK_TickPlayer:{tickPlayer} --> TickSeek:{tickSeek} Count:{count}/{innerLoop.Max}");
                    if (mode == MPTKInnerLoop.InnerLoopPhase.Exit)
                    {
                        loopFinished = true;
                    }
                    return true;


                };
                //1 - 1801
                innerLoop.Enabled = true;
                innerLoop.Max = 1;
                innerLoop.Start = startPosition;
                innerLoop.End = endPosition;

                midiFilePlayer.MPTK_Play(alreadyLoaded: true);
            }
        }
    }

    public void SeperateEventsAsBars()
    {
        if (midiFilePlayer.MPTK_Load() != null)
        {
            List<MPTKEvent> midiEvents = midiFilePlayer.MPTK_ReadMidiEvents();

            foreach (MPTKEvent midiEvent in midiEvents)
            {
                if (midiEvent.Command == MPTKCommand.NoteOn)
                {
                    int beat = midiEvent.Beat;

                    // Enter when the bar
                    if (isBarAboutToChange == true && beat == 1)
                    {
                        isBarAboutToChange = false;

                        // Send off batch of notes from the bar to main list
                        midiEventsAsBars.Add(midiEventBar.ToList());
                        midiEventBar.Clear();
                    }

                    midiEventBar.Add(midiEvent);

                    // TODO: Change hardcode to be from the time sig read from file
                    if (beat == 4 && isBarAboutToChange != true)
                    {
                        isBarAboutToChange = true;
                    }
                }
            }

            // Do further processing
        }
    }

    public void NotesToPlaySynth(List<MPTKEvent> midiEvents)
    {

        foreach (MPTKEvent midiEvent in midiEvents)
        {
            // Log if event is a note on
            if (midiEvent.Command == MPTKCommand.NoteOn)
            {

                int beat = midiEvent.Beat;

                // Enter when the bar
                if (isBarAboutToChange == true && beat == 1)
                {
                    isBarAboutToChange = false;

                    // Send off batch of notes from the bar
                    Debug.Log("------------------------------------");
                }

                Debug.Log($"Note on Time:{midiEvent.RealTime} millisecond  Note:{midiEvent.Value}  Beat:{midiEvent.Beat}");

                // Change to be from the time sig read from file
                if (beat == 4)
                {
                    isBarAboutToChange = true;
                }
            }
        }
    }

    public void NotesToPlay(List<MPTKEvent> midiEvents)
    {

        foreach (MPTKEvent midiEvent in midiEvents)
        {
            // Log if event is a note on
            if (midiEvent.Command == MPTKCommand.NoteOn)
            {

                int beat = midiEvent.Beat;

                // Enter when the bar
                if (isBarAboutToChange == true && beat == 1)
                {
                    isBarAboutToChange = false;

                    // Send off batch of notes from the bar
                    Debug.Log("------------------------------------");
                }

                Debug.Log($"Note on Time:{midiEvent.RealTime} millisecond  Note:{midiEvent.Value}  Tick:{midiEvent.Tick}  Beat:{midiEvent.Beat}");

                // Change to be from the time sig read from file
                if (beat == 4)
                {
                    isBarAboutToChange = true;
                }
            }
        }
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
