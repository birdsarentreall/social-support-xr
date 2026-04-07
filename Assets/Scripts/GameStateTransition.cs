using UnityEngine;
using Unity.Cinemachine;

public class GameStateTransition : MonoBehaviour
{
    public static GameStateTransition Instance;

    [Header("Spawnpoint")]
    public Transform player;
    public Transform spawnPoint;

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
        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<Collider2D>();
        Vector2 target = (Vector2)spawnPoint.position;

        if (battleScreen != null) battleScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);

        if (col != null) col.enabled = false;

        if (player != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = target;
        }
        
        Physics2D.SyncTransforms();
        if (col != null) col.enabled = true;

        if (brain != null) brain.enabled = true;

        PauseController.SetPause(false);
    }
}
