using MidiPlayerTK;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


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
    private List<MPTKEvent> midiEventSingleBar = new List<MPTKEvent>();

    private List<MPTKEvent> chordEvent = new List<MPTKEvent>();
    private List<List<MPTKEvent>> chordEvents = new List<List<MPTKEvent>>();

    private bool loopFinished;

    private List<string> songConversion = new List<string>();
    private List<string> metaConversion = new List<string>();

    private HashSet<instrumentChannelTrack> instrumentChannelTrackSet = new HashSet<instrumentChannelTrack>();


    public class instrumentChannelTrack
    {
        public string instrumentName { get; set; }
        public string sequenceName { get; set; }
        public int channel { get; set; }
        public long track { get; set; }
    }

    // Start is called before the first frame update
    void Start()
    {

        // PreprocessMIDI does the following:
        // 1) Processes the midi as 'bars' for the program
        // 2) Stores the instrument, track and channel instances to seperate each track
        PreprocessMIDI();
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
    //public void PreProcessStep1()
    //{
    //    List<MPTKEvent> infoEvents;
    //    // Step 1: Check if MIDI File has SequenceTrackName Meta (Put in by the creator to name tracks)
    //    if (midiFilePlayer.MPTK_Load() != null)
    //    {
    //        foreach (MPTKEvent midiEvent in allMidiEvents)
    //        {

    //            if (midiEvent.Command == MPTKCommand.NoteOn || midiEvent.Command == MPTKCommand.NoteOff)
    //            {
    //                // Exit For Each
    //                break;
    //            }
    //        }

    //        Debug.Log("We have excited");
    //    }
    //}
    public void PreprocessMIDI()
    {

        // Step 1: Parse through and add a list of MIDI information setters
        if (midiFilePlayer.MPTK_Load() != null)
        {
            // 16 Channels of Data
            List<instrumentChannelTrack> midiSetterEvents = new List<instrumentChannelTrack>(16);

            // Create 16 empty instrumentChannelTracks
            for (int i = 0; i < 16; i++)
            {
                midiSetterEvents.Add(new instrumentChannelTrack { instrumentName = "", sequenceName = "", channel = -1, track = i });
            }

            allMidiEvents = midiFilePlayer.MPTK_ReadMidiEvents();

            foreach (MPTKEvent midiEvent in allMidiEvents)
            {
                int trackIndex = (int)midiEvent.Track;

                // Store information of interest
                if (midiEvent.Meta == MPTKMeta.SequenceTrackName)
                {
                    string sequenceName = midiEvent.Info;
                    midiSetterEvents[trackIndex].sequenceName = sequenceName;
                    midiSetterEvents[trackIndex].channel = midiEvent.Channel;
                }
                else if (midiEvent.Command == MPTKCommand.PatchChange)
                {
                    string instrumentName = GetPatchName(midiEvent.Value);
                    midiSetterEvents[trackIndex].instrumentName = instrumentName;
                    midiSetterEvents[trackIndex].channel = midiEvent.Channel;
                }
                else if (midiEvent.Command == MPTKCommand.NoteOn || midiEvent.Command == MPTKCommand.NoteOff)
                {
                    break;
                }
            }

            Debug.Log("We have hit a note on/off");
            Debug.Log("Displaying stored info");

            List<instrumentChannelTrack> activeTracks = new List<instrumentChannelTrack>();

            foreach (instrumentChannelTrack ict in midiSetterEvents)
            {
                if (ict.sequenceName != "" && ict.instrumentName != "")
                {
                    activeTracks.Add(ict);
                }

            }

            foreach (instrumentChannelTrack ict in activeTracks)
            {
                Debug.Log($"Sequence: [{ict.sequenceName}] | Instrument [{ict.instrumentName}] | Track [{ict.track}] ");
            }

        }


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
                        midiEventsAsBars.Add(midiEventSingleBar.ToList());
                        midiEventSingleBar.Clear();

                        currentMeasure = midiEvent.Measure;

                    }

                    midiEventSingleBar.Add(midiEvent);
                }


            }

            // Do last bar processing

            // Capture last bar information 
            midiEventsAsBars.Add(midiEventSingleBar.ToList());
            midiEventSingleBar.Clear();



        }

        Debug.Log($"Total bar count: {midiEventsAsBars.Count}");

    }

    // Pulled from MPTK Package

    /// <summary>@brief
    /// Gets the default MIDI instrument names
    /// </summary>
    public static string GetPatchName(int patchNumber)
    {
        if (patchNumber < patchNames.Length)
            return patchNames[patchNumber];
        else
            return "unknown";
    }

    // TODO: localize
    private static readonly string[] patchNames = new string[]
    {
            "Acoustic Grand","Bright Acoustic","Electric Grand","Honky-Tonk","Electric Piano 1","Electric Piano 2","Harpsichord","Clav",
            "Celesta","Glockenspiel","Music Box","Vibraphone","Marimba","Xylophone","Tubular Bells","Dulcimer",
            "Drawbar Organ","Percussive Organ","Rock Organ","Church Organ","Reed Organ","Accoridan","Harmonica","Tango Accordian",
            "Acoustic Guitar(nylon)","Acoustic Guitar(steel)","Electric Guitar(jazz)","Electric Guitar(clean)","Electric Guitar(muted)","Overdriven Guitar","Distortion Guitar","Guitar Harmonics",
            "Acoustic Bass","Electric Bass(finger)","Electric Bass(pick)","Fretless Bass","Slap Bass 1","Slap Bass 2","Synth Bass 1","Synth Bass 2",
            "Violin","Viola","Cello","Contrabass","Tremolo Strings","Pizzicato Strings","Orchestral Strings","Timpani",
            "String Ensemble 1","String Ensemble 2","SynthStrings 1","SynthStrings 2","Choir Aahs","Voice Oohs","Synth Voice","Orchestra Hit",
            "Trumpet","Trombone","Tuba","Muted Trumpet","French Horn","Brass Section","SynthBrass 1","SynthBrass 2",
            "Soprano Sax","Alto Sax","Tenor Sax","Baritone Sax","Oboe","English Horn","Bassoon","Clarinet",
            "Piccolo","Flute","Recorder","Pan Flute","Blown Bottle","Skakuhachi","Whistle","Ocarina",
            "Lead 1 (square)","Lead 2 (sawtooth)","Lead 3 (calliope)","Lead 4 (chiff)","Lead 5 (charang)","Lead 6 (voice)","Lead 7 (fifths)","Lead 8 (bass+lead)",
            "Pad 1 (new age)","Pad 2 (warm)","Pad 3 (polysynth)","Pad 4 (choir)","Pad 5 (bowed)","Pad 6 (metallic)","Pad 7 (halo)","Pad 8 (sweep)",
            "FX 1 (rain)","FX 2 (soundtrack)","FX 3 (crystal)","FX 4 (atmosphere)","FX 5 (brightness)","FX 6 (goblins)","FX 7 (echoes)","FX 8 (sci-fi)",
            "Sitar","Banjo","Shamisen","Koto","Kalimba","Bagpipe","Fiddle","Shanai",
            "Tinkle Bell","Agogo","Steel Drums","Woodblock","Taiko Drum","Melodic Tom","Synth Drum","Reverse Cymbal",
            "Guitar Fret Noise","Breath Noise","Seashore","Bird Tweet","Telephone Ring","Helicopter","Applause","Gunshot"
    };

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
