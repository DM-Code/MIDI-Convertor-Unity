using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>@brief
    /// MIDI command codes. Defined the action to be done with the message: note on/off, change instrument, ...\n
    /// Depending of the command selected, others properties must be set; Value, Channel, ...\n
    /// </summary>
    public enum MPTKCommand : byte
    {
        /// <summary>@brief
        /// Note Off\n
        /// Stop the note defined with the Value and the Channel\n
        ///      - MPTKEvent#Value contains the note to stop 60=C5.\n
        ///      - MPTKEvent#Channel the midi channel between 0 and 15\n
        /// </summary>
        NoteOff = 0x80,

        /// <summary>@brief
        /// Note On.\n
        ///      - MPTKEvent#Value contains the note to play 60=C5.\n
        ///      - MPTKEvent#Duration the duration of the note in millisecond, -1 for infinite\n
        ///      - MPTKEvent#Channel the midi channel between 0 and 15\n
        ///      - MPTKEvent#Velocity between 0 and 127\n
        /// </summary>
        NoteOn = 0x90,

        /// <summary>@brief
        /// Key After-touch.\n
        /// Not processed by Maestro Synth.
        /// </summary>
        KeyAfterTouch = 0xA0,


        /// <summary>@brief
        /// Control change.\n
        ///      - MPTKEvent.Controller contains the controller to change. See #MPTKController (Modulation, Pan, Bank Select ...).\n
        ///      - MPTKEvent.Value contains the value of the controller between 0 and 127.
        /// </summary>
        ControlChange = 0xB0,

        /// <summary>@brief
        /// Patch change.\n
        ///      - MPTKEvent.Value contains patch/preset/instrument to select between 0 and 127. 
        /// </summary>
        PatchChange = 0xC0,

        /// <summary>@brief
        /// Channel after-touch.\n
        /// Not processed by Maestro Synth.\n
        /// </summary>
        ChannelAfterTouch = 0xD0,

        /// <summary>@brief
        /// Pitch wheel change\n
        /// MPTKEvent.Value contains the Pitch Wheel Value between 0 and 16383.\n
        /// Higher values transpose pitch up, and lower values transpose pitch down.\n
        /// The default sensitivity value is 2. That means that the maximum pitch bend will result in a pitch change of two semitones\n
        /// above and below the sounding note, meaning a total of four semitones from lowest to highest  pitch bend positions.
        ///     - 0 is the lowest bend positions (default is 2 semitones), 
        ///     - 8192 (0x2000) centered value, the sounding notes aren't being transposed up or down,
        ///     - 16383 (0x3FFF) is the highest  pitch bend position (default is 2 semitones)
        /// </summary>
        PitchWheelChange = 0xE0,

        /// <summary>@brief
        /// Sysex message - not processed by Maestro\n
        /// </summary>
        Sysex = 0xF0,

        /// <summary>@brief
        /// Eox (comes at end of a sysex message)  - not processed by Maestro
        /// </summary>
        Eox = 0xF7,

        /// <summary>@brief
        /// Timing clock \n
        /// (used when synchronization is required)
        /// </summary>
        TimingClock = 0xF8,

        /// <summary>@brief
        /// Start sequence\n
        /// </summary>
        StartSequence = 0xFA,

        /// <summary>@brief
        /// Continue sequence\n
        /// </summary>
        ContinueSequence = 0xFB,

        /// <summary>@brief
        /// Stop sequence\n
        /// </summary>
        StopSequence = 0xFC,

        /// <summary>@brief
        /// Auto-Sensing\n
        /// </summary>
        AutoSensing = 0xFE,

        /// <summary>@brief
        /// Meta events are optionnals information that could be defined in a MIDI. None are mandatory\n
        /// In MPTKEvent the attibute MPTKEvent#Meta defined the type of meta event. See #MPTKMeta (TextEvent, Lyric, TimeSignature, ...).\n
        ///     - if MPTKEvent#Meta = #MPTKMeta.SetTempo, MPTKEvent#Value contains new Microseconds Per Beat Note. . Please investigate MPTKEvent.MPTK_MPQN2BPM() to convert to BPM.
        ///     - if MPTKEvent#Meta = #MPTKMeta.TimeSignature, MPTKEvent#Value contains four bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///         -# Numerator (number of beats in a bar), 
        ///         -# Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)
        ///         -# TicksInMetronomeClick, generally 24 (number of 1/32nds of a note happen by MIDI quarter note)
        ///         -# No32ndNotesInQuarterNote, generally 8 (standard MIDI clock ticks every 24 times every quarter note)
        ///     - if MPTKEvent#Meta = #MPTKMeta.KeySignature, MPTKEvent#Value contains two bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///         -# SharpsFlats (number of sharp) 
        ///         -# MajorMinor flag (0 the scale is major, 1 the scale is minor).
        ///     - for others, attribute MPTKEvent#Info contains textual information.
        /// </summary>
        MetaEvent = 0xFF,
    }

    /// <summary>@brief
    /// Midi Controller list.\n
    /// Each MIDI CC operates at 7-bit resolution, meaning it has 128 possible values. The values start at 0 and go to 127.\n
    /// Some instruments can receive higher resolution data for their MIDI control assignments. These high res assignments are defined by combining two separate CCs,\n
    /// one being the Most Significant Byte (MSB), and one being the Least Significant Byte (LSB).\n
    /// Most instruments just receive the MSB with default 7-bit resolution.
    /// See more information here https://www.presetpatch.com/midi-cc-list.aspx
    /// </summary>
    public enum MPTKController : byte
    {
        /// <summary>@brief
        /// Bank Select (MSB)
        /// </summary>
        BankSelectMsb = 0,

        /// <summary>@brief
        /// Modulation (MSB)
        /// </summary>
        Modulation = 1,

        /// <summary>@brief
        /// Breath Controller
        /// </summary>
        BreathController = 2,

        /// <summary>@brief
        /// Foot controller (MSB)
        /// </summary>
        FootController = 4,

        PORTAMENTO_TIME_MSB = 0x05,

        DATA_ENTRY_MSB = 6,

        /// <summary>@brief
        /// Channel volume (was MainVolume before v2.88.2
        /// </summary>
        VOLUME_MSB = 7,

        BALANCE_MSB = 8,

        /// <summary>@brief Pan MSB</summary>
        Pan = 10, //0xA

        /// <summary>@brief Expression (EXPRESSION_MSB)</summary>
        Expression = 11, // 0xB

        EFFECTS1_MSB = 12, //0x0C,
        EFFECTS2_MSB = 13, //0x0D,

        GPC1_MSB = 16, //0x10, /* general purpose controller */
        GPC2_MSB = 17, //0x11,
        GPC3_MSB = 18, //0x12,
        GPC4_MSB = 19, // 0x13,

        /// <summary>@brief Bank Select LSB.\n
        /// MPTK bank style is FLUID_BANK_STYLE_GS (see fluidsynth), bank = CC0/MSB (CC32/LSB ignored)
        /// </summary>
        BankSelectLsb = 32, // 0x20

        MODULATION_WHEEL_LSB = 33, // 0x21,
        BREATH_LSB = 34, // 0x22,
        FOOT_LSB = 36, // 0x24,
        PORTAMENTO_TIME_LSB = 37, // 0x25,


        DATA_ENTRY_LSB = 38, // 0x26,

        VOLUME_LSB = 39, // 0x27,

        BALANCE_LSB = 40, // 0x28,

        PAN_LSB = 42, //0x2A,

        EXPRESSION_LSB = 43, //0x2B,

        EFFECTS1_LSB = 44, //0x2C,
        EFFECTS2_LSB = 45, // 0x2D,
        GPC1_LSB = 48, // 0x30,
        GPC2_LSB = 49, // 0x31,
        GPC3_LSB = 50, // 0x32,
        GPC4_LSB = 51, // 0x33,

        /// <summary>@brief Sustain (SUSTAIN_SWITCH)</summary>
        Sustain = 64, // 0x40

        /// <summary>@brief Portamento On/Off (PORTAMENTO_SWITCH) </summary>
        Portamento = 65, // 0x41

        /// <summary>@brief Sostenuto On/Off (SOSTENUTO_SWITCH)</summary>
        Sostenuto = 66, // 0x42

        /// <summary>@brief Soft Pedal On/Off (SOFT_PEDAL_SWITCH)</summary>
        SoftPedal = 67, // 0x43

        /// <summary>@brief Legato Footswitch (LEGATO_SWITCH)</summary>
        LegatoFootswitch = 68, // 0x44

        HOLD2_SWITCH = 69, // 0x45,

        SOUND_CTRL1 = 70, // 0x46,
        SOUND_CTRL2 = 71, // 0x47,
        SOUND_CTRL3 = 72, // 0x48,
        SOUND_CTRL4 = 73, // 0x49,
        SOUND_CTRL5 = 74, // 0x4A,
        SOUND_CTRL6 = 75, // 0x4B,
        SOUND_CTRL7 = 76, // 0x4C,
        SOUND_CTRL8 = 77, // 0x4D,
        SOUND_CTRL9 = 78, // 0x4E,
        SOUND_CTRL10 = 79, // 0x4F,

        GPC5 = 80, // 0x50,
        GPC6 = 81, // 0x51,
        GPC7 = 82, // 0x52,
        GPC8 = 83, // 0x53,

        PORTAMENTO_CTRL = 84, // 0x54, 

        EFFECTS_DEPTH1 = 91, // 0x5B,
        EFFECTS_DEPTH2 = 92, // 0x5C,
        EFFECTS_DEPTH3 = 93, // 0x5D,
        EFFECTS_DEPTH4 = 94, // 0x5E,
        EFFECTS_DEPTH5 = 95, // 0x5F,

        DATA_ENTRY_INCR = 96, // 0x60,
        DATA_ENTRY_DECR = 97, // 0x61,

        /// <summary>@brief
        /// Non Registered Parameter Number LSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        NRPN_LSB = 98, // 0x62,

        /// <summary>@brief
        /// Non Registered Parameter Number MSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        NRPN_MSB = 99, // 0x63,

        /// <summary>@brief
        /// Registered Parameter Number LSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        RPN_LSB = 100, // 0x64,

        /// <summary>@brief
        /// Registered Parameter Number MSB\n
        /// http://www.philrees.co.uk/nrpnq.htm
        /// </summary>
        RPN_MSB = 101, // 0x65,

        /// <summary@brief >All sound off (ALL_SOUND_OFF)</summary>
        AllSoundOff = 120, // 0x78,

        /// <summary>@brief Reset all controllers (ALL_CTRL_OFF)</summary>
        ResetAllControllers = 121, // 0x79

        LOCAL_CONTROL = 122, // 0x7A,

        /// <summary>@brief All notes off (ALL_NOTES_OFF)</summary>
        AllNotesOff = 123, // 0x7B

        OMNI_OFF = 124, // 0x7C,
        OMNI_ON = 125, // 0x7D,
        POLY_OFF = 126, // 0x7E,
        POLY_ON = 127, // 0x7F
    }


    /// <summary>@brief
    /// General MIDI RPN event numbers (LSB, MSB = 0)
    /// The only confusing part of using parameter numbers, initially, is that there are two parts to using them.\n
    /// First you need to tell the synthesizer what parameter you want to change, then you need to tell it how to change the parameter. \n
    /// For example, if you want to change the "pitch bend sensitivity" to 12 semitones, you would send the following controler midi message:\n
    ///     - MPTKEvent#Controller=RPN_MSB (101) MPTKEvent#Value=0
    ///     - MPTKEvent#Controller=RPN_LSB (100) MPTKEvent#Value=midi_rpn_event.RPN_PITCH_BEND_RANGE
    ///     - MPTKEvent#Controller=DATA_ENTRY_MSB (6) MPTKEvent#Value=12
    ///     - MPTKEvent#Controller=DATA_ENTRY_LSB (38) MPTKEvent#Value=0
    /// https://www.2writers.com/Eddie/TutNrpn.htm
    /// </summary>
    public enum midi_rpn_event
    {
        /// <summary>@brief
        /// Change pitch bend sensitivity
        /// </summary>
        RPN_PITCH_BEND_RANGE = 0x00,

        RPN_CHANNEL_FINE_TUNE = 0x01,
        RPN_CHANNEL_COARSE_TUNE = 0x02,
        RPN_TUNING_PROGRAM_CHANGE = 0x03,
        RPN_TUNING_BANK_SELECT = 0x04,
        RPN_MODULATION_DEPTH_RANGE = 0x05
    }

    /// <summary>@brief
    /// MIDI MetaEvent Type. Meta events are optionnals information that could be defined in a MIDI. None are mandatory\n
    /// In MPTKEvent the attibute MPTKEvent.Meta defined the type of meta event. 
    /// </summary>
    public enum MPTKMeta : byte
    {
        /// <summary>@brief Track sequence number</summary>
        TrackSequenceNumber = 0x00,
        /// <summary>@brief Text event</summary>
        TextEvent = 0x01,
        /// <summary>@brief Copyright</summary>
        Copyright = 0x02,
        /// <summary>@brief Sequence track name</summary>
        SequenceTrackName = 0x03,
        /// <summary>@brief Track instrument name</summary>
        TrackInstrumentName = 0x04,
        /// <summary>@brief Lyric</summary>
        Lyric = 0x05,
        /// <summary>@brief Marker</summary>
        Marker = 0x06,
        /// <summary>@brief Cue point</summary>
        CuePoint = 0x07,
        /// <summary>@brief Program (patch) name</summary>
        ProgramName = 0x08,
        /// <summary>@brief Device (port) name</summary>
        DeviceName = 0x09,
        /// <summary>@brief MIDI Channel (not official?)</summary>
        MidiChannel = 0x20,

        /// <summary>@brief MIDI Port (not official?)</summary>
        MidiPort = 0x21,

        /// <summary>@brief End track</summary>
        EndTrack = 0x2F,

        /// <summary>@brief Set tempo
        /// MPTKEvent.Value contains new Microseconds Per Beat Note.
        /// @deprecated version 2.10.0 - MPTKEvent.Duration no longer contains tempo in quarter per minute. Please investigate MPTKEvent.MPTK_MPQN2BPM()
        /// </summary>
        SetTempo = 0x51,

        /// <summary>@brief MPTE offset</summary>
        SmpteOffset = 0x54,

        /// <summary>@brief Time signature
        /// MPTKEvent.Value contains four bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt():
        ///    -# Numerator (number of beats in a bar), 
        ///    -# Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)
        ///    -# TicksInMetronomeClick, generally 24 (number of 1/32nds of a note happen by MIDI quarter note)
        ///    -# No32ndNotesInQuarterNote, generally 8 (standard MIDI clock ticks every 24 times every quarter note)
        /// @deprecated version 2.10.0 - MPTKEvent.Value no longer contains the numerator and MPTKEvent.Duration the Denominator (all values are merged in MPTKEvent::Value 
        /// </summary>
        TimeSignature = 0x58,

        /// <summary>@brief Key signature
        /// MPTKEvent.Value contains two bytes. From less significant to most significant. Please investigate MPTKEvent.ExtractFromInt().
        ///     -# SharpsFlats (number of sharp) 
        ///     -# MajorMinor flag (0 the scale is major, 1 the scale is minor).
        /// @deprecated version 2.10.0 - MPTKEvent.Duration no longer contains the MajorMinor.
        /// </summary>
        KeySignature = 0x59,

        /// <summary>@brief Sequencer specific</summary>
        SequencerSpecific = 0x7F,
    }

    /// <summary>
    /// Description of a MIDI Event. It's the heart of MPTK! Essential to handling MIDI by script from all others classes as MidiStreamPlayer, MidiFilePlayer, MidiFileLoader, MidiFileWriter2 ...\n
    /// 
    /// The MPTKEvent main property is MPTKEvent.Command, the content and role of other properties (as MPTKEvent.Value) depend on the value of MPTKEvent.Command. Look at the MPTKEvent.Value property.\n
    /// With this class, you can: play and stop a note, change instrument (preset, patch, ...), change some control as modulation (Pro) ...\n
    /// Use this class in relation with these classes:
    ///      - MidiFileLoader     to read MIDI events from a MIDI file.\n
    ///      - MidiFilePlayer     process MIDI events, thank to the class event OnEventNotesMidi when MIDI events are played from the internal MIDI sequencer.\n
    ///      - MidiFileWriter2    generate MIDI Music file from your own algorithm.\n
    ///      - MidiStreamPlayer   real-time generation of MIDI Music from your own algorithm.\n
    /// See here https://paxstellar.fr/class-mptkevent and here https://mptkapi.paxstellar.com/d9/d50/class_midi_player_t_k_1_1_m_p_t_k_event.html
    /// \n
    /// Also, below an example with MidiStreamPlayer\n
    /// @code
    /// 
    /// // Find a MidiStreamPlayer Prefab from the scene
    /// MidiStreamPlayer midiStreamPlayer = FindObjectOfType<MidiStreamPlayer>();
    /// midiStreamPlayer.MPTK_StartMidiStream();
    /// 
    /// // Change instrument to Marimba for channel 0
    /// MPTKEvent PatchChange = new MPTKEvent() {
    ///        Command = MPTKCommand.PatchChange,
    ///        Value = 12, // generally Marimba but depend on the SoundFont selected
    ///        Channel = 0 }; // Instrument are defined by channel (from 0 to 15). So at any time, only 16 différents instruments can be used simultaneously.
    /// midiStreamPlayer.MPTK_PlayEvent(PatchChange);    
    ///
    /// // Play a C4 during one second with the Marimba instrument
    /// MPTKEvent NotePlaying = new MPTKEvent() {
    ///        Command = MPTKCommand.NoteOn,
    ///        Value = 60, // play a C4 note
    ///        Channel = 0,
    ///        Duration = 1000, // one second
    ///        Velocity = 100 };
    /// midiStreamPlayer.MPTK_PlayEvent(NotePlaying);    
    /// @endcode
    /// </summary>
    public partial class MPTKEvent : ICloneable
    {
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>@brief
        /// Track index of the event in the midi. \n
        /// There is any impact on the music played. \n
        /// It's just a cool way to regroup MIDI events in a ... track like in a sequencer.\n
        /// Track 0 is the first track read from the midi file.
        /// </summary>
        public long Track;

        /// <summary>@brief
        /// Time in Midi Tick (part of a Beat) of the Event since the start of playing the midi file.\n
        /// This time is independent of the Tempo or Speed. Not used for MidiStreamPlayer nor MidiInReader because they are real-time player.
        /// </summary>
        public long Tick;

        /// <summary>@brief
        /// Measure (bar) when this event will be played. Measure is calculated with the Time Signature event when a MIDI file is loaded.\n
        /// By default the time signature is 4/4.
        /// @version 2.10.0
        /// </summary>
        public int Measure;

        /// <summary>@brief
        /// Beat in measure of this event (have sense only for noteon). Measure is calculated with the Time Signature event when a MIDI file is loaded.\n
        /// By default the time signature is 4/4.
        /// @version 2.10.0
        /// </summary>
        public int Beat;

        // removed with V2.9.0
        // public long DeltaTick;

        /// <summary>@brief
        /// Initial Event Index in the MIDI list, set only when a MIDI file is loaded.
        /// </summary>
        public int Index;

        /// <summary>@brief
        /// V2.9.0 Time from System.DateTime when the Event has been created (in the constructor of this class). Divide by 10000 to get milliseconds.\n
        /// Replace TickTime from previous version (was confusing).
        /// </summary>
        public long CreateTime;

        /// <summary>@brief
        /// Real time in milliseconds of this event from the start of the MIDI, take into account the tempo changes but not MPTK_Speed of the MidiPlayer.\n
        /// v2.89.6 Correct the time shift when a tempo change is read. Thanks to Ken Scott http://www.youtube.com/vjchaotic for the tip.\n
        /// Not used for MidiStreamPlayer nor MidiInReader (events played are always in real-time from your app).
        /// </summary>
        public float RealTime;

        /// <summary>@brief
        /// Midi Command code. Defined the type of message. See #MPTKCommand (Note On, Control Change, Patch Change...)
        /// </summary>
        public MPTKCommand Command;

        /// <summary>@brief
        /// Controller code. When the #Command is ControlChange, contains the code fo the controller to change (Modulation, Pan, Bank Select ...).\n
        /// #Value properties will contains the value of the controller. See #MPTKController.
        /// </summary>
        public MPTKController Controller;

        /// <summary>@brief
        /// MetaEvent Code. When the #Command is MetaEvent, contains the code of the meta event (Lyric, TimeSignature, ...).\n
        /// Others properties will contains the value of the meta. See #MPTKMeta (TextEvent, Lyric, TimeSignature, ...).\n
        /// </summary>
        public MPTKMeta Meta;

        /// <summary>@brief
        /// Information hold by textual meta event when #Command = MetaEvent
        /// </summary>
        public string Info;

        /// <summary>@brief
        /// Contains a value in relation with the #Command.
        ///! <ul>
        ///! <li>#Command = #MPTKCommand.NoteOn
        ///!     <ul>
        ///!       <li> #Value contains midi note as defined in the MIDI standard and matched to the Middle C (note number 60) as C4.\n
        ///!         look here: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html#BMA1_3
        ///        </li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.ControlChange
        ///!     <ul>
        ///!       <li> #Value contains controller value, see #MPTKController</li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.PatchChange
        ///!     <ul>
        ///!        <li>  #Value contains patch/preset/instrument value. See the current SoundFont to find value associated to each instrument.\n
        ///!                If your SoundFont follows the General Midi (GM) map, instrument Patch map will be like ths one:\n
        ///                 http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html#BMA1_4    
        ///!        </li>
        ///!     </ul>
        ///! </li>
        ///! <li>#Command = #MPTKCommand.MetaEvent and #Meta equal:
        ///!     <ul>
        ///!        <li>  #MPTKMeta.SetTempo</li>
        ///!        <ul>
        ///!            <li>  #Value contains new Microseconds Per Beat Note</li>
        ///!        </ul>
        ///!        <li>  #MPTKMeta.TimeSignature. See #MPTKMeta.TimeSignature</li>
        ///!        <li>  #MPTKMeta.KeySignature. See #MPTKMeta.KeySignature</li>
        ///!     </ul>
        ///! </li>
        ///! </ul>
        /// </summary>
        public int Value;

        /// <summary>@brief
        /// Midi channel fom 0 to 15 (9 for drum)
        /// </summary>
        public int Channel;

        /// <summary>@brief
        /// Velocity between 0 and 127. Used only if #Command equal NoteOn.
        /// </summary>
        public int Velocity;

        /// <summary>@brief
        /// Contains duration of the note in millisecond. Set -1 for a noteon played indefinitely.
        /// @Version 2.10.0 
        /// Removed these roles\n
        ///    SetTempo: no longer contains new tempo (quarter per minute)\n
        ///    TimeSignature: no longer contains the Denominator (Beat unit: 1 means 2, 2 means 4 (crochet), 3 means 8 (quaver), 4 means 16, ...)\n
        ///    KeySignature: no longer contains the MajorMinor flag.\n
        /// </summary>
        public long Duration;

        public long durationTicks { get { Debug.LogWarning("durationTicks is deprecated. It was a duplicate of the attribute Length, always return 0 in v2.9.0"); return 0; } }

        /// <summary>@brief
        /// Get the duration of the event in ticks (length note in ticks).
        /// @deprecated With v2.9.0 was a duplicate of attribute #Length. 
        /// </summary>
        public long DurationTicks { get { Debug.LogWarning("DurationTicks is deprecated. It was a duplicate of the attribute Length, always return 0 in v2.9.0"); return 0; } }

        /// <summary>@brief
        /// Short delay before playing the note in millisecond. Available only in Core mode.
        /// Apply only on NoteOn event.
        /// </summary>
        public long Delay;

        /// <summary>@brief
        /// Duration of the note in MIDI Tick. 
        /// @details
        /// Duration in ticks is converted in duration in millisecond see (#Duration) when the MIDI file is loaded.
        /// @note
        /// Maestro does not use this value for playing a MIDI file but the #Duration in millisecond.
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public int Length;

        /// <summary>@brief
        /// Note length as https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public enum EnumLength { Whole, Half, Quarter, Eighth, Sixteenth }

        /// <summary>@brief
        /// Origin of the message. Midi ID if from Midi Input else zero. V2.83: rename source to Source et set public.
        /// </summary>
        public uint Source;

        /// <summary>@brief
        /// Associate an Id with this event.\n
        /// When reading a Midi file with MidiFilePlayer: this Id is unique for all Midi events played for this Midi.\n
        /// Consequently, when switching Midi, MPTK_ClearAllSound is able to clear (note-off) only the voices associated with this Midi file.\n
        /// Switching between Midi playing is very quick.\n
        /// Could also be used for other prefab as MidiStreamPlayer for your specific need, but don't change this properties with MidiFilePlayer.
        /// </summary>
        public int IdSession;


        /// <summary>@brief
        /// V2.87 Tag information for application purpose
        /// </summary>
        public object Tag;

        /// <summary>@brief
        /// List of voices associated to this Event for playing a NoteOn event.
        /// </summary>
        public List<fluid_voice> Voices;

        /// <summary>@brief
        /// Check if playing of this midi event is over (all voices are OFF)
        /// </summary>
        public bool IsOver
        {
            get
            {
                if (Voices != null)
                {
                    foreach (fluid_voice voice in Voices)
                        if (voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                            return false;
                }
                // All voices are off or empty
                return true;
            }
        }

        public MPTKEvent()
        {
            Command = MPTKCommand.NoteOn;
            // V2.82 set default value
            Duration = -1;
            Channel = 0;
            Delay = 0;
            Velocity = 127; // max
            IdSession = -1;
            CreateTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>@brief
        /// V2.9.0 Delta time in system time (calculated with DateTime.UtcNow.Ticks) since the creation of this event.\n
        /// Mainly useful to evaluate MPTK latency. One system ticks equal 100 nano second.\n
        /// @note Disabled by default. Defined DEBUG_PERF_AUDIO in MidiSynth to activate for debug purpose only.
        /// </summary>
        public long MPTK_LatenceTime { get { return DateTime.UtcNow.Ticks - CreateTime; } }

        /// <summary>@brief
        /// V2.9.0 Delta time in milliseconds (calculated with DateTime.UtcNow.Ticks) since the creation of this event.\n
        /// Mainly useful to evaluate MPTK latency. One system ticks equal 100 nano second.\n
        /// @note Disabled by default. Defined DEBUG_PERF_AUDIO in MidiSynth to activate for debug purpose only.
        /// </summary>
        public long MPTK_LatenceTimeMillis { get { return MPTK_LatenceTime / fluid_voice.Nano100ToMilli; } }

        /// <summary>@brief
        /// Create a MPTK Midi event from a midi input message
        /// </summary>
        /// <param name="data"></param>
        public MPTKEvent(ulong data)
        {
            Source = (uint)(data & 0xffffffffUL);
            Command = (MPTKCommand)((data >> 32) & 0xFF);
            if (Command < MPTKCommand.Sysex)
            {
                Channel = (int)Command & 0xF;
                Command = (MPTKCommand)((int)Command & 0xF0);
            }
            byte data1 = (byte)((data >> 40) & 0xff);
            byte data2 = (byte)((data >> 48) & 0xff);

            if (Command == MPTKCommand.NoteOn && data2 == 0)
                Command = MPTKCommand.NoteOff;

            //if ((int)Command != 0xFE)
            //    Debug.Log($"{data >> 32:X}");

            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    Value = data1; // Key
                    Velocity = data2;
                    Duration = -1; // no duration are defined in Midi flux
                    break;
                case MPTKCommand.NoteOff:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    Value = data1; // Key
                    Velocity = data2;
                    break;
                case MPTKCommand.ControlChange:
                    Controller = (MPTKController)data1;
                    Value = data2;
                    break;
                case MPTKCommand.PatchChange:
                    Value = data1;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    Value = data1;
                    break;
                case MPTKCommand.PitchWheelChange:
                    Value = data2 << 7 | data1; // Pitch-bend is transmitted with 14-bit precision. 
                    break;
                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    // no value
                    break;

            }
        }

        /// <summary>@brief
        /// Build a packet midi message from a MPTKEvent. Example:  0x00403C90 for a noton (90h, 3Ch note,  40h volume)
        /// </summary>
        /// <returns></returns>
        public ulong ToData()
        {
            ulong data = (ulong)Command | ((ulong)Channel & 0xF);
            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.NoteOff:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.KeyAfterTouch:
                    data |= (ulong)Value << 8 | (ulong)Velocity << 16;
                    break;
                case MPTKCommand.ControlChange:
                    data |= (ulong)Controller << 8 | (ulong)Value << 16;
                    break;
                case MPTKCommand.PatchChange:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    data |= (ulong)Value << 8;
                    break;
                case MPTKCommand.PitchWheelChange:
                    // The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 2000H. 
                    // Two data after the command code 
                    //  1) the least significant 7 bits. 
                    //  2) the most significant 7 bits.
                    data |= ((ulong)Value & 0x7F) << 8 | ((ulong)Value & 0x7F00) << 16;
                    break;
                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    data = (ulong)Command;
                    break;

            }
            return data;
        }

        /// <summary>@brief
        /// Convert bmp to duration of a quarter in microsecond.\n
        /// With BPM=1,   microsecondsPerQuaterNote=60 000 000 µs ->  60 secondes per quarter (quite slow!)\n
        /// With BPM=120, microsecondsPerQuaterNote=500 000 µs -> 0.5 seconde per quarter\n
        /// </summary>
        /// <param name="bpm">Beats Per Minute (with assumption beat=quarter)</param>
        /// <returns>60000000 / bpm or 500000 if bpm <= 0</returns>
        public static int MPTK_BPM2MPQN(double bpm)
        {
            return bpm > 0 ? (int)(60000000d / bpm) : 500000;
        }

        /// <summary>@brief
        /// Convert duration of a quarter in microsecond to Beats Per Minute (with assumption beat=quarter).\n
        /// With microsecondsPerQuaterNote=500 000 µs, BPM = 120
        /// </summary>
        /// <param name="microsecondsPerQuaterNote"></param>
        /// <returns>60000000 / bpm or 120 if microsecondsPerQuaterNote <= 0</returns>
        public static double MPTK_MPQN2BPM(int microsecondsPerQuaterNote)
        {
            return microsecondsPerQuaterNote > 0 ? 60000000 / microsecondsPerQuaterNote : 120;
        }

        /// <summary>
        /// Store four bytes into one integer.
        /// </summary>
        /// <param name="b1">byte 0 - less significant</param>
        /// <param name="b2">byte 1 </param>
        /// <param name="b3">byte 2 </param>
        /// <param name="b4">byte 3 - moss significant</param>
        /// <returns>(b4 << 24) | (b3 << 16) | (b2 << 8) | b1</returns>
        static public int BuildIntFromBytes(byte b1, byte b2, byte b3, byte b4)
        {
            return (b4 << 24) | (b3 << 16) | (b2 << 8) | b1;
        }

        /// <summary>
        /// Extract byte position 'n' from an integer 
        /// </summary>
        /// <param name="v">value build with #BuildIntFromBytes</param>
        /// <param name="n">byte position from 0 (less significant) to 3 (most significant)</param>
        /// <returns>(v >> (8*n)) & 0xFF</returns>
        static public byte ExtractFromInt(uint v, int n)
        {
            return (byte)((v >> (8 * n)) & 0xFF);
        }

        /// <summary>
        /// Extract value 2 from a double int build with #BuildIntFromBytes
        /// </summary>
        /// <param name="v">value build with #BuildIntFromBytes</param>
        /// <returns>v - (v / 100) * 100</returns>
        //static public int ExtractDoubleValue2(int v)
        //{
        //    return v - (v / 100) * 100;
        //}

        /// <summary>@brief
        /// Build a string description of the Midi event. V2.83 removes "end of lines" on each returns string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            string position = $"T:{Track,-2}\t";
            if (Command == MPTKCommand.NoteOn || Command == MPTKCommand.NoteOff || Command == MPTKCommand.KeyAfterTouch || Command == MPTKCommand.ControlChange ||
                Command == MPTKCommand.PatchChange || Command == MPTKCommand.ChannelAfterTouch || Command == MPTKCommand.PitchWheelChange)
                position += $"C:{Channel,-2}\t";
            else
                position += "\t";
            position += $"Tick:{Tick,-7:000000}\tMeasure:{Measure}\tBeat:{Beat}\tRealTime:{RealTime / 1000f:F2}\t\t";

            switch (Command)
            {
                case MPTKCommand.NoteOn:
                    result = "NoteOn\t" + position;
                    string sDuration = Duration < 0 ? "Inf.    " : $"{Duration / 1000f:F2} s. Lenght:{Length} tick";
                    result += $"Note:{Value,-3} Velocity:{Velocity} Duration:{sDuration}";
                    break;
                case MPTKCommand.NoteOff:
                    result = "NoteOff\t" + position;
                    result += $"Note:{Value,-3} Velocity:{Velocity}";
                    break;
                case MPTKCommand.PatchChange:
                    result = "Preset \t" + position;
                    result += $"Value:{Value,-3}";
                    break;
                case MPTKCommand.ControlChange:
                    result = "Control\t" + position;
                    result += $"Value:{Value,-3} Controller:{Controller}";
                    break;
                case MPTKCommand.KeyAfterTouch:
                    result = "KeyAft \t" + position;
                    result += $"Not processed by Maestro Synth";
                    break;
                case MPTKCommand.ChannelAfterTouch:
                    result = "ChaAft \t" + position;
                    result += $"Not processed by Maestro Synth";
                    break;
                case MPTKCommand.PitchWheelChange:
                    result = "Pitch \t" + position;
                    result += $"Value:{Value,-3}";
                    break;
                case MPTKCommand.MetaEvent:
                    try
                    {
                        switch (Meta)
                        {
                            case MPTKMeta.KeySignature: result = $"KeySig\t{position} SharpsFlats:{MPTKEvent.ExtractFromInt((uint)Value,0)} MajorMinor:{MPTKEvent.ExtractFromInt((uint)Value,1)}"; break;
                            case MPTKMeta.TimeSignature: result = $"TimeSig\t{position} Numerator:{MPTKEvent.ExtractFromInt((uint)Value,0)} Denominator:{MPTKEvent.ExtractFromInt((uint)Value, 1)}"; break;
                            case MPTKMeta.SetTempo: result = $"Tempo\t{position} Microseconds:{Value} Tempo:{60000000 / Value:F2}"; break;
                            default:
                                string sinfo = Info ?? "";
                                result = $"MetaEvt\t{position} {Meta} {sinfo}"; break;
                        }
                    }
                    catch { result = $"{Meta} error value:{Value}"; }
                    break;

                case MPTKCommand.TimingClock:
                case MPTKCommand.StartSequence:
                case MPTKCommand.ContinueSequence:
                case MPTKCommand.StopSequence:
                case MPTKCommand.AutoSensing:
                    result += $"Command:{Command}";
                    break;
                default:
                    result += $"Command:{Command} Value:{Value} Duration:{Duration,6} Velocity:{Velocity,3} source:{Source}";
                    break;
            }
            return result;
        }
    }
}
