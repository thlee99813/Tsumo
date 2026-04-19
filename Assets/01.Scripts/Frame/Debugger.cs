using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
public class Debugger : MonoBehaviour
{
    public CinemachineCamera[] cams;
    public int activePriority = 20;
    public int inactivePriority = 0;

    public bool IsSteering = false;

    [Header("Debug Target")]
    public BattleImpulseEmitter ImpurseSource;
    public GameObject Prefab;

    public int CurrentCameraIndex { get; private set; }

    private static readonly Key[] _presetKeys =
    {
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9
    };

    [Header("Hand Cheat")]
    [SerializeField] private DeckController _deckController;
    [SerializeField] private List<HandCheatPreset> _handCheatPresets = new List<HandCheatPreset>(9);

    private void Update()
    {
        HandleHandCheatKeys();

        if (Keyboard.current.yKey.wasPressedThisFrame) 
        {
            //Boat.GetComponent<Boat>().GameStart = true;
        }
        else if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            
        }
        else if (Keyboard.current.digit1Key.wasPressedThisFrame) ActivateCamera(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) ActivateCamera(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) ActivateCamera(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) ActivateCamera(3);
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) ImpurseSource.EmitHitImpulse();
    }
    private void HandleHandCheatKeys()
    {
        bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed;
        if (!isShiftPressed)
        {
            return;
        }

        for (int i = 0; i < _presetKeys.Length; i++)
        {
            if (!Keyboard.current[_presetKeys[i]].wasPressedThisFrame)
            {
                continue;
            }

            ApplyHandCheatPreset(i);
            break;
        }
    }

    private void ApplyHandCheatPreset(int presetIndex)
    {
        if (presetIndex >= _handCheatPresets.Count)
        {
            return;
        }

        HandCheatPreset preset = _handCheatPresets[presetIndex];
        List<CardData> targetCards = new List<CardData>();

        for (int i = 0; i < preset.Cards.Count; i++)
        {
            CheatCardSpec spec = preset.Cards[i];
            if (!_deckController.TryGetCardDefinition(spec.Type, spec.Number, out CardData cardData))
            {
                continue;
            }

            targetCards.Add(cardData);
        }

        _deckController.ReplaceHandForDebug(targetCards);
    }


    public void ActivateCamera(int index)
    {
        if(index >= cams.Length) return;
        CurrentCameraIndex = index;

        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].Priority = (i == index) ? activePriority : inactivePriority;
        }
    }

    [Serializable]
    private class CheatCardSpec
    {
        [SerializeField] private CardType _type;
        [SerializeField] private int _number = 1;

        public CardType Type => _type;
        public int Number => _number;
    }

    [Serializable]
    private class HandCheatPreset
    {
        [SerializeField] private List<CheatCardSpec> _cards = new List<CheatCardSpec>();
        public List<CheatCardSpec> Cards => _cards;
    }
    



}
