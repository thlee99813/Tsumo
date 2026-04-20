using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YakuEpicMomentController : MonoBehaviour
{
    [Serializable]
    private class YakuMomentEntry
    {
        [SerializeField] private string _yakuName;
        [SerializeField] private GameObject _momentObject;
        [SerializeField] private float _showSeconds = 0.35f;

        public string YakuName => _yakuName;
        public GameObject MomentObject => _momentObject;
        public float ShowSeconds => _showSeconds;
    }

    [SerializeField] private List<YakuMomentEntry> _entries = new List<YakuMomentEntry>();
    [SerializeField] private bool _playAllMatched = false;
    [SerializeField] private bool _hideAllOnAwake = true;

    private void Awake()
    {
        if (_hideAllOnAwake)
        {
            HideAll();
        }
    }

    public IEnumerator PlayMatched(FireScoreResult scoreResult)
    {
        if (scoreResult == null || scoreResult.YakuResults == null || scoreResult.YakuResults.Count == 0)
        {
            yield break;
        }

        HashSet<string> matchedNames = new HashSet<string>();
        for (int i = 0; i < scoreResult.YakuResults.Count; i++)
        {
            matchedNames.Add(scoreResult.YakuResults[i].Name);
        }

        for (int i = 0; i < _entries.Count; i++)
        {
            YakuMomentEntry entry = _entries[i];
            if (!matchedNames.Contains(entry.YakuName))
            {
                continue;
            }

            entry.MomentObject.SetActive(true);
            yield return new WaitForSecondsRealtime(entry.ShowSeconds);
            entry.MomentObject.SetActive(false);

            if (!_playAllMatched)
            {
                yield break;
            }
        }
    }

    public void HideAll()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].MomentObject != null)
            {
                _entries[i].MomentObject.SetActive(false);
            }
        }
    }
}
