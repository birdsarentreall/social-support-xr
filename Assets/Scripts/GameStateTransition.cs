using UnityEngine;
using Unity.Cinemachine;

public class GameStateTransition : MonoBehaviour
{
    public static GameStateTransition Instance;

    [Header("Roots")]
    public GameObject gameScreen;
    public GameObject battleScreen;

    [Header("Camera")]
    public Camera mainCam;
    public CinemachineBrain brain;
    public Transform battleCameraAnchor; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (brain == null && mainCam != null) brain = mainCam.GetComponent<CinemachineBrain>();

        if (gameScreen != null) gameScreen.SetActive(true);
        if (battleScreen != null) battleScreen.SetActive(false);
    }

    public void StartBattle()
    {
        PauseController.SetPause(true);

        if (brain != null) brain.enabled = false;

        if (mainCam != null && battleCameraAnchor != null)
        {
            mainCam.transform.SetPositionAndRotation(
                battleCameraAnchor.position,
                battleCameraAnchor.rotation
            );
        }
        else
        {
            Debug.LogWarning("GameStateTransition: mainCam or battleCameraAnchor not set.");
        }

        if (gameScreen != null) gameScreen.SetActive(false);
        if (battleScreen != null) battleScreen.SetActive(true);
    }

    public void EndBattle(bool playerWon)
    {
        if (battleScreen != null) battleScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);

        if (brain != null) brain.enabled = true;

        PauseController.SetPause(false);
    }
}
