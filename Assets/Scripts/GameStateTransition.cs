using UnityEngine;

public class GameStateTransition : MonoBehaviour
{
    public static GameStateTransition Instance;

    [Header("Roots")]
    public GameObject gameScreen;
    public GameObject battleScreen;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gameScreen != null) gameScreen.SetActive(true);
        if (battleScreen != null) battleScreen.SetActive(false);
    }

    public void StartBattle()
    {
        PauseController.SetPause(true);
        if (gameScreen != null) gameScreen.SetActive(false);
        if (battleScreen != null) battleScreen.SetActive(true);
    }

    public void EndBattle(bool playerWon)
    {
        if (battleScreen != null) battleScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);
        PauseController.SetPause(false);
    }
}
