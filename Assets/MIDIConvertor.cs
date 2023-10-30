using MidiPlayerTK;
using MPTK.NAudio.Midi;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class MIDIConvertor : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiFileLoader midiFileLoader;
    public MidiStreamPlayer midiStreamPlayer;

    private MPTKInnerLoop innerLoop;

    private int barNoteCount;
    private bool isBarAboutToChange;
    // All Midi Events
    private List<MPTKEvent> allMidiEvents = new List<MPTKEvent>();
    private List<List<MPTKEvent>> midiEventsAsBars = new List<List<MPTKEvent>>();
    private List<MPTKEvent> midiEventBar = new List<MPTKEvent>();

    private List<MPTKEvent> chordEvent = new List<MPTKEvent>();
    private List<List<MPTKEvent>> chordEvents = new List<List<MPTKEvent>>();

    private bool loopFinished;

    private List<string> songConversion = new List<string>();
    private List<string> metaConversion = new List<string>();


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
        PreprocessMIDIAsBars();
        innerLoop = midiFilePlayer.MPTK_InnerLoop;



        midiFilePlayer.MPTK_PlayOnStart = false;
        isBarAboutToChange = false;
        Debug.Log($"Denomenator Time Sig: 4");

        // OnEventNotesMidi: Triggered when a group of MIDI events are read by the sequence and ready to play on the synthesizer
        // [A list of MPTKEvent are passed in the paremeter]

        midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);


        //ConvertToSongFile(-1);
        ConvertToMetaFile();


        //DisplaySongFile();
        SaveFile();
        //InnerLoopPractise();
    }

    public void Update()
    {
        if (loopFinished)
        {
            midiFilePlayer.MPTK_Stop();
        }
    }

    public void SaveFile()
    {
        string songsFolderPath = Application.persistentDataPath + "/songs/";

        string songName = midiFilePlayer.MPTK_MidiName;
        string songFileName = songName + ".song";
        string songMetaFileName = songName + ".meta";

        // Used to prevent a new line being written at the end of the file
        int counter = 0;

        if (!Directory.Exists(songsFolderPath))
        {
            Directory.CreateDirectory(songsFolderPath);
        }

        // Write data to .song file

        string destination = Application.persistentDataPath + "/songs/" + songFileName;
        List<string> fileData = songConversion;
        int songLineCount = fileData.Count;

        using (StreamWriter writer = new StreamWriter(destination))
        {
            foreach (string line in fileData)
            {
                counter++;

                if (counter != songLineCount)
                {
                    writer.WriteLine(line);
                }
                else
                {
                    writer.Write(line);
                }
            }
            Debug.Log($"File: '{songFileName}' has been saved in {destination}");

        }

        // Write data to .song.meta file

        destination = Application.persistentDataPath + "/songs/" + songMetaFileName;
        List<string> metaData = metaConversion;
        int metaLineCount = metaData.Count;
        counter = 0;

        using (StreamWriter writer = new StreamWriter(destination))
        {
            foreach (string line in metaData)
            {
                counter++;

                if (counter != songLineCount)
                {
                    writer.WriteLine(line);
                }
                else
                {
                    writer.Write(line);
                }
            }

            Debug.Log($"File: '{songFileName}' has been saved in {destination}");

        }


    }

    private void DisplaySongFile()
    {
        if (songConversion.Count >= 1)
        {
            Debug.Log("Displaying .song data...");
            foreach (string value in songConversion)
            {
                Debug.Log(value);
            }
        }
    }

    private void DisplayMetaFile()
    {
        if (metaConversion.Count >= 1)
        {
            Debug.Log("Displaying .meta data...");
            foreach (string value in metaConversion)
            {
                Debug.Log(value);
            }
        }
    }

    private void ConvertToMetaFile()
    {
        if (midiFilePlayer != null)
        {
            string artistName = "";
            int stageLevel = 0;
            string startingScores = "0, 4800";
            double bpm = midiFilePlayer.MPTK_Tempo;
            string fileMetaData = midiFilePlayer.MPTK_TextEvent;

            // First text event is USUALLY the artist name
            artistName = fileMetaData.Split('\n')[0];


            metaConversion.Add(artistName);
            metaConversion.Add(stageLevel.ToString());
            metaConversion.Add(startingScores);
            metaConversion.Add($"easy R {bpm} 1");
            metaConversion.Add($"medium R {bpm} 2");
        }
    }

    // Hard coded on the Inspector for now,
    // TODO: Grab a number from UI
    public void ConvertToSongFile(int numberOfBarsToConvert)
    {


        long firstNoteInBarTick;
        long lastNoteInBarTickEnd;
        long barLengthInTicks;

        int barCount = 0;
        int noteCount = 0;
        int convertedCount = 0;

        long tickDifference = 0;

        // Stores the tick of a current chord
        long chordInstanceTick = 0;

        // For .song conversion
        double songNoteDuration = 0;




        foreach (List<MPTKEvent> bar in midiEventsAsBars)
        {
            // TODO: Or, the convertedCount is equal to max bars in song
            if (convertedCount == numberOfBarsToConvert)
            {
                break;
            }

            if (barCount + 1 != midiEventsAsBars.Count)
            {
                // LastNoteInBarTickEnd looks at the first note in the next bar for appropriate timing
                noteCount = bar.Count;
                firstNoteInBarTick = bar[0].Tick;
                lastNoteInBarTickEnd = midiEventsAsBars[barCount + 1][0].Tick;
                barLengthInTicks = lastNoteInBarTickEnd - firstNoteInBarTick;

                Debug.Log("Bar Length In Ticks: " + barLengthInTicks);

                for (int i = 0; i < noteCount; i++)
                {
                    MPTKEvent currentNote = bar[i];

                    // Case 1: We are not at the end of the bar, therefore nextNote is the following
                    if (i != noteCount - 1)
                    {

                        MPTKEvent nextNote = bar[i + 1];

                        tickDifference = TickDifference(currentNote, nextNote);
                        if (tickDifference == 0)
                        {
                            if (chordEvent.Count == 0)
                            {
                                Debug.Log("First Note In Chord Detected");

                            }

                            chordInstanceTick = currentNote.Tick;
                            //if (chordInstanceTick == 65281)
                            //{
                            //    Debug.Log("DEBUGGER TIME");
                            //}
                            chordEvent.Add(currentNote);
                            songNoteDuration = -2;

                        }
                        // Enters when we are at the last note in the chord set
                        else if (currentNote.Tick == chordInstanceTick)
                        {
                            Debug.Log("Last Note In Chord Detected");
                            chordEvent.Add(currentNote);
                            chordEvents.Add(chordEvent.ToList());
                            chordEvent.Clear();
                            songNoteDuration = -2;

                        }
                        else
                        {
                            songNoteDuration = NoteDurationFromTick(tickDifference, barLengthInTicks);

                        }


                    }
                    // Case 2: We are at the end of the bar, therefore nextNote is the first note in the following bar
                    else
                    {
                        MPTKEvent nextNote = midiEventsAsBars[barCount + 1][0];

                        // Edge case: Check we are not at the last bar

                        if (barCount != midiEventsAsBars.Count)
                        {
                            // Edge case: Last note should wrap to the first note in the next bar



                            tickDifference = TickDifference(currentNote, nextNote);
                            if (tickDifference == 0)
                            {
                                if (chordEvent.Count == 0)
                                {
                                    Debug.Log("First Note In Chord Detected");

                                }

                                chordInstanceTick = currentNote.Tick;
                                //if (chordInstanceTick == 65281)
                                //{
                                //    Debug.Log("DEBUGGER TIME");
                                //}
                                chordEvent.Add(currentNote);
                                songNoteDuration = -2;

                            }
                            // Enters when we are at the last note in the chord set
                            else if (currentNote.Tick == chordInstanceTick)
                            {
                                Debug.Log("Last Note In Chord Detected");
                                chordEvent.Add(currentNote);
                                chordEvents.Add(chordEvent.ToList());
                                chordEvent.Clear();
                                songNoteDuration = -2;

                            }
                            else
                            {
                                songNoteDuration = NoteDurationFromTick(tickDifference, barLengthInTicks);
                            }
                        }
                    }
                    Debug.Log($"[index: {currentNote.Index}] Note: {currentNote.Value} Duration: {songNoteDuration} [bar: {barCount + 1}]");
                    songConversion.Add($"{currentNote.Value} {songNoteDuration}");


                }
            }

            // As we are at the last bar, we can't at the first note in the next bar
            // Look for tick in the index above the last note in the bar
            else
            {
                noteCount = bar.Count;

                // Look at last note, then one index above (to find ending tick)
                int lastNoteInBarEndIndex = bar[noteCount - 1].Index + 1;


                firstNoteInBarTick = bar[0].Tick;
                lastNoteInBarTickEnd = allMidiEvents[lastNoteInBarEndIndex].Tick;
                barLengthInTicks = lastNoteInBarTickEnd - firstNoteInBarTick;

                for (int i = 0; i < noteCount; i++)
                {
                    MPTKEvent currentNote = bar[i];

                    if (i != noteCount - 1)
                    {
                        tickDifference = TickDifference(bar[i], bar[i + 1]);
                        songNoteDuration = NoteDurationFromTick(tickDifference, barLengthInTicks);
                    }
                    else
                    {
                        // Edge case: Check we are not at the last bar

                        if (barCount != midiEventsAsBars.Count)
                        {
                            // Edge case: Last note should wrap to the first note in the next bar



                            tickDifference = TickDifference(bar[i], allMidiEvents[lastNoteInBarEndIndex]);
                            songNoteDuration = NoteDurationFromTick(tickDifference, barLengthInTicks);
                        }
                    }
                    Debug.Log($"[index: {currentNote.Index}] Note: {currentNote.Value} Duration: {songNoteDuration} [bar: {barCount + 1}]");
                    songConversion.Add($"{currentNote.Value} {songNoteDuration}");


                }
            }





            Debug.Log("-------------------");
            convertedCount++;
            barCount++;
        }
    }

    private long TickDifference(MPTKEvent note1, MPTKEvent note2)
    {
        long note1Tick = note1.Tick;
        long note2Tick = note2.Tick;

        return note2Tick - note1Tick;
    }

    private double NoteDurationFromTick(long tickDifference, long barLengthInTicks)
    {
        long fiveOneTwo = barLengthInTicks / 512;
        long twoFiveSixNote = barLengthInTicks / 256;
        long oneTwoEightNote = barLengthInTicks / 128;
        long sixtyFourNote = barLengthInTicks / 64;
        long threeTwoNote = barLengthInTicks / 32;
        long sixteenthNote = barLengthInTicks / 16;
        long eigthNote = barLengthInTicks / 8;
        long quarterNote = barLengthInTicks / 4;
        long halfNote = barLengthInTicks / 2;
        long wholeNote = barLengthInTicks;

        double songNoteDuration = 0;

        // Some tickDifference conditioning to combat tempo change issues
        tickDifference = (((int)tickDifference + 5) / 10 * 10);



        if (tickDifference == fiveOneTwo)
        {
            songNoteDuration = 0.00195312;
        }
        else if (tickDifference == twoFiveSixNote)
        {
            songNoteDuration = 0.00390625;
        }
        else if (tickDifference == oneTwoEightNote)
        {
            songNoteDuration = 0.0078125;
        }
        else if (tickDifference == sixtyFourNote)
        {
            songNoteDuration = 0.015625;
        }
        else if (tickDifference == threeTwoNote)
        {
            songNoteDuration = 0.03125;
        }
        else if (tickDifference == sixteenthNote)
        {
            songNoteDuration = 0.0625;
        }
        else if (tickDifference == eigthNote)
        {
            songNoteDuration = 0.125;
        }
        else if (tickDifference == quarterNote)
        {
            songNoteDuration = 0.25;
        }
        else if (tickDifference == halfNote)
        {
            songNoteDuration = 0.5;
        }
        else if (tickDifference == wholeNote)
        {
            songNoteDuration = 1;
        }

        return songNoteDuration;
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

    public void PreprocessMIDIAsBars()
    {
        int currentMeasure = 1;
        if (midiFilePlayer.MPTK_Load() != null)
        {
            allMidiEvents = midiFilePlayer.MPTK_ReadMidiEvents();

            foreach (MPTKEvent midiEvent in allMidiEvents)
            {
                if (midiEvent.Command == MPTKCommand.NoteOn)
                {
                    if (currentMeasure != midiEvent.Measure)
                    {
                        // Send off batch of notes from the bar just passed to the main list
                        midiEventsAsBars.Add(midiEventBar.ToList());
                        midiEventBar.Clear();

                        currentMeasure = midiEvent.Measure;

                    }

                    midiEventBar.Add(midiEvent);
                }


            }

            // Do last bar processing

            // Capture last bar information 
            midiEventsAsBars.Add(midiEventBar.ToList());
            midiEventBar.Clear();



        }

        Debug.Log($"Total bar count: {midiEventsAsBars.Count}");

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
