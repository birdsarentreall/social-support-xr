using System.Collections.Generic;
using UnityEngine;

public class EncounterSequence : MonoBehaviour
{
    public static EncounterSequence Instance;

    [Header("Encounter order (runtime)")]
    public List<EncounterDef> encounters = new();

    [Header("Optional overworld progression")]
    public List<GameObject> overworldRoots = new();
    public bool advanceOverworldAfterEachBattle = false;

    int encounterIndex = 0;
    int overworldIndex = 0;

    [Header("Study orders (12 permutations)")]
    [Range(0, 11)]
    public int studyOrderIndex = 0;

    public List<EncounterDef> order0 = new();
    public List<EncounterDef> order1 = new();
    public List<EncounterDef> order2 = new();
    public List<EncounterDef> order3 = new();
    public List<EncounterDef> order4 = new();
    public List<EncounterDef> order5 = new();
    public List<EncounterDef> order6 = new();
    public List<EncounterDef> order7 = new();
    public List<EncounterDef> order8 = new();
    public List<EncounterDef> order9 = new();
    public List<EncounterDef> order10 = new();
    public List<EncounterDef> order11 = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SelectStudyOrder();     // uses studyOrderIndex from Inspector
        ActivateOverworld(0);
    }

    void SelectStudyOrder()
    {
        int i = Mathf.Clamp(studyOrderIndex, 0, 11);

        encounters = i switch
        {
            0 => order0,
            1 => order1,
            2 => order2,
            3 => order3,
            4 => order4,
            5 => order5,
            6 => order6,
            7 => order7,
            8 => order8,
            9 => order9,
            10 => order10,
            _ => order11
        };

        encounterIndex = 0;

        if (encounters == null || encounters.Count != 12)
            Debug.LogWarning($"[Study] Order {i} has {encounters?.Count ?? 0} encounters (expected 12).");

        Debug.Log($"[Study] Using order {i}");
    }

    public EncounterDef GetNextEncounter()
    {
        if (encounters == null || encounters.Count == 0)
        {
            Debug.LogError("EncounterSequence: encounters list is empty.");
            return null;
        }

        if (encounterIndex >= encounters.Count)
        {
            Debug.Log("EncounterSequence: no more encounters.");
            return null;
        }

        return encounters[encounterIndex++];
    }

    public void OnBattleFinished(bool playerWon)
    {
        if (!advanceOverworldAfterEachBattle) return;

        int next = overworldIndex + 1;
        if (next < overworldRoots.Count)
            ActivateOverworld(next);
    }

    void ActivateOverworld(int index)
    {
        overworldIndex = Mathf.Clamp(index, 0, overworldRoots.Count - 1);

        for (int i = 0; i < overworldRoots.Count; i++)
            overworldRoots[i].SetActive(i == overworldIndex);
    }
}
