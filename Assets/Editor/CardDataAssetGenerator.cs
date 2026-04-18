#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CardDataAssetGenerator
{
    private const string TargetFolder = "Assets/01.Scripts/Card/CardSO";

    [MenuItem("Tsumo/CardData/Create 27 Cards (Type x 1~9)")]
    private static void CreateAllCardDataAssets()
    {
        EnsureFolder(TargetFolder);

        int createdCount = 0;
        int skippedCount = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (CardType cardType in System.Enum.GetValues(typeof(CardType)))
            {
                for (int number = 1; number <= 9; number++)
                {
                    string path = $"{TargetFolder}/{cardType}_{number}.asset";

                    if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
                    {
                        skippedCount++;
                        continue;
                    }

                    CardData asset = ScriptableObject.CreateInstance<CardData>();
                    SerializedObject serializedObject = new SerializedObject(asset);

                    SerializedProperty typeProperty = serializedObject.FindProperty("_type");
                    SerializedProperty numberProperty = serializedObject.FindProperty("_number");

                    if (typeProperty == null || numberProperty == null)
                    {
                        Object.DestroyImmediate(asset);
                        Debug.LogError("CardData 필드명을 찾지 못했습니다. (_type / _number 확인)");
                        return;
                    }

                    typeProperty.enumValueIndex = (int)cardType;
                    numberProperty.intValue = number;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();

                    AssetDatabase.CreateAsset(asset, path);
                    createdCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"CardData 생성 완료: Created={createdCount}, Skipped={skippedCount}");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] split = folderPath.Split('/');
        string current = split[0];

        for (int i = 1; i < split.Length; i++)
        {
            string next = $"{current}/{split[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, split[i]);
            }

            current = next;
        }
    }
}
#endif
