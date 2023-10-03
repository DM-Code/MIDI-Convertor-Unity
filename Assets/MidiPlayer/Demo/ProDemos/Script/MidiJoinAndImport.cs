using MidiPlayerTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DemoMPTKImport
{
    public class MidiJoinAndImport : MonoBehaviour
    {
        public Text TextMidiTime;
        public Text TextInfoMidi;
        public Slider SliderPosition;
        public InputField InputPosition;
        public Text TextPosition;
        public Toggle MidiPause;

        private PopupListItem PopMidi;
        private MidiFileLoader mfLoader;
        private MidiFilePlayer mfPlayer;
        private MidiFileWriter2 mfWriter;
        private int indexSelectedMidi;
        private CustomStyle myStyle;

        void Start()
        {
            mfPlayer = FindObjectOfType<MidiFilePlayer>();
            if (mfPlayer == null) { Debug.LogError("No MidiFilePlayer prefab found in the hierarchy"); return; };
            mfPlayer.OnEventNotesMidi.AddListener((List<MPTKEvent> events) =>
            {
                /// Called for each MIDI event (or group of MIDI events) ready to be played by the MIDI synth. All these events are on same MIDI tick.
                UpdatePosition();
            });

            mfLoader = FindObjectOfType<MidiFileLoader>();
            if (mfLoader == null) Debug.LogError("No MidiFileLoader prefab found in the hierarchy");

            PopMidi = new PopupListItem()
            {
                Title = "Popup MIDI",
                OnSelect = CallbackPopupSelectMidi,
                Tag = "",
                KeepOpen = false,
                ColCount = 2,
                ColWidth = 250,
            };

            MidiPause.onValueChanged.AddListener((bool pause) =>
            {
                if (pause)
                    mfPlayer.MPTK_Pause();
                else
                    mfPlayer.MPTK_UnPause();
            });
        }

        /// <summary>
        /// Call when a MIDI is selected in the popup.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="index"></param>
        /// <param name="indexList"></param>
        private void CallbackPopupSelectMidi(object tag, int index, int indexList)
        {
            //Debug.Log("CallbackPopup " + index + " for " + tag);
            indexSelectedMidi = index;
            mfLoader.MPTK_MidiIndex = indexSelectedMidi;
            mfLoader.MPTK_Load();
            TextInfoMidi.text = $"MIDI Selected {mfLoader.MPTK_MidiIndex} - '{mfLoader.MPTK_MidiName}'\nCount Event={mfLoader.MPTK_MidiEvents.Count}    MPTK_DeltaTicksPerQuarterNote={mfLoader.MPTK_DeltaTicksPerQuarterNote}";
        }

        /// <summary>
        /// Display the popup to select a MIDI. Triggered by a button on the UI.
        /// </summary>
        /// <param name="tag"></param>
        public void SelectMIDI(string tag)
        {
            PopMidi.Tag = tag;
            PopMidi.Show = true;
        }
        //! [ExampleMIDIImportAndPlay]

        /// <summary>
        /// Insert the midi selected . Triggered by a button on the UI.
        /// </summary>
        public void InsertMidi()
        {
            if (mfWriter == null)
                Debug.LogWarning("First, create a MidiFileWriter instance");
            else
            {
                // Load the MIDI file selected
                mfLoader.MPTK_MidiIndex = indexSelectedMidi;
                Debug.Log($"MPTK_KeepNoteOff:{mfLoader.MPTK_KeepNoteOff} MPTK_KeepEndTrack:{mfLoader.MPTK_KeepEndTrack} ");
                mfLoader.MPTK_Load();

                // From the UI, get the tick position where to insert
                if (string.IsNullOrWhiteSpace(InputPosition.text)) InputPosition.text = "0";
                long position = Convert.ToInt64(InputPosition.text);
                if (position < 0) { InputPosition.text = "-1"; position = -1; }

                // Insert the MPTK_MidiEvents of the MIDI loaded in the MPTK_MidiEvents of the MIDI Writer. 
                mfWriter.MPTK_ImportFromEventsList(mfLoader.MPTK_MidiEvents, mfLoader.MPTK_DeltaTicksPerQuarterNote, position: position, name: "MidiJoined", logPerf: true, logDebug: true); ;

                // If a MIDI player is already playing then update duration.
                // Playing of a MIDI Writer is done with: mfPlayer.MPTK_Play(mfWriter)
                if (mfPlayer != null && mfPlayer.MPTK_IsPlaying)
                    mfPlayer.MPTK_MidiLoaded.MPTK_ComputeDuration();

                Debug.Log($"{mfLoader.MPTK_MidiName} Loaded {mfLoader.MPTK_MidiEvents.Count} events added, total events: {mfWriter.MPTK_MidiEvents.Count}");
                UpdatePosition();
            }
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void PlayMidi()
        {
            if (mfWriter != null && mfWriter.MPTK_MidiEvents != null)
            {
                // The MIDI file player will play the MIDI events list found in the MIDI Writer.
                // In fact the mfPlayer.MPTK_MidiEvents will directly use the mfWriter.MPTK_MidiEvents for playing the MIDI.
                // All changes done on the mfWriter.MPTK_MidiEvents have direct impact on the player.
                // When playing, it's possible to add, modify or insert MIDI events in mfWriter.MPTK_MidiEvents,
                // changes will be automatically taken into account by the player if the change position if after the current tick player (mfPlayer.MPTK_TickPlayer) .
                // Inserting before mfPlayer.MPTK_TickPlayer could perturbate the player. 
                mfPlayer.MPTK_Play(mfWriter);
            }
            else
                Debug.LogWarning("No MidiWriter ready for playing");
        }
        //! [ExampleMIDIImportAndPlay]

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void Clear()
        {
            StopMidi();
            if (mfPlayer.MPTK_MidiLoaded != null)
                mfPlayer.MPTK_MidiLoaded.MPTK_Clear();
            mfWriter = new MidiFileWriter2(deltaTicksPerQuarterNote: 250);
            mfWriter.MPTK_AddText(0, 0, MPTKMeta.TextEvent, "Init of the sequence");
            Debug.Log("MidiFileWriter2 created");
            //mfWriter.MPTK_CreateTracksStat();
            mfWriter.MPTK_Debug();
            UpdatePosition();
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void StopMidi()
        {
            mfPlayer.MPTK_Stop();
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void WriteMidiFile()
        {
            if (mfWriter != null && mfWriter.MPTK_MidiEvents != null)
            {
                // Write the MIDI file for using with another player
                string filename = Path.Combine(Application.persistentDataPath, mfWriter.MPTK_MidiName + ".mid");
                Debug.Log("Write MIDI file:" + filename);
                mfWriter.MPTK_StableSortEvents();
                mfWriter.MPTK_WriteToFile(filename);
            }
            else
                Debug.LogWarning("No MidiWriter ready for writing a MIDI file");
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void WriteMidiDB()
        {
            if (mfWriter != null && mfWriter.MPTK_MidiEvents != null)
            {
                // Write the MIDI DB for using with another player
                mfWriter.MPTK_WriteToMidiDB("@MidiJoin");
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            else
                Debug.LogWarning("No MidiWriter ready for writing a MIDI file");
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void LogContent()
        {
            if (mfWriter == null)
                Debug.LogWarning("First, create a MidiFileWriter instance");
            else
            {
                mfWriter.MPTK_CreateTracksStat();
                mfWriter.MPTK_Debug();
            }
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void Quit()
        {
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                //Debug.Log(SceneUtility.GetScenePathByBuildIndex(i));
                if (SceneUtility.GetScenePathByBuildIndex(i).Contains("ScenesDemonstration"))
                {
                    SceneManager.LoadScene(i, LoadSceneMode.Single);
                    return;
                }
            }

            Application.Quit();
        }

        /// <summary>
        /// Triggered by a button on the UI.
        /// </summary>
        public void GotoWeb(string uri)
        {
            Application.OpenURL(uri);
        }

        /// <summary>
        /// Called for each MIDI event (or group of MIDI events) ready to be played by the MIDI synth. All these events are on same MIDI tick.
        /// </summary>
        public void UpdatePosition()
        {
            if (mfPlayer == null)
                TextPosition.text = "No MIDI Player found, check that a MidiFilePlayer is available into the scene hierarchy";
            else if (mfWriter.MPTK_MidiEvents == null || mfWriter.MPTK_MidiEvents.Count == 0)
            {
                TextPosition.text = $"No MIDI events available in the MidiFileWriter2 - ";
                TextPosition.text += "Click on Play to load the MidiFilePLayer with events";
            }
            else
            {
                if (mfPlayer.MPTK_IsPlaying)
                {
                    int count = mfPlayer.MPTK_MidiEvents.Count;
                    TextPosition.text = $"MIDI Player events: {count} - Time: {mfPlayer.MPTK_Position / 1000d:F2} / {mfPlayer.MPTK_DurationMS / 1000f:F2} second - Ticks:{mfPlayer.MPTK_TickCurrent} / {mfPlayer.MPTK_TickLast}";
                }
                else
                {
                    int count = mfWriter.MPTK_MidiEvents.Count;
                    TextPosition.text = $"Writer - DeltaTicksPerQuarterNote={mfWriter.MPTK_DeltaTicksPerQuarterNote} -  MIDI events={count}";
                }
            }
            SliderPosition.minValue = 0f;
            SliderPosition.maxValue = mfPlayer.MPTK_DurationMS;
            SliderPosition.value = (float)mfPlayer.MPTK_Position;
        }

        void OnGUI()
        {
            // Set custom Style. Good for background color 3E619800
            if (myStyle == null)
                myStyle = new CustomStyle();

            PopMidi.Draw(MidiPlayerGlobal.MPTK_ListMidi, indexSelectedMidi, myStyle);
        }

        // Update is called once per frame
        void Update()
        {
            if (mfPlayer != null && mfPlayer.MPTK_MidiLoaded != null)
                TextMidiTime.text = $"Real-time MIDI sequencer position: {mfPlayer.MPTK_MidiLoaded.MPTK_TickPlayer}";
        }
    }
}