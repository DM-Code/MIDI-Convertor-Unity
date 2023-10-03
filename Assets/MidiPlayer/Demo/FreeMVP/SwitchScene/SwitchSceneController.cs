using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoMVPSwitchScene
{

    public class SwitchSceneController : MonoBehaviour
    {

        private void Awake()
        {
            Debug.Log("Awake: SwitchSceneController");
        }

        // Start is called before the first frame update
        void Start()
        {
        }
        public void LoadSceneChild()
        {
            Debug.Log("LoadSceneChild");
            Debug.LogWarning("Possible some 'glitch' with the MIDI Player when switching from the Unity Editor run.");
            Debug.Log("Nothing from a built application.");
            SceneManager.LoadScene("SwitchSceneChild", LoadSceneMode.Single);
        }

        public void LoadSceneHome()
        {
            Debug.Log("LoadSceneHome");
            Debug.LogWarning("Possible some 'glitch' with the MIDI Player when switching from the Unity Editor run.");
            Debug.Log("Nothing from a built application.");
            SceneManager.LoadScene("SwitchScene", LoadSceneMode.Single);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}