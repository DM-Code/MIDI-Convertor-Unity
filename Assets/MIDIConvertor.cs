using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MIDIConvertor : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiFileLoader midiFileLoader;

    // Start is called before the first frame update
    void Start()
    {
        midiFilePlayer.MPTK_PlayOnStart = false;

        // Emulating button press
        LoadMidi();
        ConvertToSongFile();
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
