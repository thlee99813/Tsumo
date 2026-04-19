using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleResultPresenter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIController _uiController;
    [SerializeField] private TMP_Text _baseComboTitleText;
    [SerializeField] private TMP_Text _baseComboScoreText;
    [SerializeField] private TMP_Text _advanceComboScoreText;
    [SerializeField] private TMP_Text[] _yakuNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] _yakuBonusTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text _finalScoreMultiplyText;
    [SerializeField] private TMP_Text _finalDamageText;
    [SerializeField] private TMP_Text _enemyRemainHpTitleText; 
    [SerializeField] private TMP_Text _enemyRemainHpText;
    [SerializeField] private TMP_Text _enemyDamageTitleText;

    [SerializeField] private TMP_Text _enemyDamageToPlayerText;
    [SerializeField] private TMP_Text _playerRemainHpTitleText;
    [SerializeField] private TMP_Text _playerRemainHpText;


    [Header("Battle Result Time")]
    [SerializeField, Min(0f)] private float _resultPauseDelay = 0.2f;
    [SerializeField, Min(0f)] private float _charTypeInterval = 0.04f;
    [SerializeField, Min(0f)] private float _countUpDuration = 0.35f;
    [SerializeField, Min(0f)] private float _beforeDamagePreviewSeconds = 0.2f;

    [SerializeField, Min(0f)] private float _resultPanelHoldSeconds = 1.5f;

    public IEnumerator PlayBattleResultPanel(
    FireExecutionData fireExecutionData,
    int enemyHpBeforeBattle,
    int enemyRemainHp,
    int playerHpBeforeBattle,
    int enemyDamageToPlayer,
    int playerRemainHp)
    {
        yield return new WaitForSecondsRealtime(_resultPauseDelay);

        float prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _uiController.ShowBattleResultPanel();
        ClearAllTexts();

        FireScoreResult scoreResult = fireExecutionData != null ? fireExecutionData.ScoreResult : null;
        int finalDamage = fireExecutionData != null ? Mathf.Max(0, fireExecutionData.FinalDamage) : 0;
        int finalPercent = scoreResult != null ? Mathf.RoundToInt(scoreResult.FinalMultiplier * 100f) : 100;

        GetComboSummary(scoreResult, out int sequenceCount, out int tripleCount, out int sequenceScore, out int tripleScore);

        yield return TypeText(_baseComboTitleText, "콤보데미지");
        yield return TypeText(_baseComboScoreText, $"{sequenceScore} * {sequenceCount}");
        yield return TypeText(_advanceComboScoreText, $"{tripleScore} * {tripleCount}");

        int currentPercent = 100;
        int currentDamage = 0;
        int currentEnemyHp = Mathf.Max(0, enemyHpBeforeBattle);
        int currentPlayerHp = Mathf.Max(0, playerHpBeforeBattle);

        yield return TypeText(_finalScoreMultiplyText, $"* {currentPercent}%");
        yield return TypeText(_finalDamageText, currentDamage.ToString());
        yield return TypeText(_enemyRemainHpTitleText, "적 체력 :");
        yield return TypeText(_enemyRemainHpText, currentEnemyHp.ToString());
        yield return TypeText(_playerRemainHpText, currentPlayerHp.ToString());


        yield return new WaitForSecondsRealtime(_beforeDamagePreviewSeconds);


        int lineCount = Mathf.Min(4, Mathf.Min(_yakuNameTexts.Length, _yakuBonusTexts.Length));
        List<YakuScoreResult> yakus = BuildDisplayYakus(scoreResult);
        int displayYakus = Mathf.Min(lineCount, yakus.Count);

        float cumulativeBonus = 0f;
        int baseScoreTotal = scoreResult != null ? scoreResult.BaseScoreTotal : 0;

        for (int i = 0; i < displayYakus; i++)
        {
            YakuScoreResult yaku = yakus[i];
            int bonusPercent = Mathf.RoundToInt(yaku.BonusMultiplier * 100f);

            yield return TypeText(_yakuNameTexts[i], yaku.Name);

            yield return CountUpText(_yakuBonusTexts[i], 0, bonusPercent, value => $"+ {value}%");

            cumulativeBonus += yaku.BonusMultiplier;
            int nextPercent = Mathf.RoundToInt((1f + cumulativeBonus) * 100f);
            int projectedDamage = Mathf.RoundToInt(baseScoreTotal * (1f + cumulativeBonus));
            int projectedEnemyHp = Mathf.Max(0, enemyHpBeforeBattle - projectedDamage);

            yield return TypeThenCountUpSingle(
                _finalScoreMultiplyText,
                currentPercent,
                nextPercent,
                value => $"* {value}%");

            yield return TypeThenCountUpSingle(
                _finalDamageText,
                currentDamage,
                projectedDamage,
                value => value.ToString());

            yield return TypeThenCountUpSingle(
                _enemyRemainHpText,
                currentEnemyHp,
                projectedEnemyHp,
                value => value.ToString());



            currentPercent = nextPercent;
            currentDamage = projectedDamage;
            currentEnemyHp = projectedEnemyHp;
        }

        if (currentPercent != finalPercent || currentDamage != finalDamage || currentEnemyHp != Mathf.Max(0, enemyRemainHp))
        {
            yield return TypeThenCountUpSingle(
                _finalScoreMultiplyText,
                currentPercent,
                finalPercent,
                value => $"* {value}%");

            yield return TypeThenCountUpSingle(
                _finalDamageText,
                currentDamage,
                finalDamage,
                value => value.ToString());

            yield return TypeThenCountUpSingle(
                _enemyRemainHpText,
                currentEnemyHp,
                Mathf.Max(0, enemyRemainHp),
                value => value.ToString());



            currentPercent = finalPercent;
            currentDamage = finalDamage;
            currentEnemyHp = Mathf.Max(0, enemyRemainHp);
        }

        yield return TypeText(_enemyDamageTitleText, "적 피해");
        yield return TypeText(_enemyDamageToPlayerText, $"-{Mathf.Max(0, enemyDamageToPlayer)}");

        yield return TypeText(_playerRemainHpTitleText, "남은 체력");
        yield return CountUpText(_playerRemainHpText, currentPlayerHp, Mathf.Max(0, playerRemainHp), value => value.ToString());

        yield return new WaitForSecondsRealtime(_resultPanelHoldSeconds);

        ClearAllTexts();
        _uiController.HideBattleResultPanelImmediate();
        Time.timeScale = prevTimeScale;
    }


    private List<YakuScoreResult> BuildDisplayYakus(FireScoreResult scoreResult)
    {
        List<YakuScoreResult> display = new List<YakuScoreResult>();
        if (scoreResult == null || scoreResult.YakuResults == null || scoreResult.YakuResults.Count == 0)
        {
            return display;
        }

        int count = Mathf.Min(4, scoreResult.YakuResults.Count);
        for (int i = 0; i < count; i++)
        {
            display.Add(scoreResult.YakuResults[i]);
        }

        return display;
    }

    private void GetComboSummary(
        FireScoreResult scoreResult,
        out int sequenceCount,
        out int tripleCount,
        out int sequenceScore,
        out int tripleScore)
    {
        sequenceCount = 0;
        tripleCount = 0;
        sequenceScore = 1000;
        tripleScore = 1500;

        if (scoreResult == null || scoreResult.SquadResults == null)
        {
            return;
        }

        for (int i = 0; i < scoreResult.SquadResults.Count; i++)
        {
            SquadScoreResult squad = scoreResult.SquadResults[i];
            if (!squad.IsValid)
            {
                continue;
            }

            if (squad.ComboType == SquadComboType.Sequence)
            {
                sequenceCount++;
                sequenceScore = squad.BaseScore;
            }
            else if (squad.ComboType == SquadComboType.Triple)
            {
                tripleCount++;
                tripleScore = squad.BaseScore;
            }
        }
    }
