using System.Collections.Generic;
using UnityEngine;

public class EncounterSequence : MonoBehaviour
{
    public static EncounterSequence Instance;

    [Header("Encounter order")]
    public List<EncounterDef> encounters = new();

    [Header("Optional overworld progression")]
    public List<GameObject> overworldRoots = new(); // Overworld1, Overworld2, ...
    public bool advanceOverworldAfterEachBattle = false;

    int encounterIndex = 0;
    int overworldIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ActivateOverworld(0);
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