private void ClearAllTexts()
{
    _baseComboTitleText.text = string.Empty;
    _baseComboScoreText.text = string.Empty;
    _advanceComboScoreText.text = string.Empty;
    _finalScoreMultiplyText.text = string.Empty;
    _finalDamageText.text = string.Empty;
    _enemyRemainHpTitleText.text = string.Empty;
    _enemyRemainHpText.text = string.Empty;
    _enemyDamageTitleText.text = string.Empty;

    _enemyDamageToPlayerText.text = string.Empty;
    _playerRemainHpTitleText.text = string.Empty;
    _playerRemainHpText.text = string.Empty;

    int lineCount = Mathf.Min(_yakuNameTexts.Length, _yakuBonusTexts.Length);
    for (int i = 0; i < lineCount; i++)
    {
        _yakuNameTexts[i].text = string.Empty;
        _yakuBonusTexts[i].text = string.Empty;
    }
}

private IEnumerator CountUpQuad(
    TMP_Text firstText,
    TMP_Text secondText,
    TMP_Text thirdText,
    TMP_Text fourthText,
    int firstFrom,
    int firstTo,
    int secondFrom,
    int secondTo,
    int thirdFrom,
    int thirdTo,
    int fourthFrom,
    int fourthTo,
    Func<int, string> firstFormatter,
    Func<int, string> secondFormatter,
    Func<int, string> thirdFormatter,
    Func<int, string> fourthFormatter)
    {
        if (_countUpDuration <= 0f)
        {
            firstText.text = firstFormatter(firstTo);
            secondText.text = secondFormatter(secondTo);
            thirdText.text = thirdFormatter(thirdTo);
            fourthText.text = fourthFormatter(fourthTo);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < _countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _countUpDuration);

            int first = Mathf.RoundToInt(Mathf.Lerp(firstFrom, firstTo, t));
            int second = Mathf.RoundToInt(Mathf.Lerp(secondFrom, secondTo, t));
            int third = Mathf.RoundToInt(Mathf.Lerp(thirdFrom, thirdTo, t));
            int fourth = Mathf.RoundToInt(Mathf.Lerp(fourthFrom, fourthTo, t));

            firstText.text = firstFormatter(first);
            secondText.text = secondFormatter(second);
            thirdText.text = thirdFormatter(third);
            fourthText.text = fourthFormatter(fourth);

            yield return null;
        }

        firstText.text = firstFormatter(firstTo);
        secondText.text = secondFormatter(secondTo);
        thirdText.text = thirdFormatter(thirdTo);
        fourthText.text = fourthFormatter(fourthTo);
    }

    private IEnumerator TypeText(TMP_Text target, string value)
    {
        target.text = string.Empty;

        for (int i = 0; i < value.Length; i++)
        {
            target.text += value[i];

            if (_charTypeInterval <= 0f)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSecondsRealtime(_charTypeInterval);
            }
        }
    }

    private IEnumerator CountUpText(TMP_Text target, int from, int to, Func<int, string> formatter)
    {
        if (from == to || _countUpDuration <= 0f)
        {
            target.text = formatter(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < _countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _countUpDuration);
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            target.text = formatter(value);
            yield return null;
        }

        target.text = formatter(to);
    }

    private IEnumerator CountUpTriple(
        TMP_Text leftText,
        TMP_Text middleText,
        TMP_Text rightText,
        int leftFrom,
        int leftTo,
        int middleFrom,
        int middleTo,
        int rightFrom,
        int rightTo,
        Func<int, string> leftFormatter,
        Func<int, string> middleFormatter,
        Func<int, string> rightFormatter)
    {
        if ((leftFrom == leftTo && middleFrom == middleTo && rightFrom == rightTo) || _countUpDuration <= 0f)
        {
            leftText.text = leftFormatter(leftTo);
            middleText.text = middleFormatter(middleTo);
            rightText.text = rightFormatter(rightTo);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < _countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _countUpDuration);

            int leftValue = Mathf.RoundToInt(Mathf.Lerp(leftFrom, leftTo, t));
            int middleValue = Mathf.RoundToInt(Mathf.Lerp(middleFrom, middleTo, t));
            int rightValue = Mathf.RoundToInt(Mathf.Lerp(rightFrom, rightTo, t));

            leftText.text = leftFormatter(leftValue);
            middleText.text = middleFormatter(middleValue);
            rightText.text = rightFormatter(rightValue);

            yield return null;
        }

        leftText.text = leftFormatter(leftTo);
        middleText.text = middleFormatter(middleTo);
        rightText.text = rightFormatter(rightTo);
    }

    private IEnumerator TypeThenCountUpSingle(
    TMP_Text targetText,
    int from,
    int to,
    Func<int, string> formatter)
    {
        yield return TypeText(targetText, formatter(from));

        if (from != to)
        {
            yield return CountUpText(targetText, from, to, formatter);
        }
    }



    
}
